using System.Collections.Concurrent;
using System.IO;

namespace Rotiva.Conversion;

/// <summary>
/// Mencari program konversi (ffmpeg, magick, soffice, pandoc, gswin64c, 7z). Urutan pencarian:
/// folder "tools" di samping aplikasi, PATH (dibaca ulang dari registry supaya program
/// yang baru diinstal langsung terdeteksi), lalu lokasi instalasi yang umum.
/// </summary>
internal static class ToolLocator
{
    static readonly ConcurrentDictionary<string, string> Found = new();

    public static string? FFmpeg => Locate("ffmpeg.exe", () =>
    [
        Path.Combine(LocalAppData, @"Microsoft\WinGet\Links\ffmpeg.exe"),
        .. Glob(Path.Combine(LocalAppData, @"Microsoft\WinGet\Packages"), "Gyan.FFmpeg*", "*", @"bin\ffmpeg.exe"),
    ]);

    public static string? ImageMagick => Locate("magick.exe", () =>
        Glob(ProgramFiles, "ImageMagick*", "magick.exe"));

    public static string? Ghostscript => Locate("gswin64c.exe", () =>
        Glob(ProgramFiles, "gs", "gs*", @"bin\gswin64c.exe"));

    public static string? LibreOffice => Locate("soffice.exe", () =>
    [
        Path.Combine(ProgramFiles, @"LibreOffice\program\soffice.exe"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"LibreOffice\program\soffice.exe"),
    ]);

    public static string? Pandoc => Locate("pandoc.exe", () =>
    [
        Path.Combine(LocalAppData, @"Pandoc\pandoc.exe"),
        Path.Combine(ProgramFiles, @"Pandoc\pandoc.exe"),
    ]);

    public static string? SevenZip => Locate("7z.exe", () =>
    [
        Path.Combine(ProgramFiles, @"7-Zip\7z.exe"),
    ]);

    static string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    static string ProgramFiles => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

    // Hanya hasil yang ketemu yang di-cache, jadi program yang baru diinstal tetap bisa terdeteksi.
    static string? Locate(string exe, Func<IEnumerable<string>> knownLocations)
    {
        if (Found.TryGetValue(exe, out var cached) && File.Exists(cached))
            return cached;

        // Versi installer membawa tools\ffmpeg\ffmpeg.exe, tools\imagemagick\magick.exe,
        // tools\pandoc\pandoc.exe, tools\ghostscript\gswin64c.exe, tools\7zip\7z.exe, dan
        // tools\libreoffice\program\soffice.exe.
        string bundled = Path.Combine(AppContext.BaseDirectory, "tools");
        var candidates = Glob(bundled, "*", exe)
            .Concat(Glob(bundled, "*", "program", exe))
            .Concat(PathDirectories().Select(dir => Path.Combine(dir, exe)))
            .Concat(knownLocations());
        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
                return Found[exe] = candidate;
        }
        return null;
    }

    static IEnumerable<string> PathDirectories() =>
        new[] { EnvironmentVariableTarget.Process, EnvironmentVariableTarget.User, EnvironmentVariableTarget.Machine }
            .Select(target => Environment.GetEnvironmentVariable("PATH", target) ?? "")
            .SelectMany(path => path.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(dir => dir.IndexOfAny(Path.GetInvalidPathChars()) < 0)
            .Distinct(StringComparer.OrdinalIgnoreCase);

    // Glob(root, "A*", "*", "bin\x.exe") = semua root\A*\*\bin\x.exe yang ada.
    static IEnumerable<string> Glob(string root, params string[] parts)
    {
        IEnumerable<string> current = [root];
        foreach (var part in parts[..^1])
        {
            current = current.SelectMany(dir => Directory.Exists(dir)
                ? Directory.EnumerateDirectories(dir, part)
                : []);
        }
        return current.Select(dir => Path.Combine(dir, parts[^1]));
    }
}
