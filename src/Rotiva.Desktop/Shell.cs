using System.Diagnostics;

namespace Rotiva.Desktop;

/// <summary>Membuka link dan menunjukkan file hasil di file manager OS.</summary>
static class Shell
{
    public static void OpenUrl(string url) => Start(new ProcessStartInfo(url) { UseShellExecute = true });

    /// <summary>Explorer / Finder memilih file-nya; di Linux folder berisi file dibuka.</summary>
    public static void Reveal(string path)
    {
        if (OperatingSystem.IsWindows())
            Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\""));
        else if (OperatingSystem.IsMacOS())
            Start(new ProcessStartInfo("open", ["-R", path]));
        else
            Start(new ProcessStartInfo("xdg-open", [Directory.Exists(path) ? path : Path.GetDirectoryName(path)!]));
    }

    static void Start(ProcessStartInfo info)
    {
        try
        {
            Process.Start(info)?.Dispose();
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // Program pembuka tidak ada (mis. xdg-open di desktop minimal): abaikan.
        }
    }
}
