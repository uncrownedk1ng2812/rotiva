using System.Xml.Linq;
using Rotiva.Models;

namespace Rotiva.Core.Tests;

/// <summary>Menu "Convert with Rotiva" versi Microsoft Store harus muncul untuk semua format yang ada di roda.</summary>
public class MsixManifestTests
{
    static readonly XNamespace Uap = "http://schemas.microsoft.com/appx/manifest/uap/windows10";
    static readonly XNamespace Uap3 = "http://schemas.microsoft.com/appx/manifest/uap/windows10/3";

    static XDocument Manifest()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Rotiva.sln")))
            dir = dir.Parent;
        string root = dir?.FullName ?? throw new DirectoryNotFoundException("Rotiva.sln");
        return XDocument.Load(Path.Combine(root, "installer", "msix", "AppxManifest.xml"));
    }

    [Fact]
    public void EveryCategoryHasItsExtensions()
    {
        var associations = Manifest().Descendants(Uap3 + "FileTypeAssociation")
            .ToDictionary(a => (string)a.Attribute("Name")!, a => a.Descendants(Uap + "FileType").Select(t => t.Value).ToList());

        foreach (var category in FormatCatalog.Categories)
        {
            Assert.True(associations.TryGetValue(category.Name.ToLowerInvariant(), out var types), category.Name);
            // Semua ekstensi yang dikenali roda, termasuk alias (.jpeg → .jpg), dan tidak lebih.
            var expected = category.Extensions.ToHashSet();
            Assert.All(types, t => Assert.Contains(FormatCatalog.Normalize(t), expected));
            Assert.All(expected, e => Assert.Contains(e, types));
            Assert.Equal(types.Count, types.Distinct().Count());
        }
        Assert.Equal(FormatCatalog.Categories.Count, associations.Count);
    }

    [Fact]
    public void EveryAliasIsListed()
    {
        var types = Manifest().Descendants(Uap + "FileType").Select(t => t.Value).ToHashSet();
        Assert.All(new[] { ".jpeg", ".tif", ".htm" }, alias => Assert.Contains(alias, types));
    }
}
