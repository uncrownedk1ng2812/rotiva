using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Rotiva.Conversion;

namespace Rotiva.Desktop;

/// <summary>Rotiva lintas platform: hidup di tray/menu bar, roda dibuka untuk file yang diberikan.</summary>
sealed class App : Application
{
    const string WebsiteUrl = "https://uncrownedk1ng2812.github.io/rotiva/";
    const string GitHubUrl = "https://github.com/uncrownedk1ng2812/rotiva";
    const string KofiUrl = "https://ko-fi.com/denishchairany";
    const string SociaBuzzUrl = "https://sociabuzz.com/kianaldrich";
    const string LicenseUrl = "https://github.com/uncrownedk1ng2812/rotiva/blob/main/NOTICE";

    /// <summary>File dari baris perintah saat Rotiva pertama kali dijalankan.</summary>
    public static IReadOnlyList<string> StartupFiles { get; set; } = [];

    readonly ConversionQueue _conversions = new();
    DesktopSettings _settings = new();
    ToastWindow? _toast;
    WheelWindow? _wheel;
    DragShiftGesture? _gesture;

    public override void Initialize()
    {
        Name = "Rotiva";
        RequestedThemeVariant = ThemeVariant.Dark;
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _settings = DesktopSettings.Load();
        // TEXT (OCR) muncul di roda kalau Tesseract terpasang.
        Converter.TextRecognizer = TesseractTextRecognizer.TryCreate();
        _toast = new ToastWindow();
        _wheel = new WheelWindow();
        _wheel.ConvertRequested += (files, target) => _conversions.Enqueue(files, target, _toast);

        // Gesture drag + Shift (Windows memakai aplikasi WPF-nya sendiri).
        var wheel = _wheel;
        _gesture = DragShiftGesture.TryCreate(() => wheel.Screens.All);
        if (_gesture is not null)
        {
            _gesture.ShiftPressedDuringDrag += cursor => wheel.BeginDrag(cursor, _settings.WheelScale);
            _gesture.DragEnded += wheel.EndDrag;
            _gesture.Enabled = _settings.DragShiftGesture;
        }

        CreateTrayIcon();
        if (OperatingSystem.IsLinux() && _settings.FileManagerMenu)
            LinuxIntegration.Register();
        InstanceChannel.Listen(OpenWheel);
        ListenForFileActivation();
        if (StartupFiles.Count > 0)
            OpenWheel(StartupFiles);
        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// macOS: Finder ("Open With → Rotiva", menyeret file ke ikon app) mengirim file lewat Apple Event,
    /// bukan argumen baris perintah. Avalonia meneruskannya sebagai aktivasi file.
    /// </summary>
    void ListenForFileActivation()
    {
        if (this.TryGetFeature<IActivatableLifetime>() is not { } activatable)
            return;
        activatable.Activated += (_, e) =>
        {
            if (e is not FileActivatedEventArgs { Files: { Count: > 0 } items })
                return;
            var files = items.Select(item => item.TryGetLocalPath()).OfType<string>().ToList();
            if (files.Count > 0)
                OpenWheel(files);
        };
    }

    void OpenWheel(IReadOnlyList<string> files) => _wheel!.Open(files, _settings.WheelScale);

    void CreateTrayIcon()
    {
        var menu = new NativeMenu();
        menu.Items.Add(Item("Convert files…", PickFiles));
        menu.Items.Add(new NativeMenuItemSeparator());
        if (_gesture is not null)
        {
            menu.Items.Add(Toggle("Drag + Shift opens the wheel", _settings.DragShiftGesture, enabled =>
            {
                _settings.DragShiftGesture = enabled;
                _gesture.Enabled = enabled;
            }));
        }
        menu.Items.Add(new NativeMenuItem("Wheel size") { Menu = WheelSizeMenu() });
        if (OperatingSystem.IsLinux())
            menu.Items.Add(FileManagerMenuItem());
        menu.Items.Add(new NativeMenuItemSeparator());
        menu.Items.Add(Item("Website", () => Shell.OpenUrl(WebsiteUrl)));
        menu.Items.Add(Item("GitHub", () => Shell.OpenUrl(GitHubUrl)));
        menu.Items.Add(new NativeMenuItem("Support Rotiva")
        {
            Menu = new NativeMenu
            {
                Item("Ko-fi", () => Shell.OpenUrl(KofiUrl)),
                Item("SociaBuzz (Indonesia)", () => Shell.OpenUrl(SociaBuzzUrl)),
            },
        });
        menu.Items.Add(Item("License & copyright", () => Shell.OpenUrl(LicenseUrl)));
        menu.Items.Add(new NativeMenuItemSeparator());
        menu.Items.Add(Item("Quit Rotiva", Quit));

        var tray = new TrayIcon
        {
            Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://Rotiva/Assets/icon.png"))),
            ToolTipText = "Rotiva",
            Menu = menu,
        };
        TrayIcon.SetIcons(this, [tray]);
    }

    NativeMenu WheelSizeMenu()
    {
        var sizeMenu = new NativeMenu();
        foreach (var (size, label) in new[] { (WheelSize.Small, "Small"), (WheelSize.Medium, "Medium"), (WheelSize.Large, "Large") })
        {
            var item = new NativeMenuItem(label)
            {
                ToggleType = NativeMenuItemToggleType.Radio,
                IsChecked = _settings.WheelSize == size,
            };
            item.Click += (_, _) =>
            {
                _settings.WheelSize = size;
                _settings.Save();
                foreach (var other in sizeMenu.Items.OfType<NativeMenuItem>())
                    other.IsChecked = other == item;
            };
            sizeMenu.Items.Add(item);
        }
        return sizeMenu;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("linux")]
    NativeMenuItem FileManagerMenuItem() => Toggle("File manager menu", _settings.FileManagerMenu, enabled =>
    {
        _settings.FileManagerMenu = enabled;
        if (enabled)
            LinuxIntegration.Register();
        else
            LinuxIntegration.Unregister();
    });

    /// <summary>Pilihan nyala/mati di menu tray; <paramref name="changed"/> lalu pengaturan disimpan.</summary>
    NativeMenuItem Toggle(string header, bool initial, Action<bool> changed)
    {
        var item = new NativeMenuItem(header) { ToggleType = NativeMenuItemToggleType.CheckBox, IsChecked = initial };
        item.Click += (_, _) =>
        {
            item.IsChecked = !item.IsChecked;
            changed(item.IsChecked);
            _settings.Save();
        };
        return item;
    }

    static NativeMenuItem Item(string header, Action action)
    {
        var item = new NativeMenuItem(header);
        item.Click += (_, _) => action();
        return item;
    }

    /// <summary>
    /// Pilih file lewat dialog OS lalu buka roda. Dialog butuh jendela induk yang terlihat,
    /// jadi dipakai jendela kecil sementara di tengah layar.
    /// </summary>
    async void PickFiles()
    {
        var host = new Window
        {
            Title = "Rotiva",
            Width = 1,
            Height = 1,
            SystemDecorations = SystemDecorations.None,
            ShowInTaskbar = false,
            Background = Brushes.Transparent,
            TransparencyLevelHint = [WindowTransparencyLevel.Transparent],
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
        };
        host.Show();
        try
        {
            var picked = await host.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Convert with Rotiva",
                AllowMultiple = true,
            });
            var files = picked.Select(file => file.TryGetLocalPath()).OfType<string>().ToList();
            if (files.Count > 0)
                OpenWheel(files);
        }
        finally
        {
            host.Close();
        }
    }

    void Quit()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }
}
