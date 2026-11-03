using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Rotiva.Conversion;
using Rotiva.Models;

namespace Rotiva.Desktop;

/// <summary>
/// Jendela transparan seukuran roda, di kursor.
/// Mode klik (file dari "Open With", menu tray, baris perintah): arahkan kursor ke format lalu klik
/// (atau panah + Enter); klik di tengah, Esc, atau klik di luar roda membatalkan.
/// Mode drag (gesture drag + Shift): jendela tak terlihat menunggu drag masuk; roda baru tampil kalau
/// yang di-drag berisi file, arah kursor memilih format, dan melepas file mengonversinya.
/// </summary>
sealed class WheelWindow : Window
{
    // Hampir transparan tapi tidak 0: jendela transparan penuh tidak menerima drop.
    static readonly IBrush DropCatcher = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));

    readonly WheelControl _wheel = new();
    readonly Border _hintBar;
    readonly TextBlock _hint = new()
    {
        FontFamily = WheelControl.Font,
        FontSize = 13,
        Foreground = new SolidColorBrush(Color.FromArgb(0xD0, 0xFF, 0xFF, 0xFF)),
    };
    IReadOnlyList<string> _files = [];
    FileCategory? _category;
    bool _dragMode;
    double _scale = 1;
    bool _wasActivated;
    int _dragVersion;

    public WheelWindow()
    {
        Title = "Rotiva";
        SystemDecorations = SystemDecorations.None;
        TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
        Background = Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;

        _hintBar = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(0xE6, 0x0B, 0x0B, 0x0D)),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14, 7),
            Margin = new Thickness(0, -20, 0, 8),
            HorizontalAlignment = HorizontalAlignment.Center,
            Child = _hint,
        };
        var content = new StackPanel { Children = { _wheel, _hintBar } };
        Content = content;

        DragDrop.SetAllowDrop(content, true);
        content.AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
        content.AddHandler(DragDrop.DragOverEvent, OnDragOver);
        content.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
        content.AddHandler(DragDrop.DropEvent, OnDrop);

        _wheel.TargetChosen += Choose;
        _wheel.Cancelled += Dismiss;
        Activated += (_, _) => _wasActivated = true;
        // Klik di luar roda (jendela lain jadi aktif) = batal, seperti menu. Hanya kalau roda memang
        // pernah aktif: tanpa window manager (atau saat jendela baru muncul) "deactivated" bisa datang
        // lebih dulu, dan roda akan tertutup sebelum terlihat.
        Deactivated += (_, _) =>
        {
            if (_wasActivated && !_dragMode)
                Dismiss();
        };
    }

    /// <summary>Roda dipilih: (file yang ikut, format tujuan).</summary>
    public event Action<IReadOnlyList<string>, ConvertTarget>? ConvertRequested;

    /// <summary>Mode klik untuk file yang sudah diketahui.</summary>
    public void Open(IReadOnlyList<string> files, double scale)
    {
        _dragMode = false;
        _scale = scale;
        Background = Brushes.Transparent;
        Load(files, scale);
        _hint.Text = $"{Describe(files)}  ·  click a format  ·  Esc to cancel";

        _wasActivated = false;
        ShowActivated = true;
        ShowAt(PlaceAt(CursorPosition.Get(Screens.All), scale));
        Activate();
        _wheel.Focus();
    }

    /// <summary>
    /// Mode drag: Shift ditekan saat men-drag. Jendela tak terlihat di kursor menunggu drag masuk;
    /// tidak mengambil fokus dari file manager yang sedang men-drag.
    /// </summary>
    public void BeginDrag(PixelPoint cursor, double scale)
    {
        if (IsVisible)
            return;
        _dragMode = true;
        _scale = scale;
        _files = [];
        _category = null;
        _wheel.Prepare(scale);
        _hintBar.IsVisible = false;
        Background = DropCatcher;
        ShowActivated = false;
        ShowAt(PlaceAt(cursor, scale));
    }

    /// <summary>Tombol mouse dilepas di mana pun: tutup kalau tidak ada drop yang sampai.</summary>
    public async void EndDrag()
    {
        if (!_dragMode || !IsVisible)
            return;
        // Drop diproses sesudah tombol dilepas; tunggu sebentar sebelum menutup.
        int version = _dragVersion;
        await Task.Delay(400);
        if (_dragMode && version == _dragVersion)
            Dismiss();
    }

    void Load(IReadOnlyList<string> files, double scale)
    {
        _files = files;
        _category = FormatCatalog.ForFile(files[0], multipleFiles: files.Count > 1);
        // TEXT (OCR) hanya kalau OS punya mesin OCR yang terpasang ke Rotiva.Core.
        var targets = _category.Targets
            .Where(t => t.Action != TargetAction.ExtractText || Converter.TextRecognizer is not null)
            .ToList();
        _wheel.Load(_category, targets, scale);
        _hintBar.IsVisible = true;
    }

    static string Describe(IReadOnlyList<string> files) =>
        files.Count == 1 ? Path.GetFileName(files[0].TrimEnd('/', '\\')) : $"{files.Count} items";

    void ShowAt(PixelPoint? position)
    {
        Show();
        // Window manager Linux bisa mengabaikan Position yang diatur sebelum jendela tampil.
        if (position is { } p)
            Position = p;
    }

    /// <summary>Tutup roda (batal, selesai, atau drag pergi) dan keluar dari mode drag.</summary>
    void Dismiss()
    {
        _dragMode = false;
        Hide();
    }

    // --- Drag & drop ---

    static IReadOnlyList<string> FilesIn(DragEventArgs e) =>
        e.DataTransfer.TryGetFiles()?.Select(item => item.TryGetLocalPath()).OfType<string>().ToList() ?? [];

    void OnDragEnter(object? sender, DragEventArgs e)
    {
        _dragVersion++;
        if (_dragMode && _category is null)
        {
            var files = FilesIn(e);
            if (files.Count == 0)
            {
                // Bukan file (mis. teks yang di-drag dengan Shift): tidak ada roda.
                Dispatcher.UIThread.Post(Dismiss);
                e.DragEffects = DragDropEffects.None;
                return;
            }
            Load(files, _scale);
            _hint.Text = $"{Describe(files)}  ·  point at a format and let go";
        }
        OnDragOver(sender, e);
    }

    void OnDragOver(object? sender, DragEventArgs e)
    {
        _dragVersion++;
        if (_category is not null)
            _wheel.PointAt(e.GetPosition(_wheel));
        // Selalu Copy: jangan sampai file manager menganggapnya "Move" lalu menghapus file asli.
        e.DragEffects = _category is not null && _wheel.Selected is not null ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    async void OnDragLeave(object? sender, DragEventArgs e)
    {
        // Juga terpanggil saat drag dibatalkan (Esc). Tunggu sebentar: kursor bisa masuk lagi.
        int version = ++_dragVersion;
        await Task.Delay(80);
        if (version == _dragVersion && _dragMode)
            Dismiss();
    }

    void OnDrop(object? sender, DragEventArgs e)
    {
        _dragVersion++;
        e.DragEffects = DragDropEffects.Copy;
        e.Handled = true;
        if (_category is not null)
            _wheel.PointAt(e.GetPosition(_wheel));
        if (_wheel.Selected is { } target)
            Choose(target);
        else
            Dismiss();
    }


    /// <summary>
    /// Titik tengah roda di <paramref name="cursor"/> (tempat file dipilih, dilepas, atau di-drag),
    /// digeser seperlunya supaya tidak terpotong pinggir layar. Tanpa posisi kursor: tengah layar.
    /// </summary>
    PixelPoint? PlaceAt(PixelPoint? cursor, double scale)
    {
        var screen = cursor is { } c ? Screens.ScreenFromPoint(c) : null;
        if (Environment.GetEnvironmentVariable("ROTIVA_DEBUG") == "1")
        {
            Console.Error.WriteLine($"[rotiva] cursor={cursor?.ToString() ?? "null"} screen={screen?.Bounds.ToString() ?? "null"} " +
                $"screens={string.Join("; ", Screens.All.Select(s => $"{s.Bounds} x{s.Scaling} work={s.WorkingArea}"))}");
        }
        if (cursor is not { } point || screen is null)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            return null;
        }

        var content = (Control)Content!;
        content.Measure(Size.Infinity);
        double k = screen.Scaling;
        int width = (int)(content.DesiredSize.Width * k);
        int height = (int)(content.DesiredSize.Height * k);
        // Roda ada di bagian atas jendela, bar petunjuk di bawahnya.
        int wheelCenterY = (int)(WheelControl.DesignSize * scale / 2 * k);
        var area = screen.WorkingArea;
        var position = new PixelPoint(
            Math.Clamp(point.X - width / 2, area.X, Math.Max(area.X, area.Right - width)),
            Math.Clamp(point.Y - wheelCenterY, area.Y, Math.Max(area.Y, area.Bottom - height)));
        WindowStartupLocation = WindowStartupLocation.Manual;
        Position = position;
        return position;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        switch (e.Key)
        {
            case Key.Escape:
                Dismiss();
                break;
            case Key.Left or Key.Up:
                _wheel.StepSelection(-1);
                break;
            case Key.Right or Key.Down or Key.Tab:
                _wheel.StepSelection(1);
                break;
            case Key.Enter or Key.Space:
                _wheel.ChooseSelected();
                break;
            default:
                return;
        }
        e.Handled = true;
    }

    void Choose(ConvertTarget target)
    {
        var category = _category!;
        var files = _files;
        Dismiss();
        var chosen = FormatCatalog.FilesFor(target, category, files, NaturalOrder.Instance);
        if (chosen.Count > 0)
            ConvertRequested?.Invoke(chosen, target);
    }
}
