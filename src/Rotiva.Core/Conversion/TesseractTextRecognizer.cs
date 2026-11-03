using System.Text;

namespace Rotiva.Conversion;

/// <summary>
/// OCR lewat Tesseract (program "tesseract"): untuk macOS dan Linux, yang tidak punya mesin OCR
/// bawaan yang bisa dipakai Rotiva. Bahasa: Inggris, ditambah Indonesia kalau paketnya terpasang
/// (brew install tesseract-lang, apt install tesseract-ocr-ind).
/// </summary>
public sealed class TesseractTextRecognizer : ITextRecognizer
{
    static readonly string[] PreferredLanguages = ["eng", "ind"];
    static readonly TimeSpan Timeout = TimeSpan.FromMinutes(3);

    readonly string _tesseract;
    string? _languages;

    TesseractTextRecognizer(string tesseract) => _tesseract = tesseract;

    /// <summary>Null kalau Tesseract tidak terpasang (segmen TEXT lalu disembunyikan).</summary>
    public static TesseractTextRecognizer? TryCreate() =>
        ToolLocator.Tesseract is { } tesseract ? new TesseractTextRecognizer(tesseract) : null;

    public uint MaxImageDimension => 10000;

    public async Task<string> RecognizeAsync(string pngPath)
    {
        _languages ??= await DetectLanguagesAsync();
        var text = new StringBuilder();
        await Converter.RunAsync(_tesseract, [pngPath, "stdout", "-l", _languages],
            onStdout: line => text.AppendLine(line), timeout: Timeout);
        return text.ToString();
    }

    // "eng+ind" dari bahasa yang terpasang; kalau keduanya tidak ada, bahasa terpasang pertama.
    async Task<string> DetectLanguagesAsync()
    {
        var installed = new List<string>();
        await Converter.RunAsync(_tesseract, ["--list-langs"], onStdout: line =>
        {
            string name = line.Trim();
            if (name.Length > 0 && !name.Contains(' ') && name != "osd")
                installed.Add(name);
        }, timeout: Timeout);

        var chosen = PreferredLanguages.Where(installed.Contains).ToList();
        if (chosen.Count == 0 && installed.Count > 0)
            chosen.Add(installed[0]);
        return chosen.Count > 0 ? string.Join('+', chosen) : "eng";
    }
}
