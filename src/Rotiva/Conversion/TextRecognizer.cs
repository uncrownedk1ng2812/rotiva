using System.IO;
using System.Text;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Rotiva.Conversion;

/// <summary>
/// OCR memakai mesin bawaan Windows (Windows.Media.Ocr): gratis, offline, tanpa program tambahan.
/// Bahasa mengikuti bahasa Windows; kalau paket OCR-nya tidak ada, dipakai bahasa OCR apa pun yang
/// terpasang (huruf Latin, jadi mesin bahasa Inggris tetap bisa membaca teks bahasa Indonesia).
/// </summary>
internal static class TextRecognizer
{
    /// <summary>Batas sisi gambar yang diterima mesin OCR Windows (biasanya 10000 px).</summary>
    public static uint MaxImageDimension => OcrEngine.MaxImageDimension;

    /// <summary>Baca teks dari gambar PNG (sudah diperkecil di bawah <see cref="MaxImageDimension"/>).</summary>
    public static async Task<string> RecognizeAsync(string pngPath)
    {
        var engine = OcrEngine.TryCreateFromUserProfileLanguages()
            ?? OcrEngine.AvailableRecognizerLanguages.Select(OcrEngine.TryCreateFromLanguage).FirstOrDefault(e => e is not null)
            ?? throw new ConversionException("No Windows OCR language pack is installed (Settings → Time & language).");

        var file = await StorageFile.GetFileFromPathAsync(Path.GetFullPath(pngPath));
        using IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
        var decoder = await BitmapDecoder.CreateAsync(stream);
        using var bitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
        var result = await engine.RecognizeAsync(bitmap);

        var text = new StringBuilder();
        foreach (var line in result.Lines)
            text.AppendLine(string.Join(' ', line.Words.Select(word => FixDigits(word.Text))));
        return text.ToString();
    }

    // Angka nol sering terbaca huruf O ("1.250.OOO"). Hanya diperbaiki di kata yang berisi angka
    // dan tidak punya huruf lain selain O, supaya kata biasa ("Oktober", "OK") tidak ikut berubah.
    static string FixDigits(string word) =>
        word.Any(char.IsDigit) && word.Where(char.IsLetter).All(c => c is 'O' or 'o')
            ? word.Replace('O', '0').Replace('o', '0')
            : word;
}
