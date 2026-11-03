using Avalonia;

namespace Rotiva.Desktop;

static class Program
{
    /// <summary>
    /// <c>Rotiva [file...]</c>: tanpa file Rotiva menunggu di tray; dengan file roda langsung dibuka
    /// untuk file tersebut. Kalau Rotiva sudah berjalan, file diteruskan ke sana lalu proses ini selesai.
    /// </summary>
    [STAThread]
    public static void Main(string[] args)
    {
        var files = args
            .Where(arg => File.Exists(arg) || Directory.Exists(arg))
            .Select(Path.GetFullPath)
            .ToArray();
        if (InstanceChannel.TrySend(files))
            return;

        App.StartupFiles = files;
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, Avalonia.Controls.ShutdownMode.OnExplicitShutdown);
    }

    // Dipakai juga oleh designer Avalonia.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            // macOS: hidup di menu bar saja, tanpa ikon di Dock (Avalonia mengabaikan LSUIElement).
            .With(new MacOSPlatformOptions { ShowInDock = false })
            .LogToTrace();
}
