using Rotiva.Models;

namespace Rotiva.Core.Tests;

public class FormatCatalogTests
{
    static IEnumerable<string> Ids(FileCategory category) => category.Targets.Select(FormatCatalog.IdOf);

    [Fact]
    public void ImageHidesItsOwnFormat()
    {
        var category = FormatCatalog.ForFile("photo.png");

        Assert.Equal(ConversionEngine.ImageMagick, category.Engine);
        Assert.DoesNotContain("png", Ids(category));
        Assert.Contains("jpg", Ids(category));
        Assert.Contains("compress", Ids(category));
    }

    [Theory]
    [InlineData("photo.JPEG", "jpg")]
    [InlineData("scan.TIF", "tiff")]
    public void AliasedExtensionsAreTreatedAsTheSameFormat(string file, string hiddenId)
    {
        Assert.DoesNotContain(hiddenId, Ids(FormatCatalog.ForFile(file)));
    }

    [Fact]
    public void MergeAndZipOnlyAppearForSeveralFiles()
    {
        Assert.DoesNotContain(FormatCatalog.MergeId, Ids(FormatCatalog.ForFile("a.jpg")));
        Assert.DoesNotContain(FormatCatalog.ZipId, Ids(FormatCatalog.ForFile("a.jpg")));

        var several = Ids(FormatCatalog.ForFile("a.jpg", multipleFiles: true)).ToList();
        Assert.Contains(FormatCatalog.MergeId, several);
        Assert.Contains(FormatCatalog.ZipId, several);
    }

    [Fact]
    public void UnknownFileTypeCanOnlyBeZipped()
    {
        var category = FormatCatalog.ForFile("data.xyz123");

        Assert.False(FormatCatalog.IsSupported(category));
        Assert.All(category.Targets, target => Assert.Equal(TargetAction.Zip, target.Action));
    }

    [Fact]
    public void FolderIsZipOnly()
    {
        string folder = Directory.CreateTempSubdirectory("rotiva-test-").FullName;
        try
        {
            var category = FormatCatalog.ForFile(folder);

            Assert.Equal("Folder", category.Name);
            Assert.Equal([FormatCatalog.ZipId], Ids(category));
        }
        finally
        {
            Directory.Delete(folder);
        }
    }

    [Theory]
    [InlineData("doc.pdf", true)]
    [InlineData("photo.heic", true)]
    [InlineData("song.mp3", false)]
    public void CanMergeToPdf(string file, bool expected)
    {
        Assert.Equal(expected, FormatCatalog.CanMergeToPdf(file));
    }

    [Fact]
    public void NaturalOrderSortsNumbersByValue()
    {
        string[] names = ["page 10.jpg", "Page 2.jpg", "page 1.jpg", "page 02b.jpg"];

        var sorted = names.Order(NaturalOrder.Instance).ToList();

        Assert.Equal(["page 1.jpg", "Page 2.jpg", "page 02b.jpg", "page 10.jpg"], sorted);
    }

    [Fact]
    public void MergeTakesMergeableFilesInNaturalOrder()
    {
        var category = FormatCatalog.ForFile("b 10.jpg", multipleFiles: true);
        var merge = category.Targets.First(t => t.Action == TargetAction.MergePdf);

        var files = FormatCatalog.FilesFor(merge, category, ["b 10.jpg", "song.mp3", "b 9.pdf"], NaturalOrder.Instance);

        Assert.Equal(["b 9.pdf", "b 10.jpg"], files);
    }

    [Fact]
    public void ConvertOnlyTakesFilesOfTheSameKind()
    {
        var category = FormatCatalog.ForFile("a.png", multipleFiles: true);
        var jpg = category.Targets.First(t => FormatCatalog.IdOf(t) == "jpg");

        var files = FormatCatalog.FilesFor(jpg, category, ["a.png", "b.webp", "c.mp4", "d.jpg"], NaturalOrder.Instance);

        Assert.Equal(["a.png", "b.webp"], files);
    }

    [Fact]
    public void EveryCategoryHasTargetsWithStableIds()
    {
        foreach (var category in FormatCatalog.Categories)
        {
            Assert.NotEmpty(category.Targets);
            var ids = category.Targets.Select(FormatCatalog.IdOf).ToList();
            Assert.Equal(ids.Count, ids.Distinct().Count());
        }
    }
}
