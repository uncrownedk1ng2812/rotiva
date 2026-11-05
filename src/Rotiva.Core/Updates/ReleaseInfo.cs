using System.Globalization;
using System.Text.Json;

namespace Rotiva.Updates;

/// <summary>Satu file di rilis GitHub.</summary>
public sealed record ReleaseAsset(string Name, long Size, string DownloadUrl);

/// <summary>
/// Rilis terbaru dari GitHub (<c>GET /repos/{owner}/{repo}/releases/latest</c>): versi, catatan rilis
/// (Markdown), dan file-file yang bisa diunduh. Tanpa UI dan tanpa jaringan supaya bisa dites.
/// </summary>
public sealed record ReleaseInfo(
    Version Version,
    string Tag,
    string Notes,
    DateTimeOffset? Published,
    string PageUrl,
    IReadOnlyList<ReleaseAsset> Assets)
{
    public const string Owner = "uncrownedk1ng2812";
    public const string Repo = "rotiva";
    public const string LatestApiUrl = $"https://api.github.com/repos/{Owner}/{Repo}/releases/latest";
    public const string ChecksumsName = "SHA256SUMS.txt";

    /// <summary>Installer Windows (<c>Rotiva-Setup-X.Y.Z.exe</c>), null kalau rilis ini tidak punya.</summary>
    public ReleaseAsset? WindowsInstaller => Assets.FirstOrDefault(a =>
        a.Name.StartsWith("Rotiva-Setup-", StringComparison.OrdinalIgnoreCase) &&
        a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));

    public ReleaseAsset? Checksums => Assets.FirstOrDefault(a =>
        a.Name.Equals(ChecksumsName, StringComparison.OrdinalIgnoreCase));

    /// <summary>Bagian catatan rilis yang menarik untuk pengguna (tanpa tabel unduhan dan cara instal).</summary>
    public string WhatsNew => ReleaseNotes.WhatsNew(Notes);

    /// <summary>True kalau rilis ini lebih baru dari <paramref name="current"/>.</summary>
    public bool IsNewerThan(Version current) => Normalize(Version) > Normalize(current);

    /// <summary>Baca JSON dari API GitHub. Draft, pre-release, dan tag yang bukan versi menghasilkan null.</summary>
    public static ReleaseInfo? Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            return null;
        if (Bool(root, "draft") || Bool(root, "prerelease"))
            return null;

        string tag = Str(root, "tag_name");
        if (!TryParseVersion(tag, out var version))
            return null;

        DateTimeOffset? published = DateTimeOffset.TryParse(Str(root, "published_at"), CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal, out var date) ? date : null;

        var assets = new List<ReleaseAsset>();
        if (root.TryGetProperty("assets", out var list) && list.ValueKind == JsonValueKind.Array)
        {
            foreach (var asset in list.EnumerateArray())
            {
                string name = Str(asset, "name"), url = Str(asset, "browser_download_url");
                long size = asset.TryGetProperty("size", out var s) && s.TryGetInt64(out var n) ? n : 0;
                // Hanya unduhan dari GitHub sendiri; URL lain di JSON diabaikan.
                if (name.Length > 0 && url.StartsWith("https://github.com/", StringComparison.OrdinalIgnoreCase))
                    assets.Add(new ReleaseAsset(name, size, url));
            }
        }

        string page = Str(root, "html_url");
        if (!page.StartsWith("https://github.com/", StringComparison.OrdinalIgnoreCase))
            page = $"https://github.com/{Owner}/{Repo}/releases/tag/{tag}";

        return new ReleaseInfo(version, tag, Str(root, "body").Replace("\r\n", "\n"), published, page, assets);
    }

    /// <summary>"v1.10.2" atau "1.10.2" jadi 1.10.2; akhiran seperti "-preview.1" membuat tag ditolak.</summary>
    public static bool TryParseVersion(string tag, out Version version)
    {
        string text = tag.Trim().TrimStart('v', 'V');
        if (text.Contains('-') || !Version.TryParse(text, out var parsed))
        {
            version = new Version(0, 0, 0);
            return false;
        }
        version = Normalize(parsed);
        return true;
    }

    /// <summary>Versi tiga angka (1.10.1.0 dan 1.10.1 dianggap sama).</summary>
    public static Version Normalize(Version v) => new(v.Major, Math.Max(v.Minor, 0), Math.Max(v.Build, 0));

    /// <summary>Hash SHA-256 (huruf kecil) untuk <paramref name="fileName"/> dari isi SHA256SUMS.txt.</summary>
    public static string? ChecksumFor(string sums, string fileName)
    {
        foreach (var raw in sums.Split('\n'))
        {
            // Format sha256sum: "<hash>  <nama>" atau "<hash> *<nama>".
            var parts = raw.Trim().Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 && parts[0].Length == 64 && parts[0].All(Uri.IsHexDigit) &&
                parts[1].TrimStart('*').Equals(fileName, StringComparison.OrdinalIgnoreCase))
                return parts[0].ToLowerInvariant();
        }
        return null;
    }

    static string Str(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";

    static bool Bool(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.True;
}
