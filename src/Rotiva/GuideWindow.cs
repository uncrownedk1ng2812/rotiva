using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Rotiva.Controls;
using Rotiva.Input;
using Rotiva.Localization;

namespace Rotiva;

/// <summary>Panduan singkat cara memakai Rotiva, dibuka dari menu tray.</summary>
internal sealed class GuideWindow : Window
{
    static GuideWindow? _open;

    GuideWindow()
    {
        Title = L.T("Rotiva guide");
        Width = 620;
        Height = 680;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = new SolidColorBrush(Color.FromRgb(0x16, 0x16, 0x1A));
        Icon = BitmapFrameOfApp();
        Closed += (_, _) => _open = null;

        var page = new StackPanel { Margin = new Thickness(26, 22, 26, 26) };
        page.Children.Add(new TextBlock
        {
            Text = L.T("Rotiva guide"),
            FontFamily = RadialWheel.LabelFont,
            FontSize = 28,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 0, 6),
        });

        Section(page, L.T("Convert a file"),
            L.T("Drag a file from Explorer or the desktop."),
            L.T("While still holding the mouse button, press Shift."),
            L.T("Point at a format and let go. The result appears next to the original."),
            L.T("Changed your mind? Go back to the center and let go."));
        Section(page, L.T("Edit without opening an app"),
            L.T("Point at EDIT on the wheel and rest for a moment. The wheel changes to the tools."),
            L.T("Images: rotate, flip, grayscale. Video: rotate, flip, reverse, thumbnails. Audio: even out loudness, reverse, album art."),
            L.T("Rest on BACK to return to the formats."));
        Section(page, L.T("Put subtitles on a video"),
            L.T("Select exactly one video and one subtitle file (SRT, VTT, ASS, SSA, SMI or LRC)."),
            L.T("Drag both together and press Shift."),
            L.T("Drop on BURN IN. You get a new MP4 ending in \"(subtitled)\". Your files stay as they are."));
        Section(page, L.T("Convert a screenshot or anything you copied"),
            L.T("Take a screenshot with Win+Shift+S, or copy an image or some files."),
            L.F("Press {0}, or choose Convert clipboard in the tray menu.", ClipboardHotkey.Label),
            L.T("Click a format. Copied images are saved in Pictures\\Rotiva."),
            L.T("Choose TEXT on a screenshot to copy the words in it."));
        Section(page, L.T("Lock files with a password"),
            L.T("Drag a file, several files or a folder, press Shift and choose LOCK."),
            L.T("Type the password twice. It can't be recovered if you forget it."),
            L.T("Open the ZIP with 7-Zip or WinRAR."));
        Section(page, L.T("Make a contact sheet"),
            L.T("Drag several images, press Shift and choose SHEET to get one picture with all of them."),
            L.T("For a video, open EDIT and choose SHEET to get 16 frames in one picture."));
        Section(page, L.T("Prefer the right-click menu?"),
            L.T("Right-click a file and open Rotiva. On Windows 11 it may be under Show more options."));

        Content = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = page };
    }

    public static void ShowGuide()
    {
        _open ??= new GuideWindow();
        _open.Show();
        _open.Activate();
    }

    static void Section(StackPanel page, string title, params string[] steps)
    {
        page.Children.Add(new TextBlock
        {
            Text = title,
            FontFamily = RadialWheel.LabelFont,
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(RadialWheel.AccentColor),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 18, 0, 6),
        });
        for (int i = 0; i < steps.Length; i++)
        {
            var row = new Grid { Margin = new Thickness(0, 3, 0, 3) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.Children.Add(new TextBlock
            {
                Text = (i + 1).ToString(System.Globalization.CultureInfo.InvariantCulture),
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = Brushes.White,
            });
            var text = new TextBlock
            {
                Text = steps[i],
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromArgb(0xE0, 0xFF, 0xFF, 0xFF)),
            };
            Grid.SetColumn(text, 1);
            row.Children.Add(text);
            page.Children.Add(row);
        }
    }

    static System.Windows.Media.ImageSource? BitmapFrameOfApp()
    {
        using var icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath!);
        if (icon is null)
            return null;
        var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty,
            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        source.Freeze();
        return source;
    }
}
