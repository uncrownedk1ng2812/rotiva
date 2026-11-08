using System.Diagnostics;
using Rotiva.Conversion;
using Rotiva.Models;

namespace Rotiva.Core.Tests;

/// <summary>
/// Presentasi dari awal sampai akhir: PPTX dua slide (dibuat LibreOffice dari ODP flat) → PDF dan PNG per slide.
/// Hanya berjalan kalau LibreOffice dan Ghostscript terpasang; selain itu lolos tanpa memeriksa apa-apa.
/// </summary>
public sealed class SlidesTests : IDisposable
{
    readonly string _dir = Directory.CreateTempSubdirectory("rotiva-slides-").FullName;

    public void Dispose() => Directory.Delete(_dir, recursive: true);

    sealed class NoProgress : IProgress<double?>
    {
        public void Report(double? value) { }
    }

    const string TwoSlides = """
        <?xml version="1.0" encoding="UTF-8"?>
        <office:document xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0"
            xmlns:draw="urn:oasis:names:tc:opendocument:xmlns:drawing:1.0"
            xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0"
            xmlns:svg="urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0"
            office:version="1.3" office:mimetype="application/vnd.oasis.opendocument.presentation">
          <office:body>
            <office:presentation>
              <draw:page draw:name="One">
                <draw:frame svg:x="2cm" svg:y="2cm" svg:width="20cm" svg:height="3cm">
                  <draw:text-box><text:p>First slide</text:p></draw:text-box>
                </draw:frame>
              </draw:page>
              <draw:page draw:name="Two">
                <draw:frame svg:x="2cm" svg:y="2cm" svg:width="20cm" svg:height="3cm">
                  <draw:text-box><text:p>Second slide</text:p></draw:text-box>
                </draw:frame>
              </draw:page>
            </office:presentation>
          </office:body>
        </office:document>
        """;

    async Task<string> MakePptxAsync(string soffice)
    {
        string flat = Path.Combine(_dir, "deck.fodp");
        await File.WriteAllTextAsync(flat, TwoSlides);
        string profile = new Uri(Path.Combine(_dir, "profile")).AbsoluteUri;
        using var convert = Process.Start(new ProcessStartInfo(soffice,
            [$"-env:UserInstallation={profile}", "--headless", "--convert-to", "pptx", "--outdir", _dir, flat]))!;
        await convert.WaitForExitAsync();
        string pptx = Path.Combine(_dir, "deck.pptx");
        Assert.True(File.Exists(pptx), "LibreOffice did not make the test presentation");
        return pptx;
    }

    [Fact]
    public void PresentationGetsTheSlidesWheel()
    {
        var category = FormatCatalog.ForFile("talk.pptx");

        Assert.Equal("Slides", category.Name);
        Assert.Equal(ConversionEngine.Office, category.Engine);
        var ids = category.Targets.Select(FormatCatalog.IdOf).ToList();
        Assert.Equal(["pdf", "odp", "png", "jpg", FormatCatalog.LockId], ids);
        Assert.True(FormatCatalog.CanMergeToPdf("talk.ppt"));
    }

    [Fact]
    public async Task ConvertsSlidesToPdfAndToOneImagePerSlide()
    {
        if (ToolLocator.LibreOffice is not { } soffice || ToolLocator.Ghostscript is null)
            return;
        string pptx = await MakePptxAsync(soffice);
        var targets = FormatCatalog.ForFile(pptx).Targets;

        string pdf = await Converter.ConvertAsync(pptx, targets.First(t => t.Extension == ".pdf"), new NoProgress());
        Assert.Equal("deck.pdf", Path.GetFileName(pdf));
        Assert.StartsWith("%PDF", await File.ReadAllTextAsync(pdf));

        string images = await Converter.ConvertAsync(pptx, targets.First(t => t.Extension == ".png"), new NoProgress());
        Assert.True(Directory.Exists(images));
        Assert.Equal(["deck-01.png", "deck-02.png"],
            Directory.GetFiles(images).Select(Path.GetFileName).Order(StringComparer.Ordinal).ToList());
    }
}
