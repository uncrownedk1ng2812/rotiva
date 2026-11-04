using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Rotiva.Conversion;
using Rotiva.Localization;

namespace Rotiva.Desktop;

/// <summary>
/// Notifikasi di bawah layar untuk progres dan hasil konversi, gaya yang sama dengan versi Windows.
/// Klik hasil yang berhasil membuka folder hasil di file manager.
/// </summary>
sealed class ToastWindow : Window, IConversionReporter
{
    static readonly IBrush AccentBrush = new SolidColorBrush(WheelControl.AccentColor);
    static readonly IBrush ErrorBrush = new SolidColorBrush(Color.FromRgb(0xE5, 0x48, 0x4D));

    readonly Border _panel;
    readonly TextBlock _header = Label(11, FontWeight.Bold);
    readonly TextBlock _message = Label(16, FontWeight.SemiBold);
    readonly TextBlock _revealHint = Label(12, FontWeight.Normal);
    readonly ProgressBar _progress = new() { Height = 3, MinHeight = 3, Margin = new Thickness(0, 10, 0, 0), Foreground = AccentBrush };
    readonly DispatcherTimer _hideTimer = new();
    string? _revealPath;

    public ToastWindow()
    {
        Title = "Rotiva";
        SystemDecorations = SystemDecorations.None;
        TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
        Background = Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;
        ShowActivated = false;
        CanResize = false;

        _message.MaxWidth = 560;
        _message.TextTrimming = TextTrimming.CharacterEllipsis;
        _revealHint.Foreground = new SolidColorBrush(Color.FromArgb(0x99, 0xFF, 0xFF, 0xFF));
        _panel = new Border
        {
            Margin = new Thickness(16),
            Padding = new Thickness(18, 12, 26, 12),
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(Color.FromArgb(0xF0, 0x0B, 0x0B, 0x0D)),
            BorderThickness = new Thickness(4, 0, 0, 0),
            MinWidth = 340,
            Child = new StackPanel { Children = { _header, _message, _revealHint, _progress } },
        };
        Content = _panel;

        _hideTimer.Tick += (_, _) =>
        {
            _hideTimer.Stop();
            Dismiss();
        };
        PointerPressed += (_, _) => RevealResult();
        // Hover menahan notifikasi hasil.
        PointerEntered += (_, _) => { if (_revealPath is not null) _hideTimer.Stop(); };
        PointerExited += (_, _) => { if (_revealPath is not null) HideAfter(TimeSpan.FromMilliseconds(1500)); };
    }

    public void ShowProgress(string header, string message)
    {
        _hideTimer.Stop();
        SetContent(AccentBrush, header, message, revealPath: null);
        _progress.IsVisible = true;
        SetProgress(null);
        Present();
    }

    public void SetProgress(double? progress)
    {
        _progress.IsIndeterminate = progress is null;
        _progress.Value = (progress ?? 0) * 100;
    }

    public void ShowResult(bool success, string header, string message, string? revealPath = null)
    {
        SetContent(success ? AccentBrush : ErrorBrush, header, message, success ? revealPath : null);
        _progress.IsVisible = false;
        Present();
        HideAfter(TimeSpan.FromMilliseconds(!success ? 7000 : _revealPath is null ? 3500 : 6000));
    }

    public bool TryCopyText(string text)
    {
        if (Clipboard is not { } clipboard)
            return false;
        _ = clipboard.SetTextAsync(text);
        return true;
    }

    void SetContent(IBrush accent, string header, string message, string? revealPath)
    {
        _panel.BorderBrush = accent;
        _header.Foreground = accent;
        _header.Text = header;
        _message.Text = message;
        _revealPath = revealPath;
        _revealHint.Text = L.T(OperatingSystem.IsMacOS() ? "Click to show in Finder" : "Click to open folder");
        _revealHint.IsVisible = revealPath is not null;
        Cursor = revealPath is null ? Cursor.Default : new Cursor(StandardCursorType.Hand);
    }

    // Di tengah bawah area kerja layar utama, di atas dock/taskbar.
    void Present()
    {
        _panel.Measure(Size.Infinity);
        var size = _panel.DesiredSize;
        Width = size.Width;
        Height = size.Height;
        if (Screens.Primary is { } screen)
        {
            var area = screen.WorkingArea;
            Position = new PixelPoint(
                area.X + (area.Width - (int)(size.Width * screen.Scaling)) / 2,
                area.Bottom - (int)((size.Height + 44) * screen.Scaling));
        }
        if (!IsVisible)
            Show();
    }

    void HideAfter(TimeSpan delay)
    {
        _hideTimer.Stop();
        _hideTimer.Interval = delay;
        _hideTimer.Start();
    }

    void RevealResult()
    {
        if (_revealPath is not { } path)
            return;
        Shell.Reveal(path);
        Dismiss();
    }

    void Dismiss()
    {
        _hideTimer.Stop();
        _revealPath = null;
        Hide();
    }

    static TextBlock Label(double size, FontWeight weight) => new()
    {
        FontFamily = WheelControl.Font,
        FontSize = size,
        FontWeight = weight,
        Foreground = Brushes.White,
        HorizontalAlignment = HorizontalAlignment.Left,
    };
}
