using Rotiva.Updates;

namespace Rotiva.Core.Tests;

public sealed class UpdateTests
{
    const string Notes = """
        **PowerPoint slides on the wheel.**

        ### Downloads

        | File | For |
        |---|---|
        | `Rotiva-Setup-1.11.0.exe` | Windows |

        ### What's new

        - **Slides.** Drag a `.pptx` and pick *PDF*.
        - **Updates:** see the [notes](https://github.com/uncrownedk1ng2812/rotiva/releases).

        ### Install

        Run the installer.
        """;

    static string Json(string tag = "v1.11.0", bool prerelease = false) => $$"""
        {
          "tag_name": "{{tag}}",
          "draft": false,
          "prerelease": {{(prerelease ? "true" : "false")}},
          "html_url": "https://github.com/uncrownedk1ng2812/rotiva/releases/tag/{{tag}}",
          "published_at": "2026-11-05T10:00:00Z",
          "body": {{System.Text.Json.JsonSerializer.Serialize(Notes)}},
          "assets": [
            { "name": "Rotiva-1.11.0-x86_64.AppImage", "size": 1, "browser_download_url": "https://github.com/a/b/c.AppImage" },
            { "name": "Rotiva-Setup-1.11.0.exe", "size": 259280282, "browser_download_url": "https://github.com/a/b/Rotiva-Setup-1.11.0.exe" },
            { "name": "SHA256SUMS.txt", "size": 376, "browser_download_url": "https://github.com/a/b/SHA256SUMS.txt" },
            { "name": "evil.exe", "size": 1, "browser_download_url": "https://example.com/evil.exe" }
          ]
        }
        """;

    [Fact]
    public void ParsesLatestRelease()
    {
        var release = ReleaseInfo.Parse(Json())!;
        Assert.Equal(new Version(1, 11, 0), release.Version);
        Assert.Equal("Rotiva-Setup-1.11.0.exe", release.WindowsInstaller!.Name);
        Assert.Equal(259280282, release.WindowsInstaller.Size);
        Assert.NotNull(release.Checksums);
        Assert.DoesNotContain(release.Assets, a => a.Name == "evil.exe");
        Assert.Equal(new DateTimeOffset(2026, 11, 5, 10, 0, 0, TimeSpan.Zero), release.Published);
    }

    [Fact]
    public void IgnoresPreReleases()
    {
        Assert.Null(ReleaseInfo.Parse(Json(prerelease: true)));
        Assert.Null(ReleaseInfo.Parse(Json(tag: "v1.12.0-preview.1")));
    }

    [Theory]
    [InlineData("1.10.1.0", false)]
    [InlineData("1.10.1", false)]
    [InlineData("1.10.0.0", true)]
    [InlineData("1.9.9", true)]
    [InlineData("1.11.0", false)]
    [InlineData("2.0.0", false)]
    public void ComparesVersions(string current, bool newer)
    {
        Assert.True(ReleaseInfo.TryParseVersion("v1.10.1", out var tag));
        var release = new ReleaseInfo(tag, "v1.10.1", "", null, "", []);
        Assert.Equal(newer, release.IsNewerThan(Version.Parse(current)));
    }

    [Fact]
    public void WhatsNewDropsDownloadsAndInstall()
    {
        string text = ReleaseNotes.WhatsNew(Notes);
        Assert.Contains("PowerPoint slides on the wheel", text);
        Assert.DoesNotContain("What's new", text);
        Assert.Contains("Slides.", text);
        Assert.DoesNotContain("Downloads", text);
        Assert.DoesNotContain("Run the installer", text);
    }

    [Fact]
    public void SplitsBlocksAndSpans()
    {
        var blocks = ReleaseNotes.Blocks(ReleaseNotes.WhatsNew(Notes));
        Assert.Equal(
            [NoteBlockKind.Paragraph, NoteBlockKind.Bullet, NoteBlockKind.Bullet],
            blocks.Select(b => b.Kind));

        var spans = ReleaseNotes.Spans(blocks[1].Text);
        Assert.Equal("Slides.", spans[0].Text);
        Assert.Equal(NoteStyle.Bold, spans[0].Style);
        Assert.Contains(spans, s => s.Text == ".pptx" && s.Style == NoteStyle.Code);
        Assert.Contains(spans, s => s.Text == "PDF" && s.Style == NoteStyle.Italic);

        var link = ReleaseNotes.Spans(blocks[2].Text).Single(s => s.Style == NoteStyle.Link);
        Assert.Equal("notes", link.Text);
        Assert.StartsWith("https://github.com/", link.Url);
        Assert.Equal(NoteBlockKind.Heading, ReleaseNotes.Blocks("### Fixes")[0].Kind);
        Assert.Equal("Drag a file_name here", string.Concat(ReleaseNotes.Spans("Drag a file_name here").Select(s => s.Text)));
    }

    [Fact]
    public void FindsChecksum()
    {
        string hash = new('a', 64);
        string sums = $"{new string('b', 64)}  Rotiva-1.11.0-x86_64.AppImage\n{hash} *Rotiva-Setup-1.11.0.exe\n";
        Assert.Equal(hash, ReleaseInfo.ChecksumFor(sums, "Rotiva-Setup-1.11.0.exe"));
        Assert.Null(ReleaseInfo.ChecksumFor(sums, "Rotiva-Setup-1.10.0.exe"));
    }
}
