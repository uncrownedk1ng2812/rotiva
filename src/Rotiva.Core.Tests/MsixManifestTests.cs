using System.Xml.Linq;

namespace Rotiva.Core.Tests;

/// <summary>Menu "Convert with Rotiva" versi Microsoft Store (installer/msix/AppxManifest.xml).</summary>
public class MsixManifestTests
{
    static readonly XNamespace Desktop5 = "http://schemas.microsoft.com/appx/manifest/desktop/windows10/5";
    static readonly XNamespace Com = "http://schemas.microsoft.com/appx/manifest/com/windows10";

    static XDocument Manifest()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Rotiva.sln")))
            dir = dir.Parent;
        string root = dir?.FullName ?? throw new DirectoryNotFoundException("Rotiva.sln");
        return XDocument.Load(Path.Combine(root, "installer", "msix", "AppxManifest.xml"));
    }

    static List<string> MenuTypes() =>
        Manifest().Descendants(Desktop5 + "ItemType").Select(t => (string)t.Attribute("Type")!).ToList();

    [Fact]
    public void MenuIsOnEveryFileAndFolder()
    {
        // "*", bukan per ekstensi: menu per ekstensi tidak muncul untuk jenis file tanpa aplikasi
        // terdaftar (.md, .odt tanpa LibreOffice, ...). File apa pun bisa di-ZIP dari roda.
        Assert.Equal(["*", "Directory"], MenuTypes());
    }

    [Fact]
    public void MenuUsesTheShellExtensionClass()
    {
        var manifest = Manifest();
        string clsid = (string)manifest.Descendants(Com + "Class").Single().Attribute("Id")!;
        Assert.All(manifest.Descendants(Desktop5 + "Verb"), verb => Assert.Equal(clsid, (string)verb.Attribute("Clsid")!));
        // Sama dengan CLSID_ConvertCommand di src/Rotiva.ShellExtension/ConvertCommand.cpp.
        Assert.Equal("345C3645-D481-40C5-90AA-B30D798A46E1", clsid);
    }
}
