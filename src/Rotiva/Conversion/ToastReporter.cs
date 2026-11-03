using System.Windows;

namespace Rotiva.Conversion;

/// <summary>Laporan <see cref="ConversionQueue"/> ke notifikasi toast Rotiva di satu area monitor.</summary>
/// <param name="area">Area monitor (DIP) tempat notifikasi ditampilkan.</param>
internal sealed class ToastReporter(ToastWindow toast, Rect area) : IConversionReporter
{
    public void ShowProgress(string header, string message) => toast.ShowProgress(header, message, area);

    public void SetProgress(double? progress) => toast.SetProgress(progress);

    public void ShowResult(bool success, string header, string message, string? revealPath = null) =>
        toast.ShowResult(success, header, message, area, revealPath);

    // Clipboard butuh STA (UI thread) dan bisa sedang dikunci aplikasi lain.
    public bool TryCopyText(string text)
    {
        try
        {
            Clipboard.SetText(text);
            return true;
        }
        catch (System.Runtime.InteropServices.COMException)
        {
            return false;
        }
    }
}
