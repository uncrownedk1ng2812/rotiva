using System.Diagnostics;
using System.Text;
using Avalonia.Platform;
using Rotiva.Models;

namespace Rotiva.Desktop;

/// <summary>
/// Rotiva di file manager Linux, per user (tanpa root), seperti menu klik kanan Explorer di Windows:
/// "Open With → Rotiva" (file .desktop), Nautilus (Scripts → Convert with Rotiva), Dolphin (service
/// menu), dan Nemo (action). Ditulis ulang tiap start supaya path program selalu benar (AppImage
/// bisa dipindah); dimatikan dari menu tray.
/// </summary>
[System.Runtime.Versioning.SupportedOSPlatform("linux")]
static class LinuxIntegration
{
    const string MenuName = "Convert with Rotiva";

    static readonly string DataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME") is { Length: > 0 } xdg
        ? xdg
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");

    static string DesktopEntry => Path.Combine(DataHome, "applications", "rotiva.desktop");
    static string NautilusScript => Path.Combine(DataHome, "nautilus", "scripts", MenuName);
    static string DolphinMenu => Path.Combine(DataHome, "kio", "servicemenus", "rotiva.desktop");
    static string NemoAction => Path.Combine(DataHome, "nemo", "actions", "rotiva.nemo_action");
    static string IconFile => Path.Combine(DataHome, "rotiva", "rotiva.png");

    // Tipe MIME (shared-mime-info) untuk ekstensi di FormatCatalog, supaya Rotiva muncul di "Open With".
    static readonly Dictionary<string, string[]> MimeTypes = new()
    {
        [".png"] = ["image/png"], [".jpg"] = ["image/jpeg"], [".jfif"] = ["image/jpeg"], [".webp"] = ["image/webp"],
        [".avif"] = ["image/avif"], [".heic"] = ["image/heic"], [".heif"] = ["image/heif"], [".bmp"] = ["image/bmp"],
        [".tiff"] = ["image/tiff"], [".gif"] = ["image/gif"], [".ico"] = ["image/vnd.microsoft.icon"],
        [".mp4"] = ["video/mp4"], [".mkv"] = ["video/x-matroska"], [".mov"] = ["video/quicktime"],
        [".avi"] = ["video/x-msvideo"], [".webm"] = ["video/webm"], [".wmv"] = ["video/x-ms-wmv"],
        [".flv"] = ["video/x-flv"], [".m4v"] = ["video/x-m4v"], [".3gp"] = ["video/3gpp"], [".ts"] = ["video/mp2t"],
        [".mp3"] = ["audio/mpeg"], [".wav"] = ["audio/x-wav", "audio/wav"], [".flac"] = ["audio/flac"],
        [".ogg"] = ["audio/ogg", "audio/x-vorbis+ogg"], [".opus"] = ["audio/x-opus+ogg"], [".m4a"] = ["audio/mp4"],
        [".aac"] = ["audio/aac"], [".wma"] = ["audio/x-ms-wma"], [".aiff"] = ["audio/x-aiff"],
        [".docx"] = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
        [".doc"] = ["application/msword"], [".odt"] = ["application/vnd.oasis.opendocument.text"],
        [".rtf"] = ["application/rtf"], [".txt"] = ["text/plain"], [".md"] = ["text/markdown"], [".html"] = ["text/html"],
        [".xlsx"] = ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"],
        [".xls"] = ["application/vnd.ms-excel"], [".csv"] = ["text/csv"], [".tsv"] = ["text/tab-separated-values"],
        [".ods"] = ["application/vnd.oasis.opendocument.spreadsheet"], [".pdf"] = ["application/pdf"],
        [".pptx"] = ["application/vnd.openxmlformats-officedocument.presentationml.presentation"],
        [".ppt"] = ["application/vnd.ms-powerpoint"], [".odp"] = ["application/vnd.oasis.opendocument.presentation"],
        [".ppsx"] = ["application/vnd.openxmlformats-officedocument.presentationml.slideshow"],
        [".pps"] = ["application/vnd.ms-powerpoint"],
        [".zip"] = ["application/zip"], [".rar"] = ["application/vnd.rar", "application/x-rar"],
        [".7z"] = ["application/x-7z-compressed"], [".tar"] = ["application/x-tar"], [".gz"] = ["application/gzip"],
        [".tgz"] = ["application/x-compressed-tar"], [".bz2"] = ["application/x-bzip2"], [".xz"] = ["application/x-xz"],
        [".psd"] = ["image/vnd.adobe.photoshop"], [".svg"] = ["image/svg+xml"], [".jxl"] = ["image/jxl"], [".dng"] = ["image/x-adobe-dng"],
        [".cr2"] = ["image/x-canon-cr2"], [".nef"] = ["image/x-nikon-nef"], [".arw"] = ["image/x-sony-arw"],
        [".mpeg"] = ["video/mpeg"], [".mpg"] = ["video/mpeg"], [".ogv"] = ["video/ogg"],
        [".epub"] = ["application/epub+zip"], [".fb2"] = ["application/x-fictionbook+xml"],
        [".srt"] = ["application/x-subrip"], [".vtt"] = ["text/vtt"], [".ass"] = ["text/x-ssa"], [".ssa"] = ["text/x-ssa"],
        [".json"] = ["application/json"], [".yaml"] = ["application/yaml", "application/x-yaml"],
    };

    /// <summary>Program yang dijalankan file manager: AppImage-nya sendiri kalau dari AppImage.</summary>
    static string Executable => Environment.GetEnvironmentVariable("APPIMAGE") is { Length: > 0 } appImage
        ? appImage
        : Environment.ProcessPath!;

    public static void Register()
    {
        try
        {
            WriteIcon();
            string exec = QuoteExec(Executable) + " %F";
            string mimeTypes = string.Join(';', FormatCatalog.Categories
                .SelectMany(c => c.Extensions)
                .SelectMany(ext => MimeTypes.GetValueOrDefault(ext, []))
                .Append("inode/directory")
                .Distinct()) + ";";

            Write(DesktopEntry, executable: false, $"""
                [Desktop Entry]
                Type=Application
                Name=Rotiva
                GenericName=File Converter
                Comment=Convert files from a wheel
                Exec={exec}
                Icon={IconFile}
                Terminal=false
                Categories=Utility;
                MimeType={mimeTypes}
                StartupNotify=false
                """);

            Write(NautilusScript, executable: true, $"""
                #!/bin/sh
                # Rotiva: dipasang otomatis; matikan lewat menu tray Rotiva ("File manager menu").
                exec {QuoteShell(Executable)} "$@"
                """);

            // KDE 6 hanya memuat service menu yang executable.
            Write(DolphinMenu, executable: true, $"""
                [Desktop Entry]
                Type=Service
                MimeType=all/allfiles;inode/directory;
                Actions=rotiva
                X-KDE-Priority=TopLevel

                [Desktop Action rotiva]
                Name={MenuName}
                Icon={IconFile}
                Exec={exec}
                """);

            Write(NemoAction, executable: false, $"""
                [Nemo Action]
                Name={MenuName}
                Comment=Open the Rotiva wheel for the selected files
                Exec={exec}
                Icon-Name={IconFile}
                Selection=notnone
                Extensions=any;
                """);

            RefreshDesktopDatabase();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Folder data user tidak bisa ditulis: Rotiva tetap jalan tanpa menu file manager.
        }
    }

    public static void Unregister()
    {
        foreach (string file in new[] { DesktopEntry, NautilusScript, DolphinMenu, NemoAction })
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Biarkan; dicoba lagi saat menu dimatikan berikutnya.
            }
        }
        RefreshDesktopDatabase();
    }

    static void WriteIcon()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(IconFile)!);
        using var source = AssetLoader.Open(new Uri("avares://Rotiva/Assets/icon.png"));
        using var target = File.Create(IconFile);
        source.CopyTo(target);
    }

    static void Write(string path, bool executable, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content + "\n", new UTF8Encoding(false));
        if (executable)
            File.SetUnixFileMode(path, File.GetUnixFileMode(path) | UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute);
    }

    // Supaya "Open With" langsung memuat entri baru; tidak wajib (tidak semua distro punya alatnya).
    static void RefreshDesktopDatabase()
    {
        try
        {
            Process.Start(new ProcessStartInfo("update-desktop-database", [Path.GetDirectoryName(DesktopEntry)!])
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            })?.Dispose();
        }
        catch (System.ComponentModel.Win32Exception)
        {
        }
    }

    // Exec di file .desktop: argumen berspasi dalam tanda kutip; ", `, $, \ di-escape.
    static string QuoteExec(string path)
    {
        var escaped = new StringBuilder();
        foreach (char c in path)
        {
            if (c is '"' or '`' or '$' or '\\')
                escaped.Append('\\');
            escaped.Append(c);
        }
        return $"\"{escaped}\"";
    }

    static string QuoteShell(string path) => "'" + path.Replace("'", "'\\''") + "'";
}
