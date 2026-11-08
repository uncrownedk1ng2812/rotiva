using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Rotiva.Controls;
using Rotiva.Conversion;
using Rotiva.Interop;
using Rotiva.Localization;

namespace Rotiva;

/// <summary>Meminta password untuk ZIP terkunci. Diisi dua kali karena salah ketik tidak bisa diperbaiki.</summary>
internal sealed class PasswordWindow : Window
{
    readonly PasswordBox _first = Field();
    readonly PasswordBox _second = Field();
    readonly Button _ok;
    readonly TextBlock _asciiWarning = new()
    {
        Text = L.T("Use only English letters, numbers and common symbols."),
        TextWrapping = TextWrapping.Wrap,
        FontSize = 12,
        Foreground = new SolidColorBrush(RadialWheel.AccentColor),
        Margin = new Thickness(0, 6, 0, 0),
        Visibility = Visibility.Collapsed,
    };

    PasswordWindow()
    {
        Title = "Rotiva";
        Width = 400;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        Topmost = true;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = new SolidColorBrush(Color.FromRgb(0x16, 0x16, 0x1A));

        _ok = ActionButton(L.T("Lock"), primary: true);
        _ok.IsDefault = true;
        _ok.IsEnabled = false;
        _ok.Click += (_, _) => DialogResult = true;
        var cancel = ActionButton(L.T("Cancel"), primary: false);
        cancel.IsCancel = true;
        _first.PasswordChanged += (_, _) => Refresh();
        _second.PasswordChanged += (_, _) => Refresh();

        var panel = new StackPanel { Margin = new Thickness(22) };
        panel.Children.Add(new TextBlock
        {
            Text = L.T("Lock with a password"),
            FontFamily = RadialWheel.LabelFont,
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 0, 14),
        });
        panel.Children.Add(Label(L.T("Password")));
        panel.Children.Add(_first);
        panel.Children.Add(_asciiWarning);
        panel.Children.Add(Label(L.T("Repeat the password")));
        panel.Children.Add(_second);
        panel.Children.Add(new TextBlock
        {
            Text = L.T("The password can't be recovered if you forget it. Open the ZIP with 7-Zip or WinRAR. File names inside stay visible."),
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(0xB0, 0xFF, 0xFF, 0xFF)),
            Margin = new Thickness(0, 12, 0, 16),
        });
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        buttons.Children.Add(cancel);
        buttons.Children.Add(_ok);
        panel.Children.Add(buttons);
        Content = panel;

        Loaded += (_, _) =>
        {
            Activate();
            NativeMethods.SetForegroundWindow(new WindowInteropHelper(this).Handle);
            _first.Focus();
            Keyboard.Focus(_first);
        };
    }

    /// <summary>Password yang dimasukkan; null kalau dibatalkan.</summary>
    public static string? Ask()
    {
        var window = new PasswordWindow();
        return window.ShowDialog() == true ? window._first.Password : null;
    }

    void Refresh()
    {
        bool ascii = Converter.IsValidZipPassword(_first.Password) || _first.Password.Length == 0;
        _asciiWarning.Visibility = ascii ? Visibility.Collapsed : Visibility.Visible;
        _ok.IsEnabled = ascii && _first.Password.Length > 0 && _first.Password == _second.Password;
    }

    static TextBlock Label(string text) => new()
    {
        Text = text,
        FontSize = 12,
        Foreground = Brushes.White,
        Margin = new Thickness(0, 8, 0, 4),
    };

    static PasswordBox Field() => new() { Padding = new Thickness(6, 5, 6, 5), FontSize = 14 };

    static Button ActionButton(string text, bool primary) => new()
    {
        Content = text,
        MinWidth = 86,
        Padding = new Thickness(12, 6, 12, 6),
        Margin = new Thickness(8, 0, 0, 0),
        Background = primary ? new SolidColorBrush(RadialWheel.AccentColor) : Brushes.Transparent,
        Foreground = primary ? Brushes.Black : Brushes.White,
        BorderBrush = new SolidColorBrush(Color.FromArgb(0x60, 0xFF, 0xFF, 0xFF)),
    };
}
