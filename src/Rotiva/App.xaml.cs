using System.Windows;
using Rotiva.Conversion;
using Rotiva.Input;
using Rotiva.Interop;
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

        // Dari menu klik kanan: "--target <kode> <file>", atau "--wheel <file>..." (menu "Convert with
        // Rotiva" versi MSIX) dan "Open with → Rotiva" (hanya path file) untuk membuka roda di kursor.
        (string Target, string File)[] commands = e.Args switch
        {
            ["--target", var target, var file] => [(target, file)],
            ["--wheel", .. var files] => files.Select(file => (CommandChannel.WheelId, file)).ToArray(),
            [var first, ..] when !first.StartsWith("--") => e.Args.Select(file => (CommandChannel.WheelId, file)).ToArray(),
            _ => [],
        };

        // Hanya boleh satu instance: dua hook global akan memunculkan roda dua kali.
        _instanceMutex = new Mutex(true, "Rotiva.SingleInstance", out bool isFirstInstance);
        if (!isFirstInstance)
        {
            // Perintah dari menu diteruskan ke instance yang sudah berjalan.
            if (commands.Length > 0 && !commands.All(c => CommandChannel.TrySend(c.Target, c.File)))
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
        _trayIcon = TrayIcon.Create(Shutdown, showWelcome: commands.Length == 0);

        CommandChannel.Listen((targetId, files) =>
        {
            if (targetId == CommandChannel.WheelId)
                overlay.ShowWheel(files);
            else
                overlay.ConvertFromShell(targetId, files);
        });
        foreach (var (target, file) in commands)
            CommandChannel.Received(target, file);
        // Versi MSIX: menu klik kanan didaftarkan oleh manifest paket ("Convert with Rotiva").
        if (ShellMenu.IsEnabled && !NativeMethods.IsPackaged)
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
