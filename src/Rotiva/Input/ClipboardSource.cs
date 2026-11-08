using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Rotiva.Input;

/// <summary>Mengambil isi clipboard sebagai file yang bisa dibuka di roda.</summary>
public static class ClipboardSource
{
    /// <summary>
    /// File yang disalin di Explorer, atau gambar yang disalin (disimpan sementara).
    /// Kosong kalau clipboard berisi hal lain.
    /// </summary>
    /// <param name="temporary">Path gambar sementara yang harus dihapus sesudah dipakai; null untuk file biasa.</param>
    /// <exception cref="COMException">Clipboard sedang dikunci aplikasi lain.</exception>
    public static IReadOnlyList<string> Read(out string? temporary)
    {
        temporary = null;
        if (Clipboard.ContainsFileDropList())
            return Clipboard.GetFileDropList().Cast<string>().ToList();

        if (ReadImage() is not { } image)
            return [];

        string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Rotiva");
        Directory.CreateDirectory(folder);
        string name = "Clipboard " + DateTime.Now.ToString("yyyy-MM-dd HHmmss", CultureInfo.InvariantCulture);
        // TIFF, bukan PNG: format sumber disembunyikan dari roda, padahal PNG justru pilihan utamanya.
        temporary = Path.Combine(folder, name + ".tiff");
        SaveLossless(image, temporary);
        return [temporary];
    }

    // Browser dan alat screenshot menaruh format "PNG" yang menyimpan transparansi dengan benar.
    static BitmapSource? ReadImage()
    {
        if (Clipboard.GetDataObject() is { } data && data.GetDataPresent("PNG") && data.GetData("PNG") is MemoryStream png)
        {
            png.Position = 0;
            return BitmapFrame.Create(png, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        }
        return Clipboard.ContainsImage() ? Clipboard.GetImage() : null;
    }

    public static void SaveLossless(BitmapSource image, string path)
    {
        var encoder = new TiffBitmapEncoder { Compression = TiffCompressOption.Zip };
        encoder.Frames.Add(BitmapFrame.Create(WithoutEmptyAlpha(image)));
        using var file = File.Create(path);
        encoder.Save(file);
    }

    // Gambar DIB dari clipboard sering terbaca dengan alpha 0 di semua piksel, jadi tampak kosong.
    static BitmapSource WithoutEmptyAlpha(BitmapSource image)
    {
        if (image.Format != PixelFormats.Bgra32 && image.Format != PixelFormats.Pbgra32)
            return image;
        int stride = image.PixelWidth * 4;
        byte[] pixels = new byte[stride * image.PixelHeight];
        image.CopyPixels(pixels, stride, 0);
        for (int i = 3; i < pixels.Length; i += 4)
        {
            if (pixels[i] != 0)
                return image;
        }
        return new FormatConvertedBitmap(image, PixelFormats.Bgr32, null, 0);
    }
}

/// <summary>Shortcut global Ctrl+Alt+Shift+V. Kalau sudah dipakai aplikasi lain, menu tray tetap bisa dipakai.</summary>
internal sealed class ClipboardHotkey : IDisposable
{
    public const string Label = "Ctrl+Alt+Shift+V";

    const int WM_HOTKEY = 0x0312;
    const int HotkeyId = 0x524F;
    const uint MOD_ALT = 1, MOD_CONTROL = 2, MOD_SHIFT = 4, MOD_NOREPEAT = 0x4000, VK_V = 0x56;

    readonly HwndSource _window = new(new HwndSourceParameters("RotivaClipboardHotkey") { WindowStyle = 0 });
    readonly Action _pressed;

    public ClipboardHotkey(Action pressed)
    {
        _pressed = pressed;
        _window.AddHook(OnMessage);
        RegisterHotKey(_window.Handle, HotkeyId, MOD_CONTROL | MOD_ALT | MOD_SHIFT | MOD_NOREPEAT, VK_V);
    }

    IntPtr OnMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message == WM_HOTKEY && wParam == HotkeyId)
        {
            handled = true;
            _pressed();
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        UnregisterHotKey(_window.Handle, HotkeyId);
        _window.Dispose();
    }

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
