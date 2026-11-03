namespace Rotiva.Conversion;

/// <summary>
/// Tempat <see cref="ConversionQueue"/> melaporkan progres dan hasil (di Windows: notifikasi
/// toast Rotiva). Semua method dipanggil di thread yang memanggil <see cref="ConversionQueue.Enqueue"/>.
/// </summary>
public interface IConversionReporter
{
    void ShowProgress(string header, string message);

    /// <summary>0..1, atau null kalau progresnya tidak diketahui.</summary>
    void SetProgress(double? progress);

    /// <param name="revealPath">File hasil yang bisa dibuka/ditunjukkan dari notifikasi, atau null.</param>
    void ShowResult(bool success, string header, string message, string? revealPath = null);

    /// <summary>Salin teks ke clipboard; false kalau clipboard sedang tidak bisa dipakai.</summary>
    bool TryCopyText(string text);
}
