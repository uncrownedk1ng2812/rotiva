using System.Text.RegularExpressions;
using Rotiva.Localization;
using Rotiva.Models;

namespace Rotiva.Core.Tests;

/// <summary>
/// Tiap bahasa harus menerjemahkan semua teks yang dipakai kode, dengan {0}, {1} yang sama.
/// Teks dicari langsung di source (L.T("...") / L.F("...")) dan di <see cref="FormatCatalog"/>.
/// </summary>
[Collection("Language")]
public sealed partial class LocalizationTests
{
    // Nama produk dan kata yang memang sama di semua bahasa: boleh tidak ada di file terjemahan.
    static readonly HashSet<string> SameEverywhere =
    [
        "Video", "Audio", "Spreadsheet", "PDF", "File", "Folder", "ZIP", "GitHub",
        "QuickTime", "Matroska", "320 kbps", "Vorbis", "Markdown", "E-book", "Data",
        "Microsoft Word", "Microsoft Excel", "Microsoft PowerPoint", "Excel 97-2003",
        "LibreOffice Writer", "LibreOffice Calc", "LibreOffice Impress",
    ];

    static string SourceRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Rotiva.sln")))
            dir = dir.Parent;
        return Path.Combine(dir?.FullName ?? throw new DirectoryNotFoundException("Rotiva.sln"), "src");
    }

    [GeneratedRegex("""L\.[TF]\(\s*"((?:[^"\\]|\\.)*)"|L\.T\([^"]*?\?\s*"([^"]*)"\s*:\s*"([^"]*)"\)""")]
    private static partial Regex Translated();

    [GeneratedRegex(@"\{\d+(:[^}]*)?\}")]
    private static partial Regex Placeholder();

    static IEnumerable<string> TextsInCode()
    {
        foreach (string file in Directory.EnumerateFiles(SourceRoot(), "*.cs", SearchOption.AllDirectories))
        {
            string path = file.Replace('\\', '/');
            if (path.Contains("/obj/") || path.Contains("/bin/") || path.Contains(".Tests/"))
                continue;
            foreach (Match match in Translated().Matches(File.ReadAllText(file)))
            {
                foreach (Group group in match.Groups.Values.Skip(1).Where(g => g.Success))
                    yield return Regex.Unescape(group.Value);
            }
        }

        foreach (var category in FormatCatalog.Categories)
        {
            yield return category.Name;
            foreach (var target in category.Targets.Concat(category.Targets.SelectMany(t => t.Children ?? [])).Append(FormatCatalog.Back).Append(FormatCatalog.BurnSubtitles))
            {
                yield return target.Description;
                // Nama format (PNG, MP4, ...) tidak diterjemahkan; label aksi (COMPRESS, MERGE, ...) diterjemahkan.
                if (target.Action != TargetAction.Convert)
                    yield return target.Label;
            }
        }
        // Kategori untuk folder dan file yang belum didukung (tidak ada di Categories).
        yield return "Folder";
        yield return "File";
    }

    public static TheoryData<string> Translations() => new(L.Languages.Select(l => l.Code).Where(code => code != "en"));

    [Theory]
    [MemberData(nameof(Translations))]
    public void EveryTextIsTranslated(string language)
    {
        var strings = L.Load(language);
        var missing = TextsInCode().Distinct()
            .Where(text => text.Length > 0 && !SameEverywhere.Contains(text) && !strings.ContainsKey(text))
            .ToList();
        Assert.Empty(missing);
    }

    [Theory]
    [MemberData(nameof(Translations))]
    public void TranslationsKeepTheirPlaceholders(string language)
    {
        foreach (var (english, translated) in L.Load(language))
        {
            var expected = Placeholder().Matches(english).Select(m => m.Value).Order();
            var actual = Placeholder().Matches(translated).Select(m => m.Value).Order();
            Assert.True(expected.SequenceEqual(actual), $"{language}: \"{translated}\" should use the placeholders of \"{english}\"");
        }
    }

    [Theory]
    [MemberData(nameof(Translations))]
    public void TranslationsAreStillUsed(string language)
    {
        var used = TextsInCode().ToHashSet();
        var unused = L.Load(language).Keys.Where(key => !used.Contains(key)).ToList();
        Assert.Empty(unused);
    }

    [Fact]
    public void UnknownLanguageFallsBackToEnglish()
    {
        try
        {
            L.Use("id");
            Assert.Equal("id", L.Language);
            Assert.Equal("Ukuran kecil", L.T("Small size"));
            Assert.Equal("Ke PNG", L.F("To {0}", "PNG"));

            L.Use("xx");
            Assert.Equal("en", L.Language);
            Assert.Equal("Small size", L.T("Small size"));
        }
        finally
        {
            L.Use("en");
        }
    }
}
