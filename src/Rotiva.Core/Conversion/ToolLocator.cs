using System.Collections.Concurrent;
using System.IO;

namespace Rotiva.Conversion;

/// <summary>
/// Mencari program konversi (ffmpeg, magick, soffice, pandoc, Ghostscript, 7z). Urutan pencarian:
/// folder "tools" di samping aplikasi, PATH (di Windows juga dibaca ulang dari registry supaya
/// program yang baru diinstal langsung terdeteksi), lalu lokasi instalasi yang umum di OS tersebut.
/// </summary>
public static class ToolLocator
{
    static readonly ConcurrentDictionary<string, string> Found = new();

    // Folder program umum di macOS (Homebrew Apple Silicon & Intel) dan Linux.
    static readonly string[] UnixBinDirectories = ["/opt/homebrew/bin", "/usr/local/bin", "/usr/bin", "/snap/bin"];

    public static string? FFmpeg => Locate(Exe("ffmpeg"), () => OperatingSystem.IsWindows()
        ?
        [
            Path.Combine(LocalAppData, @"Microsoft\WinGet\Links\ffmpeg.exe"),
            .. Glob(Path.Combine(LocalAppData, @"Microsoft\WinGet\Packages"), "Gyan.FFmpeg*", "*", @"bin\ffmpeg.exe"),
        ]
        : InUnixBin("ffmpeg"));

    // ImageMagick 7 bernama magick. Distro yang masih memaketkan versi 6 (mis. Ubuntu 24.04) hanya
    // punya convert, yang menerima argumen yang sama. Di Windows convert.exe adalah alat bawaan
    // Windows (konversi FAT ke NTFS), jadi tidak dipakai di sana.
    public static string? ImageMagick => Locate(Exe("magick"), () => OperatingSystem.IsWindows()
        ? Glob(ProgramFiles, "ImageMagick*", "magick.exe")
        : InUnixBin("magick"))
        ?? (OperatingSystem.IsWindows() ? null : Locate("convert", () => InUnixBin("convert")));

    // Di Windows program konsolnya gswin64c.exe; di macOS/Linux namanya gs.
    public static string? Ghostscript => Locate(OperatingSystem.IsWindows() ? "gswin64c.exe" : "gs", () => OperatingSystem.IsWindows()
        ? Glob(ProgramFiles, "gs", "gs*", @"bin\gswin64c.exe")
        : InUnixBin("gs"));

    public static string? LibreOffice => Locate(Exe("soffice"), () => OperatingSystem.IsWindows()
        ?
        [
            Path.Combine(ProgramFiles, @"LibreOffice\program\soffice.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"LibreOffice\program\soffice.exe"),
        ]
        : ["/Applications/LibreOffice.app/Contents/MacOS/soffice", "/usr/lib/libreoffice/program/soffice", .. InUnixBin("soffice")]);

    public static string? Pandoc => Locate(Exe("pandoc"), () => OperatingSystem.IsWindows()
        ?
        [
            Path.Combine(LocalAppData, @"Pandoc\pandoc.exe"),
            Path.Combine(ProgramFiles, @"Pandoc\pandoc.exe"),
        ]
        : InUnixBin("pandoc"));

    // 7-Zip resmi untuk macOS/Linux bernama 7zz; paket p7zip memakai 7z.
    public static string? SevenZip => Locate(Exe("7z"), () => OperatingSystem.IsWindows()
        ? [Path.Combine(ProgramFiles, @"7-Zip\7z.exe")]
        : [.. InUnixBin("7zz"), .. PathDirectories().Select(dir => Path.Combine(dir, "7zz")), .. InUnixBin("7z")]);

    // OCR untuk macOS/Linux (Windows memakai OCR bawaan Windows).
    public static string? Tesseract => Locate(Exe("tesseract"), () => OperatingSystem.IsWindows()
        ? [Path.Combine(ProgramFiles, @"Tesseract-OCR\tesseract.exe")]
        : InUnixBin("tesseract"));

    /// <summary>Nama file program di OS ini: "ffmpeg.exe" di Windows, "ffmpeg" di macOS/Linux.</summary>
    internal static string Exe(string name) => OperatingSystem.IsWindows() ? name + ".exe" : name;

    static string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    static string ProgramFiles => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

    static IEnumerable<string> InUnixBin(string exe) => UnixBinDirectories.Select(dir => Path.Combine(dir, exe));

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

    // Di luar Windows, PATH User/Machine tidak ada (null): hanya PATH proses yang terbaca.
    static IEnumerable<string> PathDirectories() =>
        new[] { EnvironmentVariableTarget.Process, EnvironmentVariableTarget.User, EnvironmentVariableTarget.Machine }
            .Select(target => Environment.GetEnvironmentVariable("PATH", target) ?? "")
            .SelectMany(path => path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(dir => dir.IndexOfAny(Path.GetInvalidPathChars()) < 0)
            .Distinct(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

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
