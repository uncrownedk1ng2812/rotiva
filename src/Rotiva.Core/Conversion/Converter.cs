using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Rotiva.Models;

namespace Rotiva.Conversion;

public sealed class ConversionException(string message) : Exception(message);

/// <summary>
/// Menjalankan satu konversi. Hasil disimpan di folder yang sama dengan file asli,
/// dengan nama yang sama dan ekstensi baru; kalau sudah ada, diberi akhiran " (1)", " (2)", dst.
/// </summary>
public static partial class Converter
{
    /// <summary>Mesin OCR sistem operasi untuk segmen TEXT; diisi aplikasi saat start.</summary>
    public static ITextRecognizer? TextRecognizer { get; set; }

    static ITextRecognizer Ocr => TextRecognizer
        ?? throw new ConversionException("Text recognition isn't available on this system.");

    // Batas waktu untuk gambar & dokumen. FFmpeg tidak dibatasi karena video panjang memang lama.
    static readonly TimeSpan ToolTimeout = TimeSpan.FromMinutes(3);

    // Format gambar yang hasil kompresnya disimpan sebagai JPG (tidak ada mode lossy yang berguna).
    static readonly string[] CompressToJpg = [".bmp", ".tiff", ".heic", ".heif", ".jfif"];

    public static async Task<string> ConvertAsync(string source, ConvertTarget target, IProgress<double?> progress)
    {
        var category = FormatCatalog.ForFile(source);
        if (!FormatCatalog.IsSupported(category))
            throw new ConversionException("This file type is not supported.");
        if (target.Action == TargetAction.Extract)
            return await ExtractArchiveAsync(source);
        if (target.Action == TargetAction.Compress)
            return await CompressAsync(source, category.Engine, progress);
        if (target.Action == TargetAction.ExtractText)
            return await ExtractTextAsync(source, category.Engine, progress);

        // PDF → gambar / PDF per halaman: semua halaman, lewat Ghostscript (LibreOffice hanya halaman pertama).
        if (FormatCatalog.Normalize(Path.GetExtension(source)) == ".pdf")
        {
            string[] render = ["-r150", "-dTextAlphaBits=4", "-dGraphicsAlphaBits=4"];
            switch (target)
            {
                case { Action: TargetAction.SplitPdf }:
                    return await SplitPdfPagesAsync(source, ".pdf", "pdfwrite", [], " (pages)");
                case { Extension: ".png" }:
                    return await SplitPdfPagesAsync(source, ".png", "png16m", render, " (images)");
                case { Extension: ".jpg" }:
                    return await SplitPdfPagesAsync(source, ".jpg", "jpeg", [.. render, "-dJPEGQ=90"], " (images)");
            }
        }

        string output = UniqueOutputPath(source, target.Extension);
        await ProduceAsync(output, () => ConvertToAsync(source, category.Engine, target.Extension, output, progress));
        return output;
    }

    static Task ConvertToAsync(string source, ConversionEngine engine, string extension, string output,
        IProgress<double?> progress) => engine switch
    {
        ConversionEngine.ImageMagick => ConvertImageAsync(source, extension, output),
        ConversionEngine.FFmpeg => ConvertMediaAsync(source, extension, output, progress),
        _ => ConvertDocumentAsync(source, extension, output),
    };

    /// <summary>Jalankan <paramref name="work"/>; kalau gagal atau tidak menghasilkan file, sisa output dihapus.</summary>
    static async Task ProduceAsync(string output, Func<Task> work)
    {
        try
        {
            await work();
            if (!File.Exists(output))
                throw new ConversionException("The converter did not produce a file.");
        }
        catch
        {
            TryDelete(output);
            throw;
        }
    }

    /// <summary>
    /// Perkecil ukuran file. Hasilnya "nama (kompres).ext"; kalau ternyata tidak lebih kecil
    /// dari aslinya, hasil dibuang supaya tidak ada file yang sia-sia.
    /// </summary>
    static async Task<string> CompressAsync(string source, ConversionEngine engine, IProgress<double?> progress)
    {
        string extension = FormatCatalog.Normalize(Path.GetExtension(source));
        string outputExtension = engine switch
        {
            ConversionEngine.FFmpeg => ".mp4",
            _ when CompressToJpg.Contains(extension) => ".jpg",
            _ => extension,
        };
        string output = UniqueOutputPath(source, outputExtension, " (compressed)");
        await ProduceAsync(output, async () =>
        {
            switch (engine)
            {
                case ConversionEngine.ImageMagick:
                    await CompressImageAsync(source, outputExtension, output);
                    break;
                case ConversionEngine.FFmpeg:
                    await ConvertMediaAsync(source, ".mp4", output, progress, compress: true);
                    break;
                default:
                    await CompressPdfAsync(source, output);
                    break;
            }
            if (new FileInfo(output).Length >= new FileInfo(source).Length)
                throw new ConversionException("The file is already small and can't be compressed further.");
        });
        return output;
    }

    /// <summary>
    /// Ambil teks dari gambar (OCR) atau PDF ke "nama.txt". PDF yang punya teks asli dibaca
    /// langsung; PDF hasil scan (tanpa teks) dirender per halaman lalu di-OCR.
    /// </summary>
    static async Task<string> ExtractTextAsync(string source, ConversionEngine engine, IProgress<double?> progress)
    {
        string output = UniqueOutputPath(source, ".txt");
        await ProduceAsync(output, () => WithTempDirectoryAsync(async tempDir =>
        {
            string text = engine == ConversionEngine.ImageMagick
                ? await OcrImageAsync(source, tempDir)
                : await ExtractPdfTextAsync(source, tempDir, progress);
            if (string.IsNullOrWhiteSpace(text))
                throw new ConversionException("No readable text was found.");
            await File.WriteAllTextAsync(output, text.Trim() + Environment.NewLine);
        }));
        return output;
    }

    static async Task<string> OcrImageAsync(string source, string tempDir)
    {
        // Lewat ImageMagick dulu: semua format (HEIC, AVIF, ...) jadi PNG, rotasi EXIF diterapkan,
        // dan ukuran dijaga di bawah batas mesin OCR.
        string png = Path.Combine(tempDir, "ocr.png");
        uint max = Ocr.MaxImageDimension;
        await RunMagickAsync(Magick,
        [
            source + "[0]", "-auto-orient", "-resize", $"{max}x{max}>",
            "-background", "white", "-alpha", "remove", "-alpha", "off", png,
        ]);
        return await Ocr.RecognizeAsync(png);
    }

    static async Task<string> ExtractPdfTextAsync(string source, string tempDir, IProgress<double?> progress)
    {
        string txt = Path.Combine(tempDir, "text.txt");
        await RunGhostscriptAsync("txtwrite", [], [source], txt);
        string text = File.Exists(txt) ? await File.ReadAllTextAsync(txt) : "";
        // Ada teks asli (bukan sekadar nomor halaman atau sisa spasi): pakai itu, jauh lebih akurat dari OCR.
        if (text.Count(c => !char.IsWhiteSpace(c)) >= 100)
            return text;

        // PDF hasil scan: render tiap halaman 200 dpi (cukup tajam untuk OCR), lalu baca satu per satu.
        await RunGhostscriptAsync("pnggray", ["-r200"], [source],
            Path.Combine(tempDir.Replace("%", "%%"), "page-%04d.png"), escapeOutput: false);
        var pages = Directory.GetFiles(tempDir, "page-*.png").Order(StringComparer.Ordinal).ToList();
        var result = new StringBuilder();
        for (int i = 0; i < pages.Count; i++)
        {
            progress.Report((double)i / pages.Count);
            if (i > 0)
                result.AppendLine();
            result.Append(await Ocr.RecognizeAsync(pages[i]));
        }
        return result.ToString();
    }

    /// <summary>
    /// Satu file per halaman PDF di folder baru "nama (gambar)" / "nama (per halaman)", bernama
    /// "nama-01.jpg", "nama-02.jpg", dst. PDF satu halaman → gambar langsung di samping file asli.
    /// Mengembalikan path folder (atau file, untuk PDF satu halaman).
    /// </summary>
    static async Task<string> SplitPdfPagesAsync(string source, string extension, string device, string[] options,
        string folderSuffix)
    {
        string folder = UniqueOutputPath(source, "", folderSuffix);
        Directory.CreateDirectory(folder);
        try
        {
            await RunGhostscriptAsync(device, options, [source],
                Path.Combine(folder.Replace("%", "%%"), "page-%05d" + extension), escapeOutput: false);
            var pages = Directory.GetFiles(folder).Order(StringComparer.Ordinal).ToList();
            if (pages.Count == 0)
                throw new ConversionException("Ghostscript did not produce a file.");

            if (pages.Count == 1)
            {
                if (extension == ".pdf")
                    throw new ConversionException("This PDF has only 1 page; nothing to split.");
                string single = UniqueOutputPath(source, extension);
                File.Move(pages[0], single);
                Directory.Delete(folder);
                return single;
            }

            string name = Path.GetFileNameWithoutExtension(source);
            int width = Math.Max(2, pages.Count.ToString(CultureInfo.InvariantCulture).Length);
            for (int i = 0; i < pages.Count; i++)
                File.Move(pages[i], Path.Combine(folder, $"{name}-{(i + 1).ToString(CultureInfo.InvariantCulture).PadLeft(width, '0')}{extension}"));
            return folder;
        }
        catch
        {
            // Folder ini baru dibuat di atas (namanya unik), jadi aman dihapus seluruhnya.
            try
            {
                Directory.Delete(folder, recursive: true);
            }
            catch (IOException)
            {
            }
            throw;
        }
    }

    static async Task WithTempDirectoryAsync(Func<string, Task> action)
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"rotiva-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            await action(tempDir);
        }
        finally
        {
            try
            {
                Directory.Delete(tempDir, recursive: true);
            }
            catch (IOException)
            {
            }
        }
    }

    /// <summary>
    /// Gabungkan beberapa file jadi satu PDF bernama "Merged.pdf" di folder file pertama.
    /// File yang belum PDF (gambar, dokumen, tabel) diubah ke PDF dulu di folder sementara.
    /// </summary>
    public static async Task<string> MergeToPdfAsync(IReadOnlyList<string> sources, IProgress<double?> progress)
    {
        string output = UniqueOutputPath(Path.Combine(Path.GetDirectoryName(sources[0])!, "Merged.pdf"), ".pdf");
        await WithTempDirectoryAsync(async tempDir =>
        {
            var pdfs = new List<string>();
            for (int i = 0; i < sources.Count; i++)
            {
                progress.Report((double)i / (sources.Count + 1));
                string source = sources[i];
                var category = FormatCatalog.ForFile(source);
                if (FormatCatalog.Normalize(Path.GetExtension(source)) == ".pdf")
                {
                    pdfs.Add(source);
                    continue;
                }
                string pdf = Path.Combine(tempDir, $"{i}.pdf");
                await ProduceAsync(pdf, () => ConvertToAsync(source, category.Engine, ".pdf", pdf, progress));
                pdfs.Add(pdf);
            }

            progress.Report((double)sources.Count / (sources.Count + 1));
            await ProduceAsync(output, () => RunGhostscriptAsync("pdfwrite", [], pdfs, output));
        });
        return output;
    }

    /// <summary>
    /// Jadikan file dan folder yang di-drag satu .zip di folder item pertama: satu item jadi
    /// "nama.zip", beberapa item jadi "Arsip.zip". Folder ikut beserta isinya (termasuk subfolder kosong).
    /// </summary>
    public static async Task<string> ZipAsync(IReadOnlyList<string> sources, IProgress<double?> progress)
    {
        string first = sources[0].TrimEnd('\\', '/');
        string directory = Path.GetDirectoryName(first)
            ?? throw new ConversionException("A whole drive can't be zipped; pick a folder inside it.");
        string name = sources.Count > 1 ? "Archive"
            : Directory.Exists(first) ? Path.GetFileName(first)
            : Path.GetFileNameWithoutExtension(first);
        string output = UniquePath(directory, name, ".zip");

        await ProduceAsync(output, () => Task.Run(() =>
        {
            // (path di disk, nama di dalam zip); path null = folder kosong.
            var entries = new List<(string? Path, string Name)>();
            foreach (string source in sources)
            {
                string item = source.TrimEnd('\\', '/');
                string root = Path.GetFileName(item);
                if (!Directory.Exists(item))
                {
                    entries.Add((item, root));
                    continue;
                }
                foreach (string dir in Directory.EnumerateDirectories(item, "*", SearchOption.AllDirectories).Prepend(item))
                {
                    string prefix = dir == item ? root : Path.Join(root, Path.GetRelativePath(item, dir)).Replace('\\', '/');
                    var files = Directory.GetFiles(dir);
                    if (files.Length == 0 && !Directory.EnumerateDirectories(dir).Any())
                        entries.Add((null, prefix + "/"));
                    entries.AddRange(files.Select(file => ((string?)file, $"{prefix}/{Path.GetFileName(file)}")));
                }
            }

            long total = Math.Max(1, entries.Sum(e => e.Path is null ? 0 : new FileInfo(e.Path).Length));
            long done = 0;
            using var zip = ZipFile.Open(output, ZipArchiveMode.Create);
            foreach (var (path, entryName) in entries)
            {
                if (path is null)
                {
                    zip.CreateEntry(entryName);
                    continue;
                }
                zip.CreateEntryFromFile(path, entryName, CompressionLevel.Optimal);
                done += new FileInfo(path).Length;
                progress.Report((double)done / total);
            }
        }));
        return output;
    }

    /// <summary>
    /// Buka arsip (.zip, .rar, .7z, .tar.gz, ...) ke folder baru bernama sama di sebelahnya, lewat 7-Zip.
    /// </summary>
    static async Task<string> ExtractArchiveAsync(string source)
    {
        string sevenZip = ToolLocator.SevenZip
            ?? throw new ConversionException("7-Zip is not installed (winget install 7zip.7zip).");
        string name = Path.GetFileNameWithoutExtension(source);
        if (name.EndsWith(".tar", StringComparison.OrdinalIgnoreCase))
            name = name[..^4];
        string folder = UniquePath(Path.GetDirectoryName(source)!, name, "");
        Directory.CreateDirectory(folder);
        try
        {
            // "-p" tanpa isi: arsip berpassword langsung gagal, bukan menunggu input password selamanya.
            string[] quiet = ["-y", "-p", "-bso0", "-bsp0"];
            await RunSevenZipAsync(sevenZip, ["x", source, "-o" + folder, .. quiet]);

            // .tar.gz/.tgz/.tar.xz: langkah pertama hanya menghasilkan satu .tar; buka sekali lagi.
            if (Directory.GetFileSystemEntries(folder) is [var inner]
                && File.Exists(inner) && inner.EndsWith(".tar", StringComparison.OrdinalIgnoreCase)
                && !source.EndsWith(".tar", StringComparison.OrdinalIgnoreCase))
            {
                await RunSevenZipAsync(sevenZip, ["x", inner, "-o" + folder, .. quiet]);
                File.Delete(inner);
            }
            if (Directory.GetFileSystemEntries(folder).Length == 0)
                throw new ConversionException("The archive is empty.");
            return folder;
        }
        catch
        {
            // Folder ini baru dibuat di atas (namanya unik), jadi aman dihapus seluruhnya.
            try
            {
                Directory.Delete(folder, recursive: true);
            }
            catch (IOException)
            {
            }
            throw;
        }
    }

    static async Task RunSevenZipAsync(string sevenZip, IEnumerable<string> args)
    {
        try
        {
            await RunAsync(sevenZip, args, timeout: TimeSpan.FromMinutes(30));
        }
        catch (ConversionException ex) when (ex.Message.Contains("password", StringComparison.OrdinalIgnoreCase))
        {
            throw new ConversionException("The archive is password-protected. Open it with 7-Zip or WinRAR.");
        }
    }

    static string UniqueOutputPath(string source, string extension, string suffix = "") =>
        UniquePath(Path.GetDirectoryName(source)!, Path.GetFileNameWithoutExtension(source) + suffix, extension);

    static string UniquePath(string directory, string name, string extension)
    {
        string candidate = Path.Combine(directory, name + extension);
        for (int i = 1; File.Exists(candidate) || Directory.Exists(candidate); i++)
            candidate = Path.Combine(directory, $"{name} ({i}){extension}");
        return candidate;
    }

    static string Magick => ToolLocator.ImageMagick
        ?? throw new ConversionException("ImageMagick is not installed (winget install ImageMagick.ImageMagick).");

    // MAGICK_HOME supaya ImageMagick yang dibundel memakai config di foldernya sendiri.
    static Task RunMagickAsync(string magick, List<string> args) =>
        RunAsync(magick, args, environment: new() { ["MAGICK_HOME"] = Path.GetDirectoryName(magick)! },
            timeout: ToolTimeout);

    static Task CompressImageAsync(string source, string extension, string output)
    {
        string magick = Magick;
        bool animated = extension == ".gif";

        // Sisi terpanjang maks. 2560 px (">" = hanya diperkecil, tidak pernah diperbesar).
        // Metadata EXIF (termasuk lokasi GPS) dibuang, profil warna ICC dipertahankan.
        var args = new List<string> { animated ? source : source + "[0]", "-auto-orient" };
        if (animated)
            args.Add("-coalesce");
        args.AddRange(["-resize", "2560x2560>", "+profile", "!icc,*"]);
        switch (extension)
        {
            case ".jpg":
                args.AddRange(["-background", "white", "-alpha", "remove", "-alpha", "off",
                               "-sampling-factor", "4:2:0", "-interlace", "JPEG", "-quality", "80"]);
                break;
            case ".webp":
                args.AddRange(["-quality", "78"]);
                break;
            case ".avif":
                args.AddRange(["-quality", "60"]);
                break;
            case ".png":
                args.AddRange(["-define", "png:compression-level=9"]);
                break;
            case ".gif":
                args.AddRange(["-layers", "Optimize"]);
                break;
        }
        args.Add(output);
        return RunMagickAsync(magick, args);
    }

    static Task ConvertImageAsync(string source, string extension, string output)
    {
        string magick = Magick;

        // [0] = frame/halaman pertama saja, supaya GIF animasi tidak pecah jadi banyak file.
        var args = new List<string> { source + "[0]", "-auto-orient" };
        switch (extension)
        {
            case ".jpg":
                args.AddRange(["-background", "white", "-alpha", "remove", "-alpha", "off", "-quality", "92"]);
                break;
            case ".bmp":
                args.AddRange(["-background", "white", "-alpha", "remove", "-alpha", "off"]);
                break;
            case ".webp":
                args.AddRange(["-quality", "90"]);
                break;
            case ".avif":
                args.AddRange(["-quality", "80"]);
                break;
            case ".ico":
                args.AddRange(["-background", "none", "-define", "icon:auto-resize=256,128,64,48,32,16"]);
                break;
        }
        args.Add(output);
        return RunMagickAsync(magick, args);
    }

    /// <summary>
    /// Kompres PDF dengan Ghostscript preset /ebook: gambar diperkecil ke 150 dpi, teks tetap tajam.
    /// Paling terasa untuk PDF hasil scan atau berisi banyak foto.
    /// </summary>
    static Task CompressPdfAsync(string source, string output) =>
        RunGhostscriptAsync("pdfwrite", ["-dPDFSETTINGS=/ebook", "-dDetectDuplicateImages=true"], [source], output);

    /// <param name="escapeOutput">
    /// "%" di nama output dianggap format nomor halaman oleh Ghostscript, jadi di-escape; false kalau
    /// <paramref name="output"/> memang pola seperti "page-%04d.png" (bagian folder harus sudah di-escape).
    /// </param>
    static Task RunGhostscriptAsync(string device, string[] options, IEnumerable<string> sources, string output,
        bool escapeOutput = true)
    {
        string gs = ToolLocator.Ghostscript
            ?? throw new ConversionException("Ghostscript is not installed (needed for PDF compress, merge and text).");
        string[] pdfOptions = device == "pdfwrite" ? ["-dCompatibilityLevel=1.5"] : [];
        return RunAsync(gs,
        [
            "-q", "-dNOPAUSE", "-dBATCH", "-dSAFER", $"-sDEVICE={device}", .. pdfOptions, .. options,
            "-sOutputFile=" + (escapeOutput ? output.Replace("%", "%%") : output), .. sources,
        ], timeout: TimeSpan.FromMinutes(10));
    }

    static async Task ConvertMediaAsync(string source, string extension, string output, IProgress<double?> progress,
        bool compress = false)
    {
        string ffmpeg = ToolLocator.FFmpeg
            ?? throw new ConversionException("FFmpeg is not installed (winget install Gyan.FFmpeg).");

        string[] codecArgs = compress
            // Sisi terpendek maks. 1080 px (portrait maupun landscape), CRF 28 ≈ kualitas video medsos.
            ? ["-vf", "scale='if(gt(iw,ih),-2,trunc(min(1080,iw)/2)*2)':'if(gt(iw,ih),trunc(min(1080,ih)/2)*2,-2)'",
               "-c:v", "libx264", "-preset", "medium", "-crf", "28", "-pix_fmt", "yuv420p",
               "-c:a", "aac", "-b:a", "128k", "-movflags", "+faststart"]
            : extension switch
        {
            ".mp4" or ".mov" => ["-c:v", "libx264", "-preset", "medium", "-crf", "20", "-pix_fmt", "yuv420p",
                                 "-c:a", "aac", "-b:a", "192k", "-movflags", "+faststart"],
            ".webm" => ["-c:v", "libvpx-vp9", "-crf", "32", "-b:v", "0", "-row-mt", "1", "-c:a", "libopus", "-b:a", "128k"],
            ".mkv" => ["-map", "0", "-c", "copy"],
            ".avi" => ["-c:v", "mpeg4", "-q:v", "3", "-c:a", "libmp3lame", "-q:a", "2"],
            ".gif" => ["-an", "-vf", "fps=15,scale='min(720,iw)':-2:flags=lanczos,split[a][b];[a]palettegen[p];[b][p]paletteuse",
                       "-loop", "0"],
            ".mp3" => ["-vn", "-c:a", "libmp3lame", "-b:a", "320k"],
            ".wav" => ["-vn", "-c:a", "pcm_s16le"],
            ".flac" => ["-vn", "-c:a", "flac"],
            ".ogg" => ["-vn", "-c:a", "libvorbis", "-q:a", "6"],
            ".opus" => ["-vn", "-c:a", "libopus", "-b:a", "160k"],
            ".m4a" => ["-vn", "-c:a", "aac", "-b:a", "256k"],
            _ => throw new ConversionException($"The {extension} format is not supported yet."),
        };

        // Progres dihitung dari durasi total (stderr) dan posisi saat ini (-progress ke stdout).
        double totalSeconds = 0;
        progress.Report(null);
        await RunAsync(ffmpeg,
            ["-hide_banner", "-nostdin", "-y", "-i", source, .. codecArgs, "-progress", "pipe:1", "-nostats", output],
            onStdout: line =>
            {
                if (totalSeconds > 0 && line.StartsWith("out_time_us=", StringComparison.Ordinal)
                    && long.TryParse(line.AsSpan("out_time_us=".Length), out long micros))
                {
                    progress.Report(Math.Clamp(micros / 1_000_000.0 / totalSeconds, 0, 1));
                }
            },
            onStderr: line =>
            {
                if (totalSeconds == 0 && DurationPattern().Match(line) is { Success: true } match)
                    totalSeconds = TimeSpan.ParseExact(match.Groups[1].Value, @"hh\:mm\:ss\.ff", CultureInfo.InvariantCulture).TotalSeconds;
            });
    }

    static async Task ConvertDocumentAsync(string source, string extension, string output)
    {
        string sourceExtension = FormatCatalog.Normalize(Path.GetExtension(source));

        // Pembagian kerja:
        // - LibreOffice tidak bisa membaca/menulis Markdown, dan ekspor TXT-nya macet (bug), jadi
        //   tujuan .txt/.md dan sumber .md ditangani Pandoc.
        // - Pandoc tidak bisa membaca .doc lama, jadi .doc diubah dulu ke .docx oleh LibreOffice.
        // - Pandoc tidak bisa membuat PDF tanpa LaTeX, jadi Markdown → PDF lewat .docx lalu LibreOffice.
        bool viaPandoc = extension is ".txt" or ".md" || sourceExtension == ".md";
        if (!viaPandoc)
        {
            // CSV: pisah koma, tanda kutip ", UTF-8 (76), mulai baris 1.
            string filter = extension == ".csv" ? "csv:Text - txt - csv (StarCalc):44,34,76,1" : extension.TrimStart('.');
            await RunLibreOfficeAsync(source, filter, output);
            return;
        }

        if (extension == ".pdf")
        {
            await WithTempFileAsync(".docx", async docx =>
            {
                await RunPandocAsync(source, ".docx", docx);
                await RunLibreOfficeAsync(docx, "pdf", output);
            });
        }
        else if (sourceExtension == ".doc")
        {
            await WithTempFileAsync(".docx", async docx =>
            {
                await RunLibreOfficeAsync(source, "docx", docx);
                await RunPandocAsync(docx, extension, output);
            });
        }
        else
        {
            await RunPandocAsync(source, extension, output);
        }
    }

    static Task RunPandocAsync(string source, string extension, string output)
    {
        string pandoc = ToolLocator.Pandoc
            ?? throw new ConversionException("Pandoc is not installed (winget install JohnMacFarlane.Pandoc).");

        // Pandoc menebak format dari ekstensi; .txt (dibaca sebagai Markdown) dan .md perlu disebutkan.
        string[] reader = FormatCatalog.Normalize(Path.GetExtension(source)) is ".txt" or ".md" ? ["-f", "markdown"] : [];
        string[] writer = extension switch
        {
            ".txt" => ["-t", "plain"],
            ".md" => ["-t", "gfm"],
            _ => [],
        };
        return RunAsync(pandoc, [source, .. reader, "-s", .. writer, "-o", output], timeout: ToolTimeout);
    }

    static async Task WithTempFileAsync(string extension, Func<string, Task> action)
    {
        string temp = Path.Combine(Path.GetTempPath(), $"rotiva-{Guid.NewGuid():N}{extension}");
        try
        {
            await action(temp);
        }
        finally
        {
            TryDelete(temp);
        }
    }

    static async Task RunLibreOfficeAsync(string source, string filter, string output)
    {
        string soffice = ToolLocator.LibreOffice
            ?? throw new ConversionException("LibreOffice is not installed (winget install TheDocumentFoundation.LibreOffice).");

        // LibreOffice selalu menamai hasil "<nama asli>.<ext>" di --outdir, jadi pakai folder
        // sementara lalu pindahkan. Profil terpisah supaya tidak bentrok dengan LibreOffice yang sedang dibuka.
        string tempDir = Path.Combine(Path.GetTempPath(), $"rotiva-{Guid.NewGuid():N}");
        string profile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Rotiva", "libreoffice-profile");
        Directory.CreateDirectory(tempDir);
        try
        {
            // HTML defaultnya dibuka sebagai "Writer/Web" yang tidak bisa ekspor ke DOCX/RTF, dan PDF
            // defaultnya dibuka di Draw (cukup untuk PNG/JPG, tapi tidak bisa ke DOCX/ODT);
            // paksa dibuka sebagai dokumen Writer biasa.
            string[] importFilter = FormatCatalog.Normalize(Path.GetExtension(source)) switch
            {
                ".html" => ["--infilter=HTML (StarWriter)"],
                ".pdf" when filter is "docx" or "odt" => ["--infilter=writer_pdf_import"],
                // Tanpa ini LibreOffice menanyakan pemisah kolom, atau membuka CSV sebagai dokumen Writer.
                ".csv" or ".tsv" => [$"--infilter=CSV:{CsvImportOptions(source)}"],
                _ => [],
            };
            await RunAsync(soffice,
            [
                $"-env:UserInstallation={new Uri(profile).AbsoluteUri}",
                "--headless", "--norestore", .. importFilter, "--convert-to", filter, "--outdir", tempDir, source,
            ], timeout: ToolTimeout);
            string produced = Directory.EnumerateFiles(tempDir).FirstOrDefault()
                ?? throw new ConversionException("LibreOffice did not produce a file.");
            File.Move(produced, output);
        }
        finally
        {
            try
            {
                Directory.Delete(tempDir, recursive: true);
            }
            catch (IOException)
            {
            }
        }
    }

    /// <summary>
    /// Opsi filter CSV LibreOffice: "pemisah,kutip,charset,baris awal[,,bahasa]". Pemisah ditebak dari baris
    /// pertama (Excel versi Indonesia menyimpan CSV dengan titik koma), charset UTF-8 (76) kalau
    /// isinya UTF-8 valid, selain itu ANSI Windows-1252 (1).
    /// </summary>
    static string CsvImportOptions(string path)
    {
        byte[] head = new byte[64 * 1024];
        int length;
        using (var stream = File.OpenRead(path))
            length = stream.ReadAtLeast(head, head.Length, throwOnEndOfStream: false);
        // Jangan sampai karakter multibyte yang terpotong di ujung buffer membuatnya dianggap bukan UTF-8.
        int checkLength = length == head.Length ? Math.Max(0, length - 4) : length;
        bool utf8 = true;
        try
        {
            new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(head, 0, checkLength);
        }
        catch (DecoderFallbackException)
        {
            utf8 = false;
        }

        string text = (utf8 ? Encoding.UTF8 : Encoding.Latin1).GetString(head, 0, length);
        string firstLine = text.Split('\n').FirstOrDefault(line => line.Trim().Length > 0) ?? "";
        int Count(char c)
        {
            int count = 0;
            bool quoted = false;
            foreach (char ch in firstLine)
            {
                if (ch == '"')
                    quoted = !quoted;
                else if (ch == c && !quoted)
                    count++;
            }
            return count;
        }
        char separator = new[] { ',', ';', '\t', '|' }.MaxBy(Count);
        if (Count(separator) == 0)
            separator = FormatCatalog.Normalize(Path.GetExtension(path)) == ".tsv" ? '\t' : ',';

        // CSV titik koma biasanya memakai koma desimal ("1500,5"): baca angkanya dengan bahasa Indonesia (LCID 1057).
        string language = separator == ';' ? ",,1057" : "";
        return $"{(int)separator},34,{(utf8 ? 76 : 1)},1{language}";
    }

    static async Task RunAsync(string exe, IEnumerable<string> args,
        Action<string>? onStdout = null, Action<string>? onStderr = null, Dictionary<string, string>? environment = null,
        TimeSpan? timeout = null)
    {
        var startInfo = new ProcessStartInfo(exe)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };
        foreach (var arg in args)
            startInfo.ArgumentList.Add(arg);
        if (environment is not null)
        {
            foreach (var (name, value) in environment)
                startInfo.Environment[name] = value;
        }

        using var process = new Process { StartInfo = startInfo };
        var errorTail = new Queue<string>();
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null)
                onStdout?.Invoke(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (string.IsNullOrWhiteSpace(e.Data))
                return;
            onStderr?.Invoke(e.Data);
            lock (errorTail)
            {
                errorTail.Enqueue(e.Data.Trim());
                if (errorTail.Count > 5)
                    errorTail.Dequeue();
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        using (var cancellation = new CancellationTokenSource(timeout ?? Timeout.InfiniteTimeSpan))
        {
            try
            {
                await process.WaitForExitAsync(cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                process.Kill(entireProcessTree: true);
                throw new ConversionException($"{Path.GetFileNameWithoutExtension(exe)} stopped responding (over {timeout!.Value.TotalMinutes:0} minutes).");
            }
        }

        if (process.ExitCode != 0)
        {
            string detail;
            lock (errorTail)
                detail = errorTail.LastOrDefault() ?? $"exit code {process.ExitCode}";
            throw new ConversionException($"{Path.GetFileNameWithoutExtension(exe)}: {detail}");
        }
    }

    static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    [GeneratedRegex(@"Duration: (\d{2}:\d{2}:\d{2}\.\d{2})")]
    private static partial Regex DurationPattern();
}
