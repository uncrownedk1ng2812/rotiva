using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Rotiva.Interop;

namespace Rotiva;

/// <summary>
/// Notifikasi di bawah layar untuk progres dan hasil konversi.
/// Klik tembus dan tidak mengambil fokus.
/// </summary>
public partial class ToastWindow : Window
{
    static readonly Brush AccentBrush = Frozen(Color.FromRgb(0xFF, 0x8A, 0x1F));
    static readonly Brush ErrorBrush = Frozen(Color.FromRgb(0xE5, 0x48, 0x4D));
    const string ProgressGlyph = "";
    const string SuccessGlyph = "";
    const string ErrorGlyph = "";

    Rect _area;
    int _version;
    bool _visible;
    string? _revealPath;

    public ToastWindow()
    {
        InitializeComponent();
        SourceInitialized += (_, _) =>
            NativeMethods.MakeOverlayWindow(new WindowInteropHelper(this).Handle, clickThrough: true);

        // Hanya aktif saat hasil bisa dibuka (jendela tidak klik tembus); hover menahan notifikasi.
        MouseEnter += (_, _) =>
        {
            if (_revealPath is null)
                return;
            Panel.BeginAnimation(OpacityProperty, null);
            Panel.Opacity = 1;
        };
        MouseLeave += (_, _) =>
        {
            if (_revealPath is not null)
                FadeOut(_version, TimeSpan.FromMilliseconds(1500));
        };
        MouseLeftButtonUp += (_, _) => RevealResult();
    }

    /// <summary>Tampilkan progres (tetap terlihat sampai <see cref="ShowResult"/>).</summary>
    public void ShowProgress(string header, string message, Rect area)
    {
        _version++;
        SetRevealPath(null);
        SetContent(AccentBrush, ProgressGlyph, header, message);
        ProgressTrack.Visibility = Visibility.Visible;
        IconSpin.BeginAnimation(RotateTransform.AngleProperty,
            new DoubleAnimation(0, 360, TimeSpan.FromSeconds(1.2)) { RepeatBehavior = RepeatBehavior.Forever });
        SetProgress(null);
        Present(area);
        Panel.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(180)));
    }

    /// <param name="fraction">0..1, atau null jika progres tidak diketahui.</param>
    public void SetProgress(double? fraction)
    {
        double trackWidth = ProgressTrack.ActualWidth > 0 ? ProgressTrack.ActualWidth : 300;
        if (fraction is { } value)
        {
            ProgressSlide.BeginAnimation(TranslateTransform.XProperty, null);
            ProgressSlide.X = 0;
            ProgressFill.BeginAnimation(WidthProperty,
                new DoubleAnimation(trackWidth * value, TimeSpan.FromMilliseconds(200)));
        }
        else
        {
            // Bar pendek yang bolak-balik: progres tidak diketahui.
            ProgressFill.BeginAnimation(WidthProperty, null);
            ProgressFill.Width = trackWidth * 0.3;
            ProgressSlide.BeginAnimation(TranslateTransform.XProperty,
                new DoubleAnimation(-trackWidth * 0.3, trackWidth, TimeSpan.FromSeconds(1.1)) { RepeatBehavior = RepeatBehavior.Forever });
        }
    }

    /// <summary>Tampilkan hasil akhir, lalu hilang sendiri.</summary>
    /// <param name="revealPath">File/folder hasil: kalau diisi, klik notifikasi membukanya di Explorer.</param>
    public void ShowResult(bool success, string header, string message, Rect area, string? revealPath = null)
    {
        int version = ++_version;
        SetRevealPath(success ? revealPath : null);
        SetContent(success ? AccentBrush : ErrorBrush, success ? SuccessGlyph : ErrorGlyph, header, message);
        ProgressTrack.Visibility = Visibility.Collapsed;
        IconSpin.BeginAnimation(RotateTransform.AngleProperty, null);
        IconSpin.Angle = 0;
        Present(area);
        FadeOut(version, TimeSpan.FromMilliseconds(!success ? 7000 : _revealPath is null ? 3500 : 6000));
    }

    void FadeOut(int version, TimeSpan hold)
    {
        var fade = new DoubleAnimationUsingKeyFrames();
        fade.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(180))));
        fade.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromTimeSpan(hold)));
        fade.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(hold + TimeSpan.FromMilliseconds(500))));
        fade.Completed += (_, _) =>
        {
            if (version != _version)
                return;
            Dismiss();
        };
        Panel.BeginAnimation(OpacityProperty, fade);
    }

    void SetRevealPath(string? path)
    {
        _revealPath = path;
        HintText.Visibility = path is null ? Visibility.Collapsed : Visibility.Visible;
        Cursor = path is null ? null : System.Windows.Input.Cursors.Hand;
        var handle = new WindowInteropHelper(this).Handle;
        if (handle != IntPtr.Zero)
            NativeMethods.SetClickThrough(handle, clickThrough: path is null);
    }

    void RevealResult()
    {
        if (_revealPath is not { } path)
            return;
        try
        {
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{path}\"");
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // Explorer gagal dibuka: notifikasi tetap ditutup.
        }
        _version++;
        Dismiss();
    }

    void Dismiss()
    {
        SetRevealPath(null);
        _visible = false;
        Hide();
    }

    void SetContent(Brush accent, string glyph, string header, string message)
    {
        Panel.BorderBrush = accent;
        IconText.Foreground = accent;
        HeaderText.Foreground = accent;
        IconText.Text = glyph;
        HeaderText.Text = header;
        MessageText.Text = message;
    }

    void Present(Rect area)
    {
        _area = area;
        if (!_visible)
        {
            _visible = true;
            Show();
            PanelShift.BeginAnimation(TranslateTransform.YProperty,
                new DoubleAnimation(20, 0, TimeSpan.FromMilliseconds(260)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
        }
        UpdateLayout();
        Left = _area.Left + (_area.Width - ActualWidth) / 2;
        Top = _area.Bottom - ActualHeight - 60;
    }

    static Brush Frozen(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
