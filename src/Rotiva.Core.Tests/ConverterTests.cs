using System.IO.Compression;
using Rotiva.Conversion;
using Rotiva.Models;

namespace Rotiva.Core.Tests;

/// <summary>Bagian Converter yang tidak butuh engine eksternal, jadi bisa dites di semua OS.</summary>
[Collection("Language")]
public sealed class ConverterTests : IDisposable
{
    readonly string _dir = Directory.CreateTempSubdirectory("rotiva-test-").FullName;

    public void Dispose() => Directory.Delete(_dir, recursive: true);

    sealed class NoProgress : IProgress<double?>
    {
        public void Report(double? value) { }
    }

    [Fact]
    public async Task ZipsFilesAndFoldersWithTheirStructure()
    {
        string file = Path.Combine(_dir, "notes.txt");
        await File.WriteAllTextAsync(file, "hello");
        string folder = Path.Combine(_dir, "photos");
        Directory.CreateDirectory(Path.Combine(folder, "empty"));
        await File.WriteAllTextAsync(Path.Combine(folder, "a.txt"), "a");

        string output = await Converter.ZipAsync([file, folder], new NoProgress());

        Assert.Equal(Path.Combine(_dir, "Archive.zip"), output);
        using var zip = ZipFile.OpenRead(output);
        var names = zip.Entries.Select(e => e.FullName).Order().ToList();
        Assert.Equal(["notes.txt", "photos/a.txt", "photos/empty/"], names);
    }

    [Fact]
    public async Task SecondZipGetsANumberedName()
    {
        string file = Path.Combine(_dir, "report.txt");
        await File.WriteAllTextAsync(file, "x");

        string first = await Converter.ZipAsync([file], new NoProgress());
        string second = await Converter.ZipAsync([file], new NoProgress());

        Assert.Equal("report.zip", Path.GetFileName(first));
        Assert.NotEqual(first, second);
        Assert.True(File.Exists(second));
    }

    [Fact]
    public async Task TextWithoutRecognizerFailsWithAClearMessage()
    {
        var previous = Converter.TextRecognizer;
        Converter.TextRecognizer = null;
        try
        {
            string image = Path.Combine(_dir, "scan.png");
            await File.WriteAllBytesAsync(image, [0]);
            var text = FormatCatalog.ForFile(image).Targets.First(t => t.Action == TargetAction.ExtractText);

            await Assert.ThrowsAsync<ConversionException>(() => Converter.ConvertAsync(image, text, new NoProgress()));
        }
        finally
        {
            Converter.TextRecognizer = previous;
        }
    }

    [Fact]
    public void ExecutableNamesFollowTheOperatingSystem()
    {
        Assert.Equal(OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg", ToolLocator.Exe("ffmpeg"));
    }
}
