using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Rotiva.Controls;
using Rotiva.Interop;
using Rotiva.Localization;

namespace Rotiva;

/// <summary>
/// Menu tray bergaya Rotiva (gelap, aksen oranye, sudut membulat) sebagai pengganti
/// ContextMenuStrip WinForms yang tampilannya kuno. Menutup sendiri saat kehilangan fokus.
/// </summary>
internal sealed class TrayMenuWindow : Window
{
    const string WebsiteUrl = "https://uncrownedk1ng2812.github.io/rotiva/";
    const string GitHubUrl = "https://github.com/uncrownedk1ng2812/rotiva";
    const string KofiUrl = "https://ko-fi.com/denishchairany";
    const string SociaBuzzUrl = "https://sociabuzz.com/kianaldrich";
    const double MenuWidth = 292;
    const double ShadowMargin = 14;

    static readonly FontFamily TextFont = new("Segoe UI Variable Text, Segoe UI");
    static readonly Brush TextBrush = Frozen(Color.FromArgb(0xF2, 0xFF, 0xFF, 0xFF));
    static readonly Brush MutedBrush = Frozen(Color.FromArgb(0x8C, 0xFF, 0xFF, 0xFF));
    static readonly Brush HoverBrush = Frozen(Color.FromArgb(0x1A, 0xFF, 0xFF, 0xFF));
    static readonly Brush DividerBrush = Frozen(Color.FromArgb(0x1F, 0xFF, 0xFF, 0xFF));
    static readonly Brush AccentBrush = Frozen(RadialWheel.AccentColor);
    static readonly Brush OffTrackBrush = Frozen(Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF));

    static TrayMenuWindow? _open;

    readonly Action _onExit;
    bool _closing;

    TrayMenuWindow(Action onExit)
    {
        _onExit = onExit;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        ResizeMode = ResizeMode.NoResize;
        Topmost = true;
        ShowInTaskbar = false;
        Title = "Rotiva";

        var panel = new Border
        {
            Margin = new Thickness(ShadowMargin),
            Padding = new Thickness(5),
            CornerRadius = new CornerRadius(8),
            Background = Frozen(Color.FromArgb(0xF7, 0x16, 0x16, 0x1A)),
            BorderBrush = Frozen(Color.FromArgb(0x2E, 0xFF, 0xFF, 0xFF)),
            BorderThickness = new Thickness(1),
            // Bayangan statis: hanya digambar sekali saat menu dibuka.
            Effect = new DropShadowEffect { BlurRadius = 18, ShadowDepth = 3, Opacity = 0.45 },
            Child = Items(),
            RenderTransform = new TranslateTransform(),
        };
        Content = panel;

        Deactivated += (_, _) => CloseMenu();
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
                CloseMenu();
        };
        Closed += (_, _) => _open = null;
    }

    /// <summary>Buka menu di dekat kursor (biasanya di atas ikon tray); klik kedua menutupnya.</summary>
    public static void ShowAtCursor(Action onExit)
    {
        if (_open is not null)
        {
            _open.CloseMenu();
            return;
        }

        var menu = new TrayMenuWindow(onExit);
        _open = menu;
        var content = (FrameworkElement)menu.Content;
        content.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var size = content.DesiredSize;
        menu.Width = size.Width;
        menu.Height = size.Height;

        // HWND dibuat dulu supaya DPI monitor bisa dibaca sebelum menu diposisikan.
        new WindowInteropHelper(menu).EnsureHandle();
        double dpi = VisualTreeHelper.GetDpi(menu).DpiScaleX;
        NativeMethods.GetCursorPos(out var cursor);
        var work = NativeMethods.GetWorkAreaAtCursor();
        double left = work.X / dpi, top = work.Y / dpi;
        double right = (work.X + work.Width) / dpi, bottom = (work.Y + work.Height) / dpi;
        // Sudut kanan bawah menu di kursor, lalu dijaga tetap di dalam area kerja.
        menu.Left = Math.Clamp(cursor.X / dpi - size.Width + ShadowMargin, left, Math.Max(left, right - size.Width));
        menu.Top = Math.Clamp(cursor.Y / dpi - size.Height + ShadowMargin, top, Math.Max(top, bottom - size.Height));

        menu.Show();
        menu.Activate();
        NativeMethods.SetForegroundWindow(new WindowInteropHelper(menu).Handle);
        menu.PlayOpen();
    }

    StackPanel Items()
    {
        var items = new StackPanel { Width = MenuWidth };
        items.Children.Add(Header());
        items.Children.Add(Divider());
        items.Children.Add(ToggleRow("", L.T("Start with Windows"), TrayIcon.IsAutostartEnabled(), TrayIcon.SetAutostart));
        items.Children.Add(ToggleRow("", L.T("Explorer right-click menu"), Shell.ShellMenu.IsEnabled,
            enabled => Shell.ShellMenu.IsEnabled = enabled));
        items.Children.Add(ChoiceRow("", L.T("Wheel size"),
            [(WheelSize.Small, "S", L.T("Small wheel")), (WheelSize.Medium, "M", L.T("Medium wheel")), (WheelSize.Large, "L", L.T("Large wheel"))],
            () => AppSettings.WheelSize, size => AppSettings.WheelSize = size));
        items.Children.Add(ChoiceRow("", L.T("Language"),
            [("", "AUTO", L.T("Same as Windows")), .. L.Languages.Select(l => (l.Code, l.Code.ToUpperInvariant(), l.Name))],
            () => AppSettings.Language, ChangeLanguage));
        items.Children.Add(Divider());
        items.Children.Add(ActionRow("", L.T("Website"), () => Open(WebsiteUrl)));
        items.Children.Add(ActionRow("", "GitHub", () => Open(GitHubUrl)));
        items.Children.Add(SupportRow());
        items.Children.Add(ActionRow("", L.T("License & copyright"), TrayIcon.OpenLicense));
        items.Children.Add(Divider());
        items.Children.Add(ActionRow("", L.T("Exit Rotiva"), _onExit));
        items.Children.Add(new TextBlock
        {
            Text = "© 2026 Denish Chairany · GPL-3.0",
            FontFamily = TextFont,
            FontSize = 11,
            Foreground = MutedBrush,
            Margin = new Thickness(12, 6, 12, 4),
        });
        return items;
    }

    /// <summary>Ganti bahasa, lalu bangun ulang isi menu di tempat (sudut kanan bawah tetap).</summary>
    void ChangeLanguage(string code)
    {
        AppSettings.Language = code;
        L.Use(code);
        // Menu klik kanan Explorer ditulis dalam bahasa yang sedang dipakai.
        if (Shell.ShellMenu.IsEnabled)
            Shell.ShellMenu.Register();

        var panel = (Border)Content;
        double right = Left + Width, bottom = Top + Height;
        panel.Child = Items();
        panel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        Width = panel.DesiredSize.Width;
        Height = panel.DesiredSize.Height;
        Left = right - Width;
        Top = bottom - Height;
    }

    void PlayOpen()
    {
        var panel = (Border)Content;
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
        var duration = TimeSpan.FromMilliseconds(140);
        panel.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, duration));
        ((TranslateTransform)panel.RenderTransform).BeginAnimation(TranslateTransform.YProperty,
            new DoubleAnimation(8, 0, duration) { EasingFunction = ease });
    }

    void CloseMenu()
    {
        if (_closing)
            return;
        _closing = true;
        Close();
    }

    static UIElement Header()
    {
        var icon = new Image
        {
            Width = 28,
            Height = 28,
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Source = AppIcon(),
        };
        var title = new TextBlock
        {
            Text = "ROTIVA",
            FontFamily = RadialWheel.LabelFont,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = TextBrush,
        };
        var version = new TextBlock
        {
            Text = "  v" + (Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "?"),
            FontFamily = RadialWheel.LabelFont,
            FontSize = 12,
            Foreground = AccentBrush,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 2),
        };
        var titleLine = new StackPanel { Orientation = Orientation.Horizontal };
        titleLine.Children.Add(title);
        titleLine.Children.Add(version);

        var text = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        text.Children.Add(titleLine);
        text.Children.Add(new TextBlock
        {
            Text = L.T("Drag a file, then press Shift"),
            FontFamily = TextFont,
            FontSize = 12,
            Foreground = MutedBrush,
        });

        var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(10, 8, 10, 8) };
        row.Children.Add(icon);
        row.Children.Add(text);
        return row;
    }

    static ImageSource? AppIcon()
    {
        using var icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath!);
        if (icon is null)
            return null;
        var source = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        source.Freeze();
        return source;
    }

    UIElement ActionRow(string glyph, string label, Action action)
    {
        var row = Row(glyph, label, trailing: null);
        row.MouseLeftButtonUp += (_, _) =>
        {
            CloseMenu();
            action();
        };
        return row;
    }

    static UIElement ToggleRow(string glyph, string label, bool initial, Action<bool> changed)
    {
        bool value = initial;
        var knob = new Ellipse { Width = 12, Height = 12, Fill = Brushes.White };
        var knobShift = new TranslateTransform(value ? 8 : -8, 0);
        knob.RenderTransform = knobShift;
        var track = new Border
        {
            Width = 36,
            Height = 20,
            CornerRadius = new CornerRadius(10),
            Background = value ? AccentBrush : OffTrackBrush,
            Child = knob,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var row = Row(glyph, label, track);
        row.MouseLeftButtonUp += (_, _) =>
        {
            value = !value;
            track.Background = value ? AccentBrush : OffTrackBrush;
            knobShift.BeginAnimation(TranslateTransform.XProperty,
                new DoubleAnimation(value ? 8 : -8, TimeSpan.FromMilliseconds(120)) { EasingFunction = new CubicEase() });
            changed(value);
        };
        return row;
    }

    /// <summary>Baris dengan pilihan chip (ukuran roda S/M/L, bahasa AUTO/EN/ID).</summary>
    static UIElement ChoiceRow<T>(string glyph, string label, IEnumerable<(T Value, string Label, string ToolTip)> options,
        Func<T> current, Action<T> choose)
    {
        var choices = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        var chips = new List<(Border Chip, TextBlock Text, T Value)>();

        void Refresh()
        {
            var selected = current();
            foreach (var (chip, text, value) in chips)
            {
                bool active = EqualityComparer<T>.Default.Equals(value, selected);
                chip.Background = active ? AccentBrush : Brushes.Transparent;
                text.Foreground = active ? Brushes.Black : TextBrush;
            }
        }

        foreach (var (value, chipLabel, toolTip) in options)
        {
            var text = new TextBlock
            {
                Text = chipLabel,
                FontFamily = RadialWheel.LabelFont,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            var chip = new Border
            {
                MinWidth = 26,
                Height = 22,
                Padding = new Thickness(4, 0, 4, 0),
                Margin = new Thickness(2, 0, 0, 0),
                CornerRadius = new CornerRadius(4),
                Child = text,
                Cursor = Cursors.Hand,
                ToolTip = toolTip,
            };
            chip.MouseLeftButtonUp += (_, e) =>
            {
                e.Handled = true;
                choose(value);
                Refresh();
            };
            chips.Add((chip, text, value));
            choices.Children.Add(chip);
        }
        Refresh();

        // Baris ini sendiri tidak bisa diklik (hanya chip-nya), jadi tanpa efek hover.
        var row = Row(glyph, label, choices);
        row.Cursor = null;
        return row;
    }

    UIElement SupportRow()
    {
        var links = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        foreach (var (label, url) in new[] { ("Ko-fi", KofiUrl), ("SociaBuzz", SociaBuzzUrl) })
        {
            var chip = new Border
            {
                Margin = new Thickness(4, 0, 0, 0),
                Padding = new Thickness(8, 2, 8, 3),
                CornerRadius = new CornerRadius(4),
                BorderBrush = AccentBrush,
                BorderThickness = new Thickness(1),
                Background = Brushes.Transparent,
                Cursor = Cursors.Hand,
                Child = new TextBlock
                {
                    Text = label,
                    FontFamily = TextFont,
                    FontSize = 11.5,
                    Foreground = AccentBrush,
                },
            };
            chip.MouseEnter += (_, _) => chip.Background = Frozen(Color.FromArgb(0x33, 0xFF, 0x8A, 0x1F));
            chip.MouseLeave += (_, _) => chip.Background = Brushes.Transparent;
            chip.MouseLeftButtonUp += (_, e) =>
            {
                e.Handled = true;
                CloseMenu();
                Open(url);
            };
            links.Children.Add(chip);
        }

        var row = Row("", L.T("Support Rotiva"), links, glyphBrush: AccentBrush);
        // Klik di luar chip membuka Ko-fi (pilihan utama untuk internasional).
        row.MouseLeftButtonUp += (_, _) =>
        {
            CloseMenu();
            Open(KofiUrl);
        };
        return row;
    }

    static Border Row(string glyph, string label, UIElement? trailing, Brush? glyphBrush = null)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var icon = new TextBlock
        {
            Text = glyph,
            FontFamily = RadialWheel.IconFont,
            FontSize = 14,
            Foreground = glyphBrush ?? TextBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var text = new TextBlock
        {
            Text = label,
            FontFamily = TextFont,
            FontSize = 13,
            Foreground = TextBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(text, 1);
        grid.Children.Add(icon);
        grid.Children.Add(text);
        if (trailing is not null)
        {
            Grid.SetColumn(trailing, 2);
            grid.Children.Add(trailing);
        }

        var row = new Border
        {
            MinHeight = 34,
            Padding = new Thickness(10, 4, 10, 4),
            CornerRadius = new CornerRadius(5),
            Background = Brushes.Transparent,
            Child = grid,
            Cursor = Cursors.Hand,
        };
        row.MouseEnter += (_, _) => { if (row.Cursor is not null) row.Background = HoverBrush; };
        row.MouseLeave += (_, _) => row.Background = Brushes.Transparent;
        return row;
    }

    static UIElement Divider() => new Rectangle { Height = 1, Fill = DividerBrush, Margin = new Thickness(8, 4, 8, 4) };

    static void Open(string url) => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });

    static SolidColorBrush Frozen(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
