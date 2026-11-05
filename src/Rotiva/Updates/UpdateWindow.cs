using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
using Rotiva.Updates;

namespace Rotiva;

/// <summary>
/// Jendela "Update tersedia" di pojok kanan bawah (dekat tray): versi baru, tanggal, ukuran,
/// daftar perubahan dari catatan rilis GitHub, lalu tombol Update sekarang / Nanti / Lewati versi ini.
/// </summary>
internal sealed class UpdateWindow : Window
{
    const double PanelWidth = 440;
    const double ShadowMargin = 14;

    static readonly FontFamily TextFont = new("Segoe UI Variable Text, Segoe UI");
    static readonly FontFamily CodeFont = new("Cascadia Mono, Consolas");
    static readonly Brush TextBrush = Frozen(Color.FromArgb(0xF2, 0xFF, 0xFF, 0xFF));
    static readonly Brush BodyBrush = Frozen(Color.FromArgb(0xD0, 0xFF, 0xFF, 0xFF));
    static readonly Brush MutedBrush = Frozen(Color.FromArgb(0x8C, 0xFF, 0xFF, 0xFF));
    static readonly Brush HoverBrush = Frozen(Color.FromArgb(0x1A, 0xFF, 0xFF, 0xFF));
    static readonly Brush DividerBrush = Frozen(Color.FromArgb(0x1F, 0xFF, 0xFF, 0xFF));
    static readonly Brush CodeBrush = Frozen(Color.FromArgb(0x24, 0xFF, 0xFF, 0xFF));
    static readonly Brush AccentBrush = Frozen(RadialWheel.AccentColor);
    static readonly Brush ErrorBrush = Frozen(Color.FromRgb(0xFF, 0x6B, 0x6B));

    static UpdateWindow? _open;

    readonly ReleaseInfo _release;
    readonly StackPanel _progressArea = new() { Visibility = Visibility.Collapsed, Margin = new Thickness(0, 12, 0, 0) };
    readonly Border _progressFill = new() { Background = AccentBrush, HorizontalAlignment = HorizontalAlignment.Left, Width = 0 };
    readonly TextBlock _progressText = new() { FontFamily = TextFont, FontSize = 12, Foreground = MutedBrush, Margin = new Thickness(0, 6, 0, 0) };
    readonly StackPanel _buttons = new() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
    readonly TextBlock _skipLink;
    Border _primary = null!;
    TextBlock _primaryText = null!;
    CancellationTokenSource? _download;

    UpdateWindow(ReleaseInfo release)
    {
        _release = release;
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        ResizeMode = ResizeMode.NoResize;
        SizeToContent = SizeToContent.WidthAndHeight;
        Topmost = true;
        ShowInTaskbar = false;
        Title = L.F("Rotiva {0} is available", release.Version.ToString(3));

        _skipLink = Link(L.T("Skip this version"), () =>
        {
            Updater.Skip(_release);
            Close();
        });

        var content = new StackPanel { Width = PanelWidth };
        content.Children.Add(Header());
        content.Children.Add(Divider(14));
        content.Children.Add(new TextBlock
        {
            Text = L.T("WHAT'S NEW"),
            FontFamily = RadialWheel.LabelFont,
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = AccentBrush,
            Margin = new Thickness(0, 0, 0, 6),
        });
        content.Children.Add(new ScrollViewer
        {
            MaxHeight = 300,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = Notes(release.WhatsNew),
            Padding = new Thickness(0, 0, 6, 0),
        });
        content.Children.Add(Link(L.T("Full release notes on GitHub"), () => Open(release.PageUrl), small: true));
        content.Children.Add(Progress());
        content.Children.Add(Divider(12));
        content.Children.Add(Footer());

        var panel = new Border
        {
            Margin = new Thickness(ShadowMargin),
            Padding = new Thickness(18, 16, 18, 14),
            CornerRadius = new CornerRadius(10),
            Background = Frozen(Color.FromArgb(0xFA, 0x16, 0x16, 0x1A)),
            BorderBrush = Frozen(Color.FromArgb(0x2E, 0xFF, 0xFF, 0xFF)),
            BorderThickness = new Thickness(1),
            Effect = new DropShadowEffect { BlurRadius = 22, ShadowDepth = 3, Opacity = 0.5 },
            Child = content,
            RenderTransform = new TranslateTransform(),
        };
        // Bisa digeser dengan menyeret bagian mana pun yang bukan tombol/tautan.
        panel.MouseLeftButtonDown += (_, e) =>
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        };
        Content = panel;

        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape && _download is null)
                Close();
        };
        Closed += (_, _) =>
        {
            _download?.Cancel();
            if (_open == this)
                _open = null;
        };
    }

    /// <summary>
    /// Tampilkan jendela untuk <paramref name="release"/> di atas area tray.
    /// <paramref name="activate"/> false = muncul tanpa mencuri fokus (cek otomatis).
    /// </summary>
    public static void ShowFor(ReleaseInfo release, bool activate)
    {
        if (_open is not null)
        {
            if (_open._release.Tag == release.Tag)
            {
                if (activate)
                    _open.Activate();
                return;
            }
            _open.Close();
        }

        var window = new UpdateWindow(release) { ShowActivated = activate };
        _open = window;
        new WindowInteropHelper(window).EnsureHandle();
        var content = (FrameworkElement)window.Content;
        content.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var size = content.DesiredSize;

        // Pojok kanan bawah layar utama, tepat di atas taskbar/tray.
        var work = SystemParameters.WorkArea;
        window.Left = work.Right - size.Width + ShadowMargin - 8;
        window.Top = work.Bottom - size.Height + ShadowMargin - 8;
        window.Show();
        if (activate)
        {
            window.Activate();
            NativeMethods.SetForegroundWindow(new WindowInteropHelper(window).Handle);
        }
        window.PlayOpen();
    }

    UIElement Header()
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var icon = new Image { Width = 40, Height = 40, Margin = new Thickness(0, 0, 14, 0), Source = AppIcon(), VerticalAlignment = VerticalAlignment.Top };
        grid.Children.Add(icon);

        var text = new StackPanel();
        text.Children.Add(new TextBlock
        {
            Text = L.T("UPDATE AVAILABLE"),
            FontFamily = RadialWheel.LabelFont,
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Foreground = AccentBrush,
        });
        text.Children.Add(new TextBlock
        {
            Text = "Rotiva " + _release.Version.ToString(3),
            FontFamily = RadialWheel.LabelFont,
            FontSize = 22,
            FontWeight = FontWeights.SemiBold,
            Foreground = TextBrush,
        });

        var meta = new List<string> { L.F("You have {0}", Updater.Current.ToString(3)) };
        if (_release.Published is { } published)
            meta.Add(L.F("Released {0}", published.ToLocalTime().ToString("d MMM yyyy", CultureInfo.CurrentCulture)));
        if (_release.WindowsInstaller is { Size: > 0 } installer)
            meta.Add(FormatSize(installer.Size));
        text.Children.Add(new TextBlock
        {
            Text = string.Join("  ·  ", meta),
            FontFamily = TextFont,
            FontSize = 12,
            Foreground = MutedBrush,
            Margin = new Thickness(0, 2, 0, 0),
        });
        Grid.SetColumn(text, 1);
        grid.Children.Add(text);

        var close = IconButton("", L.T("Later"), Close);
        close.VerticalAlignment = VerticalAlignment.Top;
        Grid.SetColumn(close, 2);
        grid.Children.Add(close);
        return grid;
    }

    /// <summary>Catatan rilis (Markdown sederhana) sebagai teks berformat.</summary>
    static UIElement Notes(string markdown)
    {
        var panel = new StackPanel();
        var blocks = ReleaseNotes.Blocks(markdown);
        if (blocks.Count == 0)
            blocks.Add(new NoteBlock(NoteBlockKind.Paragraph, L.T("Bug fixes and improvements.")));

        foreach (var block in blocks)
        {
            var text = new TextBlock
            {
                FontFamily = TextFont,
                FontSize = 13,
                Foreground = BodyBrush,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 19,
                Margin = new Thickness(0, 0, 0, 6),
            };
            AddSpans(text.Inlines, block.Text);

            switch (block.Kind)
            {
                case NoteBlockKind.Heading:
                    text.FontWeight = FontWeights.SemiBold;
                    text.Foreground = TextBrush;
                    text.Margin = new Thickness(0, panel.Children.Count == 0 ? 0 : 6, 0, 4);
                    panel.Children.Add(text);
                    break;
                case NoteBlockKind.Bullet:
                    var row = new Grid { Margin = new Thickness(0, 0, 0, 6) };
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    row.Children.Add(new Ellipse { Width = 5, Height = 5, Fill = AccentBrush, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(2, 7, 0, 0) });
                    text.Margin = new Thickness(0);
                    Grid.SetColumn(text, 1);
                    row.Children.Add(text);
                    panel.Children.Add(row);
                    break;
                default:
                    panel.Children.Add(text);
                    break;
            }
        }
        return panel;
    }

    static void AddSpans(InlineCollection inlines, string text)
    {
        foreach (var span in ReleaseNotes.Spans(text))
        {
            Inline inline;
            if (span.Style.HasFlag(NoteStyle.Code))
            {
                inline = new Run(span.Text) { FontFamily = CodeFont, FontSize = 12, Background = CodeBrush };
            }
            else if (span.Style.HasFlag(NoteStyle.Link) && span.Url is { } url)
            {
                var link = new Hyperlink(new Run(span.Text)) { Foreground = AccentBrush, Cursor = Cursors.Hand };
                link.Click += (_, _) => Open(url);
                inline = link;
            }
            else
            {
                inline = new Run(span.Text);
            }
            if (span.Style.HasFlag(NoteStyle.Bold))
            {
                inline.FontWeight = FontWeights.SemiBold;
                if (!span.Style.HasFlag(NoteStyle.Link))
                    inline.Foreground = TextBrush;
            }
            if (span.Style.HasFlag(NoteStyle.Italic))
                inline.FontStyle = FontStyles.Italic;
            inlines.Add(inline);
        }
    }

    UIElement Progress()
    {
        var track = new Border
        {
            Height = 4,
            CornerRadius = new CornerRadius(2),
            Background = Frozen(Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF)),
            ClipToBounds = true,
            Child = _progressFill,
        };
        _progressFill.CornerRadius = new CornerRadius(2);
        _progressArea.Children.Add(track);
        _progressArea.Children.Add(_progressText);
        return _progressArea;
    }

    UIElement Footer()
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        _skipLink.VerticalAlignment = VerticalAlignment.Center;
        grid.Children.Add(_skipLink);

        _buttons.Children.Add(Button(L.T("Later"), Close, primary: false));
        _primary = Button(L.T("Update now"), StartUpdate, primary: true);
        _primaryText = (TextBlock)_primary.Child;
        _buttons.Children.Add(_primary);
        Grid.SetColumn(_buttons, 1);
        grid.Children.Add(_buttons);
        return grid;
    }

    async void StartUpdate()
    {
        if (_download is not null)
            return;
        _download = new CancellationTokenSource();
        _skipLink.Visibility = Visibility.Hidden;
        _progressArea.Visibility = Visibility.Visible;
        _progressText.Foreground = MutedBrush;
        _progressText.Text = L.T("Starting download…");
        _primary.IsEnabled = false;
        _primary.Opacity = 0.5;
        _primaryText.Text = L.T("Downloading…");
        // "Nanti" berubah jadi "Batal" selama mengunduh.
        var later = (Border)_buttons.Children[0];
        ((TextBlock)later.Child).Text = L.T("Cancel");

        var progress = new Progress<(long Done, long Total)>(p =>
        {
            double fraction = p.Total > 0 ? Math.Clamp((double)p.Done / p.Total, 0, 1) : 0;
            _progressFill.Width = fraction * (PanelWidth);
            _progressText.Text = L.F("Downloading… {0}%  ({1} of {2})",
                (int)(fraction * 100), FormatSize(p.Done), FormatSize(p.Total));
        });

        try
        {
            string installer = await Updater.DownloadAsync(_release, progress, _download.Token);
            _progressText.Text = L.T("Installing… Rotiva will restart by itself.");
            _primaryText.Text = L.T("Installing…");
            await Task.Delay(600);
            Updater.Install(installer);
        }
        catch (OperationCanceledException) when (_download.IsCancellationRequested)
        {
            // Dibatalkan pengguna (jendela ditutup).
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or InvalidDataException
                                       or OperationCanceledException or UnauthorizedAccessException
                                       or System.ComponentModel.Win32Exception)
        {
            _download = null;
            _progressFill.Width = 0;
            _progressText.Foreground = ErrorBrush;
            _progressText.Text = ex is InvalidDataException
                ? L.T("The download was damaged. Please try again.")
                : L.T("The update couldn't be downloaded. Check your internet connection and try again.");
            _primary.IsEnabled = true;
            _primary.Opacity = 1;
            _primaryText.Text = L.T("Try again");
            ((TextBlock)later.Child).Text = L.T("Later");
            _skipLink.Visibility = Visibility.Visible;
        }
    }

    void PlayOpen()
    {
        var panel = (Border)Content;
        var duration = TimeSpan.FromMilliseconds(200);
        panel.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, duration));
        ((TranslateTransform)panel.RenderTransform).BeginAnimation(TranslateTransform.YProperty,
            new DoubleAnimation(16, 0, duration) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
    }

    static Border Button(string label, Action click, bool primary)
    {
        var button = new Border
        {
            MinWidth = 88,
            Height = 32,
            Margin = new Thickness(8, 0, 0, 0),
            Padding = new Thickness(14, 0, 14, 0),
            CornerRadius = new CornerRadius(6),
            Background = primary ? AccentBrush : Brushes.Transparent,
            BorderBrush = primary ? AccentBrush : Frozen(Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF)),
            BorderThickness = new Thickness(1),
            Cursor = Cursors.Hand,
            Child = new TextBlock
            {
                Text = label,
                FontFamily = TextFont,
                FontSize = 13,
                FontWeight = primary ? FontWeights.SemiBold : FontWeights.Normal,
                Foreground = primary ? Brushes.Black : TextBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
        var normal = button.Background;
        button.MouseEnter += (_, _) => button.Background = primary ? Frozen(Color.FromRgb(0xFF, 0xA0, 0x48)) : HoverBrush;
        button.MouseLeave += (_, _) => button.Background = normal;
        button.MouseLeftButtonDown += (_, e) => e.Handled = true; // jangan memicu DragMove
        button.MouseLeftButtonUp += (_, e) =>
        {
            e.Handled = true;
            if (button.IsEnabled)
                click();
        };
        return button;
    }

    static Border IconButton(string glyph, string toolTip, Action click)
    {
        var button = new Border
        {
            Width = 28,
            Height = 28,
            CornerRadius = new CornerRadius(5),
            Background = Brushes.Transparent,
            Cursor = Cursors.Hand,
            ToolTip = toolTip,
            Child = new TextBlock
            {
                Text = glyph,
                FontFamily = RadialWheel.IconFont,
                FontSize = 11,
                Foreground = MutedBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
        button.MouseEnter += (_, _) => button.Background = HoverBrush;
        button.MouseLeave += (_, _) => button.Background = Brushes.Transparent;
        button.MouseLeftButtonDown += (_, e) => e.Handled = true;
        button.MouseLeftButtonUp += (_, e) =>
        {
            e.Handled = true;
            click();
        };
        return button;
    }

    static TextBlock Link(string label, Action click, bool small = false)
    {
        var link = new TextBlock
        {
            Text = label,
            FontFamily = TextFont,
            FontSize = small ? 12 : 12.5,
            Foreground = small ? AccentBrush : MutedBrush,
            Cursor = Cursors.Hand,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = small ? new Thickness(0, 4, 0, 0) : new Thickness(0),
        };
        link.MouseEnter += (_, _) => link.TextDecorations = TextDecorations.Underline;
        link.MouseLeave += (_, _) => link.TextDecorations = null;
        link.MouseLeftButtonDown += (_, e) => e.Handled = true;
        link.MouseLeftButtonUp += (_, e) =>
        {
            e.Handled = true;
            click();
        };
        return link;
    }

    static UIElement Divider(double space) =>
        new Rectangle { Height = 1, Fill = DividerBrush, Margin = new Thickness(0, space, 0, space) };

    static string FormatSize(long bytes) => bytes >= 1 << 20
        ? (bytes / (1024.0 * 1024)).ToString("0", CultureInfo.CurrentCulture) + " MB"
        : (bytes / 1024.0).ToString("0", CultureInfo.CurrentCulture) + " KB";

    static void Open(string url)
    {
        if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
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

    static SolidColorBrush Frozen(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
