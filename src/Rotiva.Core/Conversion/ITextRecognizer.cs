namespace Rotiva.Conversion;

/// <summary>
/// Mesin OCR milik sistem operasi (Windows: Windows.Media.Ocr). Diisi aplikasi lewat
/// <see cref="Converter.TextRecognizer"/>; tanpa itu segmen TEXT gagal dengan pesan yang jelas.
/// </summary>
public interface ITextRecognizer
{
    /// <summary>Sisi terpanjang gambar yang diterima mesin OCR.</summary>
    uint MaxImageDimension { get; }

    /// <summary>Baca teks dari gambar PNG (sudah diperkecil di bawah <see cref="MaxImageDimension"/>).</summary>
    Task<string> RecognizeAsync(string pngPath);
}
