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
        Assert.All(category.Targets, target => Assert.True(target.Action is TargetAction.Zip or TargetAction.ZipPassword));
    }

    [Fact]
    public void FolderIsZipOnly()
    {
        string folder = Directory.CreateTempSubdirectory("rotiva-test-").FullName;
        try
        {
            var category = FormatCatalog.ForFile(folder);

            Assert.Equal("Folder", category.Name);
            Assert.Equal([FormatCatalog.ZipId, FormatCatalog.LockId], Ids(category));
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
            var ids = FormatCatalog.Leaves(category).Select(FormatCatalog.IdOf).ToList();
            Assert.Equal(ids.Count, ids.Distinct().Count());
        }
    }

    [Fact]
    public void NoExtensionBelongsToTwoCategories()
    {
        var extensions = FormatCatalog.Categories.SelectMany(c => c.Extensions).ToList();
        Assert.Equal(extensions.Count, extensions.Distinct().Count());
    }

    [Theory]
    [InlineData("photo.CR3", "Image")]
    [InlineData("art.psd", "Image")]
    [InlineData("logo.svg", "Image")]
    [InlineData("clip.mpeg", "Video")]
    [InlineData("song.aif", "Audio")]
    [InlineData("novel.epub", "E-book")]
    [InlineData("movie.vtt", "Subtitle")]
    [InlineData("config.yml", "Data")]
    public void NewFormatsLandInTheirCategory(string file, string category)
    {
        Assert.Equal(category, FormatCatalog.ForFile(file).Name);
    }

    [Theory]
    [InlineData("config.yml", "yaml")]
    [InlineData("song.aif", "aiff")]
    public void NewAliasesHideTheirOwnFormat(string file, string hiddenId)
    {
        Assert.DoesNotContain(hiddenId, Ids(FormatCatalog.ForFile(file)));
    }

    [Fact]
    public void EditToolsLiveInAGroupOnlyOnImages()
    {
        var image = FormatCatalog.ForFile("a.png");
        var group = Assert.Single(image.Targets, t => t.Action == TargetAction.Group);
        Assert.Equal(5, group.Children!.Count);
        Assert.Contains("rotate-right", FormatCatalog.Leaves(image).Select(FormatCatalog.IdOf));
        Assert.DoesNotContain("edit", FormatCatalog.Leaves(image).Select(FormatCatalog.IdOf));

        Assert.DoesNotContain(FormatCatalog.ForFile("a.pdf").Targets, t => t.Action == TargetAction.Group);
    }

    [Theory]
    [InlineData("a.mp4", "rotate-right")]
    [InlineData("a.mp4", "reverse")]
    [InlineData("a.mp3", "normalize")]
    [InlineData("a.mp3", "cover")]
    public void MediaHasItsEditTools(string file, string id)
    {
        Assert.Contains(id, FormatCatalog.Leaves(FormatCatalog.ForFile(file)).Select(FormatCatalog.IdOf));
    }

    [Theory]
    [InlineData("clip.mp4", "clip.srt")]
    [InlineData("clip.srt", "clip.mkv")]
    public void VideoPlusSubtitleOffersBurnIn(string first, string second)
    {
        var category = FormatCatalog.ForSelection([first, second]);

        Assert.Equal("Video", category.Name);
        Assert.Equal(["burn", FormatCatalog.ZipId], Ids(category));
    }

    [Theory]
    [InlineData("a.mp4", "b.mp4")]
    [InlineData("a.srt", "b.srt")]
    [InlineData("a.png", "b.srt")]
    public void OtherPairsKeepTheNormalWheel(string first, string second)
    {
        Assert.DoesNotContain("burn", Ids(FormatCatalog.ForSelection([first, second])));
    }

    [Fact]
    public void LockIsOfferedEvenForOneFile()
    {
        Assert.Contains(FormatCatalog.LockId, Ids(FormatCatalog.ForFile("a.pdf")));
    }

    [Fact]
    public void ContactSheetTakesOnlyImagesInNameOrder()
    {
        Assert.DoesNotContain(FormatCatalog.SheetId, Ids(FormatCatalog.ForFile("a.jpg")));

        var category = FormatCatalog.ForFile("b 10.jpg", multipleFiles: true);
        var sheet = category.Targets.First(t => t.Action == TargetAction.ContactSheet);

        var files = FormatCatalog.FilesFor(sheet, category, ["b 10.jpg", "x.mp4", "b 9.png"], NaturalOrder.Instance);

        Assert.Equal(["b 9.png", "b 10.jpg"], files);
    }

    [Fact]
    public void VideoHasThumbnails()
    {
        Assert.Contains("thumbnails", FormatCatalog.Leaves(FormatCatalog.ForFile("a.mp4")).Select(FormatCatalog.IdOf));
    }

    [Fact]
    public void ThreeFilesNeverBurn()
    {
        Assert.DoesNotContain("burn", Ids(FormatCatalog.ForSelection(["a.mp4", "b.srt", "c.srt"])));
    }

    [Fact]
    public void EveryGroupPageFillsTheWheel()
    {
        // Roda punya minimal 4 slot; dengan BACK, grup harus punya 3 alat supaya tidak ada slot kosong.
        foreach (var group in FormatCatalog.Categories.SelectMany(c => c.Targets).Where(t => t.Action == TargetAction.Group))
            Assert.True(group.Children!.Count >= 3, group.Description);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(5)]
    public void GroupPagePutsBackWhereThePointerIs(int backIndex)
    {
        var group = FormatCatalog.ForFile("a.png").Targets.First(t => t.Action == TargetAction.Group);

        var page = FormatCatalog.PageOf(group, backIndex);

        Assert.Equal(6, page.Count);
        Assert.Same(FormatCatalog.Back, page[backIndex]);
    }

    [Fact]
    public void EditTakesEveryImageEvenFromTheSecondPage()
    {
        var category = FormatCatalog.ForFile("a.png", multipleFiles: true);
        var rotate = FormatCatalog.Leaves(category).First(t => FormatCatalog.IdOf(t) == "rotate-left");

        var files = FormatCatalog.FilesFor(rotate, category, ["a.png", "b.jpg", "c.mp4"], NaturalOrder.Instance);

        Assert.Equal(["a.png", "b.jpg"], files);
    }
}
