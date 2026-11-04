using System.Windows;
using Rotiva.Conversion;
using Rotiva.Input;
using Rotiva.Localization;
using Rotiva.Shell;

namespace Rotiva;

public partial class App : Application
{
    Mutex? _instanceMutex;
    DragShiftDetector? _detector;
    System.Windows.Forms.NotifyIcon? _trayIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        L.Use(AppSettings.Language);

        // Dipanggil installer/uninstaller: cukup tulis/hapus menu klik kanan lalu keluar.
        if (e.Args is ["--register"] or ["--unregister"])
        {
            if (e.Args[0] == "--register" && ShellMenu.IsEnabled)
                ShellMenu.Register();
            else if (e.Args[0] == "--unregister")
                ShellMenu.Unregister();
            Shutdown();
            return;
        }

        // Dari menu klik kanan: "--target <kode> <file>".
        (string Target, string File)? command = e.Args is ["--target", var target, var file] ? (target, file) : null;

        // Hanya boleh satu instance: dua hook global akan memunculkan roda dua kali.
        _instanceMutex = new Mutex(true, "Rotiva.SingleInstance", out bool isFirstInstance);
        if (!isFirstInstance)
        {
            // Perintah dari menu diteruskan ke instance yang sudah berjalan.
            if (command is { } c && !CommandChannel.TrySend(c.Target, c.File))
            {
                MessageBox.Show(L.T("Rotiva is running but not responding. Close it from the tray icon and try again."),
                    "Rotiva", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            Shutdown();
            return;
        }

        Converter.TextRecognizer = new WindowsTextRecognizer();
        var overlay = new OverlayWindow();
        _detector = new DragShiftDetector();
        _detector.ShiftPressedDuringDrag += overlay.BeginProbe;
        _detector.DragEnded += overlay.OnGlobalDragEnded;
        _trayIcon = TrayIcon.Create(Shutdown, showWelcome: command is null);

        CommandChannel.Listen(overlay.ConvertFromShell);
        if (command is { } own)
            CommandChannel.Received(own.Target, own.File);
        if (ShellMenu.IsEnabled)
            ShellMenu.Register();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _detector?.Dispose();
        if (_trayIcon is not null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }
        _instanceMutex?.Dispose();
        base.OnExit(e);
    }
}
