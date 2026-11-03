using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Rotiva.Conversion;
using Rotiva.Models;

namespace Rotiva.Desktop;

/// <summary>
/// Jendela transparan seukuran roda. Mode klik: arahkan kursor ke format lalu klik
/// (atau panah + Enter); klik di tengah, Esc, atau klik di luar roda membatalkan.
/// </summary>
sealed class WheelWindow : Window
{
    readonly WheelControl _wheel = new();
    readonly TextBlock _hint = new()
    {
        FontFamily = WheelControl.Font,
        FontSize = 13,
        Foreground = new SolidColorBrush(Color.FromArgb(0xD0, 0xFF, 0xFF, 0xFF)),
    };
    IReadOnlyList<string> _files = [];
    FileCategory? _category;

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

        var hintBar = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(0xE6, 0x0B, 0x0B, 0x0D)),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14, 7),
            Margin = new Thickness(0, -20, 0, 8),
            HorizontalAlignment = HorizontalAlignment.Center,
            Child = _hint,
        };
        Content = new StackPanel { Children = { _wheel, hintBar } };

        _wheel.TargetChosen += Choose;
        _wheel.Cancelled += Hide;
        // Klik di luar roda (jendela lain jadi aktif) = batal, seperti menu.
        Deactivated += (_, _) => Hide();
    }

    /// <summary>Roda dipilih: (file yang ikut, format tujuan).</summary>
    public event Action<IReadOnlyList<string>, ConvertTarget>? ConvertRequested;

    public void Open(IReadOnlyList<string> files, double scale)
    {
        _files = files;
        _category = FormatCatalog.ForFile(files[0], multipleFiles: files.Count > 1);
        // TEXT (OCR) hanya kalau OS punya mesin OCR yang terpasang ke Rotiva.Core.
        var targets = _category.Targets
            .Where(t => t.Action != TargetAction.ExtractText || Converter.TextRecognizer is not null)
            .ToList();
        _wheel.Load(_category, targets, scale);

        string what = files.Count == 1 ? Path.GetFileName(files[0].TrimEnd('/', '\\')) : $"{files.Count} items";
        _hint.Text = $"{what}  ·  click a format  ·  Esc to cancel";

        var position = PlaceAtCursor(scale);
        Show();
        // Window manager Linux bisa mengabaikan Position yang diatur sebelum jendela tampil.
        if (position is { } p)
            Position = p;
        Activate();
        _wheel.Focus();
    }

    /// <summary>
    /// Titik tengah roda di kursor (tempat file dipilih atau dilepas), digeser seperlunya supaya
    /// tidak terpotong pinggir layar. Kalau posisi kursor tidak terbaca: tengah layar.
    /// </summary>
    PixelPoint? PlaceAtCursor(double scale)
    {
        if (CursorPosition.Get(Screens.All) is not { } cursor || Screens.ScreenFromPoint(cursor) is not { } screen)
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
            Math.Clamp(cursor.X - width / 2, area.X, Math.Max(area.X, area.Right - width)),
            Math.Clamp(cursor.Y - wheelCenterY, area.Y, Math.Max(area.Y, area.Bottom - height)));
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
                Hide();
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
        Hide();
        var files = FormatCatalog.FilesFor(target, _category!, _files, NaturalOrder.Instance);
        if (files.Count > 0)
            ConvertRequested?.Invoke(files, target);
    }
}
