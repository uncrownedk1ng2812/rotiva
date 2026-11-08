using System.IO;
using Rotiva.Localization;

namespace Rotiva.Models;

public sealed record ConvertTarget(string Label, string Description, string Extension, string Glyph)
{
    public TargetAction Action { get; init; }

    /// <summary>Operasi gambar untuk <see cref="TargetAction.Edit"/>.</summary>
    public ImageEdit Edit { get; init; }

    /// <summary>Password untuk <see cref="TargetAction.ZipPassword"/>; diisi tepat sebelum masuk antrean.</summary>
    public string? Password { get; init; }

    /// <summary>Isi halaman kedua roda untuk <see cref="TargetAction.Group"/>; null pada irisan BACK.</summary>
    public IReadOnlyList<ConvertTarget>? Children { get; init; }
}

public enum ImageEdit { None, RotateRight, RotateLeft, FlipHorizontal, FlipVertical, Grayscale, Reverse, Normalize, Cover, Thumbnails }

/// <summary>
/// Convert = ubah ke format <see cref="ConvertTarget.Extension"/>; Compress = perkecil ukuran
/// (format tetap); MergePdf = semua file yang di-drag digabung jadi satu PDF;
/// ExtractText = baca teksnya (OCR) ke file .txt dan clipboard; SplitPdf = tiap halaman jadi PDF sendiri;
/// Zip = semua yang di-drag (termasuk folder) jadi satu .zip; Extract = buka arsip ke folder di sebelahnya;
/// Edit = putar/balik/abu-abu, format tetap; Group = bukan konversi, membuka halaman pilihan lain di roda.
/// </summary>
public enum TargetAction { Convert, Compress, MergePdf, ExtractText, SplitPdf, Zip, Extract, Edit, Group, BurnSubtitles, ZipPassword, ContactSheet }

/// <summary>
/// Program yang dipakai untuk mengonversi file dalam satu kategori. Data = kode Rotiva sendiri (JSON, YAML, CSV).
/// None = belum didukung (hanya bisa di-ZIP).
/// </summary>
public enum ConversionEngine { ImageMagick, FFmpeg, Office, SevenZip, Data, None }

/// <param name="FileName">Nama file yang sedang di-drag (kosong untuk definisi di katalog).</param>
public sealed record FileCategory(string Name, string Glyph, string FileName, IReadOnlyList<ConvertTarget> Targets)
{
    /// <summary>Ekstensi sumber yang masuk kategori ini (huruf kecil, sudah dinormalisasi).</summary>
    public IReadOnlyList<string> Extensions { get; init; } = [];

    public ConversionEngine Engine { get; init; }
}

/// <summary>
/// Daftar format tujuan per jenis file. Kategori dipilih otomatis dari ekstensi file yang di-drag.
/// </summary>
public static class FormatCatalog
{
    // Glyph dari font "Segoe Fluent Icons" / "Segoe MDL2 Assets" (bawaan Windows).
    const string Image = "";
    const string Video = "";
    const string Audio = "";
    const string Document = "";
    const string Pdf = "";
    const string Sheet = "\uF0E2";
    const string Slides = "\uE786";
    const string Shrink = "\uE73F";
    const string Stack = "\uE8C8";
    const string Text = "\uE8D2";
    const string ZipFolder = "\uF012";
    const string OpenFolder = "\uE8DA";
    const string Folder = "\uE8B7";
    const string Page = "\uE7C3";
    const string Book = "\uE82D";
    const string Captions = "\uE8BD";
    const string Code = "\uE943";
    const string EditPencil = "\uE70F";

    static ConvertTarget EditTarget(string label, string description, ImageEdit edit, string glyph) =>
        new(label, description, "", glyph) { Action = TargetAction.Edit, Edit = edit };

    static readonly ConvertTarget RotateRight = EditTarget("ROTATE R", "Rotate right 90\u00B0", ImageEdit.RotateRight, "\uE7A6");
    static readonly ConvertTarget RotateLeft = EditTarget("ROTATE L", "Rotate left 90\u00B0", ImageEdit.RotateLeft, "\uE7A7");
    static readonly ConvertTarget FlipH = EditTarget("FLIP H", "Mirror left to right", ImageEdit.FlipHorizontal, "\uE8AB");
    static readonly ConvertTarget FlipV = EditTarget("FLIP V", "Mirror top to bottom", ImageEdit.FlipVertical, "\uE8CB");
    static readonly ConvertTarget Reverse = EditTarget("REVERSE", "Play backwards", ImageEdit.Reverse, "\uE7A7");

    // Satu irisan yang membuka halaman kedua: alat-alat ini tidak muat sebagai irisan di roda.
    // Tiap grup minimal 3 alat, supaya halamannya (dengan BACK) tidak punya slot kosong.
    static readonly ConvertTarget ImageEditGroup = new("EDIT", "Rotate, flip, grayscale", "", EditPencil)
    {
        Action = TargetAction.Group,
        Children = [RotateRight, RotateLeft, FlipH, FlipV, EditTarget("GRAY", "Black and white", ImageEdit.Grayscale, "\uE790")],
    };

    static readonly ConvertTarget VideoEditGroup = new("EDIT", "Rotate, flip, reverse", "", EditPencil)
    {
        Action = TargetAction.Group,
        Children =
        [
            RotateRight, RotateLeft, FlipH, FlipV, Reverse,
            EditTarget("SHEET", "16 frames in one picture", ImageEdit.Thumbnails, ""),
        ],
    };

    static readonly ConvertTarget AudioEditGroup = new("EDIT", "Loudness, reverse, cover", "", EditPencil)
    {
        Action = TargetAction.Group,
        Children =
        [
            EditTarget("NORMALIZE", "Even out the loudness", ImageEdit.Normalize, "\uE767"),
            Reverse,
            EditTarget("COVER", "Save the album art", ImageEdit.Cover, "\uE93C"),
        ],
    };

    /// <summary>Muncul kalau yang di-drag tepat satu video dan satu file subtitle.</summary>
    public static readonly ConvertTarget BurnSubtitles = new("BURN IN", "Put the subtitles on the video", ".mp4", Captions)
    {
        Action = TargetAction.BurnSubtitles,
    };

    /// <summary>Irisan kembali ke halaman utama roda.</summary>
    public static readonly ConvertTarget Back = new("BACK", "All formats", "", "\uE72B") { Action = TargetAction.Group };

    /// <summary>Foto kamera RAW: hanya bisa dibaca, jadi hasil edit disimpan sebagai JPG.</summary>
    public static readonly IReadOnlyList<string> RawExtensions =
    [
        ".dng", ".cr2", ".cr3", ".crw", ".nef", ".nrw", ".arw", ".srf", ".sr2", ".orf", ".rw2", ".raf",
        ".pef", ".srw", ".x3f", ".3fr", ".erf", ".mrw", ".dcr", ".kdc", ".iiq", ".mos", ".mef", ".rwl",
    ];

    // Format tetap; hanya disarankan untuk file yang memang besar (foto, video, PDF hasil scan).
    static readonly ConvertTarget Compress = new("COMPRESS", "Shrink file size", "", Shrink)
    {
        Action = TargetAction.Compress,
    };

    // Hanya muncul kalau yang di-drag lebih dari satu file. Gambar, dokumen, dan tabel diubah ke PDF dulu.
    static readonly ConvertTarget MergePdf = new("MERGE", "Combine into one PDF", ".pdf", Stack)
    {
        Action = TargetAction.MergePdf,
    };

    // OCR untuk gambar; untuk PDF teks aslinya diambil langsung, OCR hanya kalau PDF-nya hasil scan.
    static readonly ConvertTarget ExtractText = new("TEXT", "Copy text (OCR)", ".txt", Text)
    {
        Action = TargetAction.ExtractText,
    };

    // Muncul kalau beberapa file di-drag (di semua kategori), dan selalu untuk folder/file yang belum didukung.
    static readonly ConvertTarget Zip = new("ZIP", "Pack into a .zip", ".zip", ZipFolder)
    {
        Action = TargetAction.Zip,
    };

    // Selalu muncul, juga untuk satu file: mengunci satu dokumen adalah pemakaian yang paling umum.
    static readonly ConvertTarget LockZip = new("LOCK", "ZIP with a password", ".zip", "")
    {
        Action = TargetAction.ZipPassword,
    };

    static readonly ConvertTarget ContactSheet = new("SHEET", "One picture of all the images", ".jpg", "")
    {
        Action = TargetAction.ContactSheet,
    };

    static readonly ConvertTarget Extract = new("EXTRACT", "Unpack to a folder", "", OpenFolder)
    {
        Action = TargetAction.Extract,
    };

    /// <summary>Folder dan file yang jenisnya belum didukung: hanya bisa dijadikan ZIP.</summary>
    static readonly FileCategory Other = new("File", Page, "", [Zip, LockZip]) { Engine = ConversionEngine.None };

    public static IReadOnlyList<FileCategory> Categories { get; } =
    [
        new("Image", Image, "",
        [
            Compress,
            new("PNG", "Lossless, transparent", ".png", Image),
            new("JPG", "Small size", ".jpg", Image),
            new("WEBP", "Modern, for the web", ".webp", Image),
            new("AVIF", "Best compression", ".avif", Image),
            new("PDF", "PDF document", ".pdf", Pdf),
            new("ICO", "Windows icon", ".ico", Image),
            new("TIFF", "Print quality", ".tiff", Image),
            // BMP sebagai tujuan dihapus supaya roda tidak terlalu penuh (file .bmp tetap bisa dikonversi).
            ExtractText,
            ImageEditGroup,
            ContactSheet,
            MergePdf,
            Zip,
            LockZip,
        ])
        {
            Engine = ConversionEngine.ImageMagick,
            Extensions =
            [
                ".png", ".jpg", ".webp", ".avif", ".heic", ".heif", ".bmp", ".tiff", ".gif", ".ico", ".jfif",
                ".psd", ".psb", ".jxl", ".jp2", ".j2k", ".tga", ".svg", ".svgz", ".exr", ".hdr", ".dds", ".pcx",
                ".xcf", ".ppm", ".pgm", ".pbm", ".qoi", ".cur",
                .. RawExtensions,
            ],
        },
        new("Video", Video, "",
        [
            Compress with { Description = "MP4 up to 1080p" },
            new("MP4", "H.264, most compatible", ".mp4", Video),
            new("WEBM", "VP9 for the web", ".webm", Video),
            new("MOV", "QuickTime", ".mov", Video),
            new("MKV", "Matroska", ".mkv", Video),
            new("AVI", "Legacy format", ".avi", Video),
            new("GIF", "Silent animation", ".gif", Image),
            new("MP3", "Audio only", ".mp3", Audio),
            new("WAV", "Lossless audio", ".wav", Audio),
            VideoEditGroup,
            Zip,
            LockZip,
        ])
        {
            Engine = ConversionEngine.FFmpeg,
            Extensions =
            [
                ".mp4", ".mkv", ".mov", ".avi", ".webm", ".wmv", ".flv", ".m4v", ".3gp", ".ts",
                ".3g2", ".mpg", ".mpeg", ".m2ts", ".mts", ".vob", ".ogv", ".asf", ".mxf", ".rm", ".rmvb",
            ],
        },
        new("Audio", Audio, "",
        [
            new("MP3", "320 kbps", ".mp3", Audio),
            new("WAV", "Uncompressed", ".wav", Audio),
            new("FLAC", "Compressed lossless", ".flac", Audio),
            new("OGG", "Vorbis", ".ogg", Audio),
            new("OPUS", "Most efficient", ".opus", Audio),
            new("M4A", "AAC for Apple", ".m4a", Audio),
            new("AAC", "Wide compatibility", ".aac", Audio),
            new("AIFF", "Uncompressed for Apple", ".aiff", Audio),
            AudioEditGroup,
            Zip,
            LockZip,
        ])
        {
            Engine = ConversionEngine.FFmpeg,
            Extensions =
            [
                ".mp3", ".wav", ".flac", ".ogg", ".opus", ".m4a", ".aac", ".wma", ".aiff",
                ".m4b", ".mp2", ".ac3", ".dts", ".amr", ".ape", ".wv", ".tta", ".caf", ".au", ".dsf", ".oga", ".mka", ".weba",
            ],
        },
        new("Document", Document, "",
        [
            new("PDF", "Ready to print & share", ".pdf", Pdf),
            new("DOCX", "Microsoft Word", ".docx", Document),
            new("ODT", "LibreOffice Writer", ".odt", Document),
            new("TXT", "Plain text", ".txt", Document),
            new("HTML", "Web page", ".html", Document),
            new("MD", "Markdown", ".md", Document),
            new("RTF", "Rich Text", ".rtf", Document),
            new("EPUB", "E-book", ".epub", Book),
            MergePdf,
            Zip,
            LockZip,
        ])
        {
            Engine = ConversionEngine.Office,
            Extensions = [".docx", ".doc", ".odt", ".rtf", ".txt", ".md", ".html"],
        },
        // Dikonversi oleh LibreOffice Calc. CSV hanya berisi satu sheet: dari file multi-sheet yang diambil sheet pertama.
        new("Spreadsheet", Sheet, "",
        [
            new("XLSX", "Microsoft Excel", ".xlsx", Sheet),
            new("CSV", "Comma-separated text", ".csv", Sheet),
            new("ODS", "LibreOffice Calc", ".ods", Sheet),
            new("PDF", "Ready to print & share", ".pdf", Pdf),
            new("XLS", "Excel 97-2003", ".xls", Sheet),
            new("HTML", "Web page table", ".html", Document),
            new("JSON", "Structured data", ".json", Code),
            MergePdf,
            Zip,
            LockZip,
        ])
        {
            Engine = ConversionEngine.Office,
            Extensions = [".xlsx", ".xls", ".csv", ".tsv", ".ods"],
        },
        // Dikonversi oleh LibreOffice Impress. PNG/JPG = semua slide (lewat PDF, lalu Ghostscript per halaman).
        new("Slides", Slides, "",
        [
            new("PDF", "Ready to print & share", ".pdf", Pdf),
            new("PPTX", "Microsoft PowerPoint", ".pptx", Slides),
            new("ODP", "LibreOffice Impress", ".odp", Slides),
            new("PNG", "Every slide", ".png", Image),
            new("JPG", "Every slide", ".jpg", Image),
            MergePdf,
            Zip,
            LockZip,
        ])
        {
            Engine = ConversionEngine.Office,
            Extensions = [".pptx", ".ppt", ".odp", ".ppsx", ".pps"],
        },
        // DOCX/ODT oleh LibreOffice; sisanya oleh Ghostscript. PNG/JPG = semua halaman
        // (lebih dari satu halaman → satu folder berisi gambar per halaman).
        new("PDF", Pdf, "",
        [
            Compress with { Description = "Images downscaled to 150 dpi" },
            new("DOCX", "Microsoft Word", ".docx", Document),
            new("ODT", "LibreOffice Writer", ".odt", Document),
            new("PNG", "Every page", ".png", Image),
            new("JPG", "Every page", ".jpg", Image),
            ExtractText with { Description = "Copy all the text" },
            new("SPLIT", "One PDF per page", ".pdf", Stack) { Action = TargetAction.SplitPdf },
            MergePdf,
            Zip,
            LockZip,
        ])
        {
            Engine = ConversionEngine.Office,
            Extensions = [".pdf"],
        },
        // Dibuka oleh 7-Zip (dibundel), termasuk RAR. Membuat RAR tidak bisa: formatnya milik WinRAR.
        new("Archive", ZipFolder, "",
        [
            Extract,
            Zip,
            LockZip,
        ])
        {
            Engine = ConversionEngine.SevenZip,
            Extensions = [".zip", ".rar", ".7z", ".tar", ".gz", ".tgz", ".bz2", ".xz"],
        },
        // Lewat Pandoc; MOBI/AZW butuh Calibre jadi belum didukung.
        new("E-book", Book, "",
        [
            new("PDF", "Ready to print & share", ".pdf", Pdf),
            new("EPUB", "E-book", ".epub", Book),
            new("DOCX", "Microsoft Word", ".docx", Document),
            new("HTML", "Web page", ".html", Document),
            new("TXT", "Plain text", ".txt", Document),
            new("MD", "Markdown", ".md", Document),
            Zip,
            LockZip,
        ])
        {
            Engine = ConversionEngine.Office,
            Extensions = [".epub", ".fb2"],
        },
        new("Subtitle", Captions, "",
        [
            new("SRT", "Most compatible", ".srt", Captions),
            new("VTT", "For the web", ".vtt", Captions),
            new("ASS", "Styled subtitles", ".ass", Captions),
            new("LRC", "Song lyrics", ".lrc", Captions),
            Zip,
            LockZip,
        ])
        {
            Engine = ConversionEngine.FFmpeg,
            Extensions = [".srt", ".vtt", ".ass", ".ssa", ".smi", ".lrc"],
        },
        new("Data", Code, "",
        [
            new("JSON", "Structured data", ".json", Code),
            new("YAML", "Readable config", ".yaml", Code),
            new("CSV", "Comma-separated text", ".csv", Sheet),
            Zip,
            LockZip,
        ])
        {
            Engine = ConversionEngine.Data,
            Extensions = [".json", ".yaml"],
        },
    ];

    /// <summary>
    /// Kategori roda untuk semua file yang di-drag: biasanya ditentukan file pertama, kecuali pasangan
    /// video + subtitle (urutan bebas), yang pilihannya BURN IN dan ZIP.
    /// </summary>
    public static FileCategory ForSelection(IReadOnlyList<string> files)
    {
        if (files.Count == 2 && SubtitlePair(files) is { } pair)
            return ForFile(pair.Video, multipleFiles: true) with { Targets = [BurnSubtitles, Zip] };
        return ForFile(files[0], multipleFiles: files.Count > 1);
    }

    /// <summary>Video dan subtitle di antara dua file ini; null kalau bukan pasangan seperti itu.</summary>
    public static (string Video, string Subtitle)? SubtitlePair(IReadOnlyList<string> files)
    {
        if (files.Count != 2)
            return null;
        string first = ForFile(files[0]).Name, second = ForFile(files[1]).Name;
        return (first, second) switch
        {
            ("Video", "Subtitle") => (files[0], files[1]),
            ("Subtitle", "Video") => (files[1], files[0]),
            _ => null,
        };
    }

    /// <summary>Semua pilihan yang benar-benar dikerjakan: isi grup ikut, irisan grupnya sendiri tidak.</summary>
    public static IEnumerable<ConvertTarget> Leaves(FileCategory category) =>
        category.Targets.SelectMany(t => t.Children ?? new[] { t });

    /// <summary>
    /// Isi halaman grup dengan BACK di posisi <paramref name="backIndex"/>. Roda menaruh BACK tepat di arah
    /// kursor, supaya halaman baru tidak langsung memilih alat yang belum sempat dilihat.
    /// </summary>
    public static IReadOnlyList<ConvertTarget> PageOf(ConvertTarget group, int backIndex)
    {
        var page = group.Children!.ToList();
        page.Insert(Math.Clamp(backIndex, 0, page.Count), Back);
        return page;
    }

    /// <summary>
    /// Kategori untuk file tertentu, dengan format yang sama dengan file sumber dibuang
    /// dari daftar tujuan. Folder dan file yang jenisnya belum didukung masuk kategori
    /// "Folder"/"File" yang hanya berisi ZIP.
    /// </summary>
    /// <param name="multipleFiles">True kalau file ini bagian dari beberapa file yang di-drag sekaligus.</param>
    public static FileCategory ForFile(string path, bool multipleFiles = false)
    {
        string name = Path.GetFileName(path.TrimEnd('\\', '/'));
        if (Directory.Exists(path))
            return Other with { Name = "Folder", Glyph = Folder, FileName = name };

        string extension = Normalize(Path.GetExtension(path));
        var category = Categories.FirstOrDefault(c => c.Extensions.Contains(extension));
        if (category is null)
            return Other with { FileName = name };

        return category with
        {
            FileName = name,
            Targets = category.Targets.Where(t => t.Action switch
            {
                TargetAction.Convert => t.Extension != extension,
                TargetAction.MergePdf or TargetAction.Zip or TargetAction.ContactSheet => multipleFiles,
                _ => true,
            }).ToList(),
        };
    }

    /// <summary>Pilihan untuk folder (menu klik kanan folder di Explorer).</summary>
    public static IReadOnlyList<ConvertTarget> FolderTargets => Other.Targets;

    /// <summary>False untuk folder dan file yang jenisnya belum didukung (hanya bisa di-ZIP).</summary>
    public static bool IsSupported(FileCategory category) => category.Engine != ConversionEngine.None;

    /// <summary>
    /// Kode tujuan untuk baris perintah menu klik kanan ("png", "compress", ...): tetap sama
    /// walau label atau deskripsi di roda diubah.
    /// </summary>
    public static string IdOf(ConvertTarget target) => target.Action switch
    {
        TargetAction.Convert => target.Extension.TrimStart('.'),
        TargetAction.Compress => "compress",
        TargetAction.MergePdf => MergeId,
        TargetAction.ExtractText => "text",
        TargetAction.Zip => ZipId,
        TargetAction.Extract => "extract",
        TargetAction.Group => "edit",
        TargetAction.BurnSubtitles => "burn",
        TargetAction.ZipPassword => LockId,
        TargetAction.ContactSheet => SheetId,
        TargetAction.Edit => target.Edit switch
        {
            ImageEdit.RotateRight => "rotate-right",
            ImageEdit.RotateLeft => "rotate-left",
            ImageEdit.FlipHorizontal => "flip-horizontal",
            ImageEdit.FlipVertical => "flip-vertical",
            ImageEdit.Reverse => "reverse",
            ImageEdit.Normalize => "normalize",
            ImageEdit.Cover => "cover",
            ImageEdit.Thumbnails => "thumbnails",
            _ => "grayscale",
        },
        _ => "split",
    };

    public const string MergeId = "merge";
    public const string ZipId = "zip";
    public const string LockId = "zip-lock";
    public const string SheetId = "sheet";

    /// <summary>Teks di menu klik kanan Explorer, dalam bahasa yang sedang dipakai.</summary>
    public static string MenuTextOf(ConvertTarget target) => target.Action switch
    {
        TargetAction.Convert => L.F("To {0}", target.Label),
        TargetAction.Compress => L.T("Compress (shrink size)"),
        TargetAction.MergePdf => L.T("Merge into 1 PDF"),
        TargetAction.ExtractText => L.T("Copy text (OCR)"),
        TargetAction.Zip => L.T("Pack into ZIP"),
        TargetAction.Extract => L.T("Extract here"),
        TargetAction.Edit or TargetAction.ZipPassword or TargetAction.ContactSheet => L.T(target.Description),
        _ => L.T("Split into pages"),
    };

    /// <summary>File yang bisa ikut digabung jadi satu PDF (gambar, PDF, dokumen, tabel).</summary>
    public static bool CanMergeToPdf(string path) =>
        ForFile(path, multipleFiles: true).Targets.Contains(MergePdf);

    /// <summary>
    /// File yang ikut saat beberapa file dilepas di <paramref name="target"/> pada roda kategori
    /// <paramref name="category"/>. Gabung PDF menerima campuran gambar/PDF/dokumen, diurutkan
    /// dengan <paramref name="nameOrder"/> (urutan nama seperti di file manager); ZIP menerima
    /// semuanya, termasuk folder. Selain itu hanya file sejenis yang bisa dikonversi ke format ini.
    /// </summary>
    public static List<string> FilesFor(ConvertTarget target, FileCategory category, IEnumerable<string> files,
        IComparer<string?> nameOrder) => target.Action switch
    {
        TargetAction.MergePdf => files.Where(CanMergeToPdf).OrderBy(Path.GetFileName, nameOrder).ToList(),
        TargetAction.Zip or TargetAction.BurnSubtitles or TargetAction.ZipPassword => files.ToList(),
        TargetAction.ContactSheet => files.Where(file => ForFile(file).Name == "Image").OrderBy(Path.GetFileName, nameOrder).ToList(),
        _ => files
            .Where(file => ForFile(file) is var own && own.Name == category.Name && Leaves(own).Contains(target))
            .ToList(),
    };

    public static string Normalize(string extension) => extension.ToLowerInvariant() switch
    {
        ".jpeg" => ".jpg",
        ".tif" => ".tiff",
        ".htm" => ".html",
        ".yml" => ".yaml",
        ".aif" => ".aiff",
        var other => other,
    };

    /// <summary>Ejaan lain dari ekstensi katalog; ikut didaftarkan ke menu klik kanan.</summary>
    public static readonly IReadOnlyList<string> AliasExtensions = [".jpeg", ".tif", ".htm", ".yml", ".aif"];
}
