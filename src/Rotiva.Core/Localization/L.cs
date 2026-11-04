using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace Rotiva.Localization;

/// <summary>
/// Terjemahan teks yang dilihat pengguna. Kuncinya kalimat bahasa Inggris itu sendiri (gaya gettext):
/// <c>L.T("Small size")</c>. Terjemahan ada di <c>Localization/&lt;kode&gt;.json</c> (embedded resource);
/// kalimat yang belum diterjemahkan tampil dalam bahasa Inggris.
/// </summary>
public static class L
{
    /// <summary>Bahasa yang tersedia, dengan namanya dalam bahasa itu sendiri.</summary>
    public static IReadOnlyList<(string Code, string Name)> Languages { get; } =
    [
        ("en", "English"),
        ("id", "Bahasa Indonesia"),
    ];

    static Dictionary<string, string> _strings = [];

    /// <summary>Kode bahasa yang sedang dipakai ("en", "id", ...).</summary>
    public static string Language { get; private set; } = "en";

    /// <summary>Dipanggil sesudah bahasa diganti, supaya UI yang sudah tampil bisa diperbarui.</summary>
    public static event Action? Changed;

    /// <summary>
    /// Pakai bahasa <paramref name="code"/>; null atau kosong = ikuti bahasa sistem
    /// (bahasa yang belum tersedia jatuh ke bahasa Inggris).
    /// </summary>
    public static void Use(string? code)
    {
        string wanted = string.IsNullOrEmpty(code) ? CultureInfo.CurrentUICulture.TwoLetterISOLanguageName : code;
        string language = Languages.Any(l => l.Code == wanted) ? wanted : "en";
        _strings = language == "en" ? [] : Load(language);
        Language = language;
        Changed?.Invoke();
    }

    /// <summary>Terjemahan <paramref name="english"/>.</summary>
    public static string T(string english) =>
        english.Length > 0 && _strings.TryGetValue(english, out var translated) ? translated : english;

    /// <summary>Terjemahan <paramref name="english"/> (dengan {0}, {1}, ...) diisi <paramref name="args"/>.</summary>
    public static string F(string english, params object?[] args) =>
        string.Format(CultureInfo.CurrentCulture, T(english), args);

    /// <summary>Semua terjemahan satu bahasa (untuk tes: tiap kalimat harus punya {0} yang sama).</summary>
    public static Dictionary<string, string> Load(string language)
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream($"Rotiva.Localization.{language}.json");
        if (stream is null)
            return [];
        // Dibaca manual (tanpa serializer berbasis refleksi) supaya aman kalau aplikasi di-trim.
        using var json = JsonDocument.Parse(stream, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
        var strings = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in json.RootElement.EnumerateObject())
        {
            if (entry.Value.GetString() is { Length: > 0 } value)
                strings[entry.Name] = value;
        }
        return strings;
    }
}
