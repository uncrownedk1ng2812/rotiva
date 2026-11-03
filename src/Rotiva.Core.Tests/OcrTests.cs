using System.Diagnostics;
using Rotiva.Conversion;
using Rotiva.Models;

namespace Rotiva.Core.Tests;

/// <summary>
/// Jalur TEXT dengan Tesseract dari awal sampai akhir: gambar (dibuat ImageMagick) → teks .txt.
/// Hanya berjalan kalau Tesseract dan ImageMagick terpasang (CI Linux memasang keduanya);
/// selain itu test ini lolos tanpa memeriksa apa-apa.
/// </summary>
public sealed class OcrTests : IDisposable
{
    readonly string _dir = Directory.CreateTempSubdirectory("rotiva-ocr-").FullName;

    public void Dispose() => Directory.Delete(_dir, recursive: true);

    sealed class NoProgress : IProgress<double?>
    {
        public void Report(double? value) { }
    }

    [Fact]
    public async Task ReadsTextFromAnImageWithTesseract()
    {
        if (TesseractTextRecognizer.TryCreate() is not { } tesseract || ToolLocator.ImageMagick is not { } magick)
            return;

        string image = Path.Combine(_dir, "scan.png");
        using (var draw = Process.Start(new ProcessStartInfo(magick,
            ["-size", "900x200", "xc:white", "-fill", "black", "-pointsize", "64", "-annotate", "+30+120", "Hello Rotiva 2026", image])))
        {
            await draw!.WaitForExitAsync();
            Assert.Equal(0, draw.ExitCode);
        }

        var previous = Converter.TextRecognizer;
        Converter.TextRecognizer = tesseract;
        try
        {
            var text = FormatCatalog.ForFile(image).Targets.First(t => t.Action == TargetAction.ExtractText);
            string output = await Converter.ConvertAsync(image, text, new NoProgress());

            string recognized = await File.ReadAllTextAsync(output);
            Assert.Contains("Hello", recognized);
            Assert.Contains("Rotiva", recognized);
            Assert.Contains("2026", recognized);
        }
        finally
        {
            Converter.TextRecognizer = previous;
        }
    }
}
