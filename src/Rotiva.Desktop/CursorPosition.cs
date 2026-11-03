using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Platform;

namespace Rotiva.Desktop;

/// <summary>
/// Posisi kursor mouse di layar (piksel, seperti <see cref="Avalonia.Controls.Window.Position"/>), supaya
/// roda dibuka tepat di tempat file dipilih atau dilepas. Avalonia tidak punya API untuk ini, jadi
/// dibaca dari OS: Win32 di Windows, CoreGraphics di macOS, X11 di Linux. Null kalau tidak bisa
/// dibaca (mis. sesi Wayland tanpa XWayland): roda lalu dibuka di tengah layar.
/// </summary>
static partial class CursorPosition
{
    public static PixelPoint? Get(IReadOnlyList<Screen> screens)
    {
        try
        {
            if (OperatingSystem.IsWindows())
                return GetCursorPos(out var p) ? new PixelPoint(p.X, p.Y) : null;
            if (OperatingSystem.IsMacOS())
                return FromMacPoints(MacCursor(), screens);
            // Di WSL (WSLg) X11 hanya tahu kursor saat kursor di atas jendela Linux; selain itu
            // ia melaporkan titik tetap (320, 240). Tengah layar lebih masuk akal di sana.
            if (OperatingSystem.IsLinux() && Environment.GetEnvironmentVariable("WSL_DISTRO_NAME") is null)
                return X11Cursor();
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
        {
            // Library OS tidak tersedia (mis. Linux tanpa libX11).
        }
        return null;
    }

    // --- Windows ---

    [StructLayout(LayoutKind.Sequential)]
    struct Win32Point
    {
        public int X;
        public int Y;
    }

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetCursorPos(out Win32Point point);

    // --- macOS ---

    const string CoreGraphics = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";
    const string CoreFoundation = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

    [StructLayout(LayoutKind.Sequential)]
    struct CGPoint
    {
        public double X;
        public double Y;
    }

    [LibraryImport(CoreGraphics)]
    private static partial IntPtr CGEventCreate(IntPtr source);

    [LibraryImport(CoreGraphics)]
    private static partial CGPoint CGEventGetLocation(IntPtr evt);

    [LibraryImport(CoreFoundation)]
    private static partial void CFRelease(IntPtr obj);

    // Titik dalam "points" macOS, pojok kiri atas layar utama = (0, 0).
    static CGPoint MacCursor()
    {
        IntPtr evt = CGEventCreate(IntPtr.Zero);
        try
        {
            return CGEventGetLocation(evt);
        }
        finally
        {
            CFRelease(evt);
        }
    }

    // Avalonia memakai piksel (points × skala layar Retina): cari layar yang berisi titik itu.
    static PixelPoint? FromMacPoints(CGPoint point, IReadOnlyList<Screen> screens)
    {
        foreach (var screen in screens)
        {
            var b = screen.Bounds;
            double s = screen.Scaling;
            if (point.X >= b.X / s && point.X < b.Right / s && point.Y >= b.Y / s && point.Y < b.Bottom / s)
                return new PixelPoint((int)(point.X * s), (int)(point.Y * s));
        }
        return null;
    }

    // --- Linux (X11, juga XWayland) ---

    const string X11 = "libX11.so.6";

    [LibraryImport(X11)]
    private static partial IntPtr XOpenDisplay(IntPtr name);

    [LibraryImport(X11)]
    private static partial IntPtr XDefaultRootWindow(IntPtr display);

    [LibraryImport(X11)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool XQueryPointer(IntPtr display, IntPtr window, out IntPtr root, out IntPtr child,
        out int rootX, out int rootY, out int windowX, out int windowY, out uint mask);

    [LibraryImport(X11)]
    private static partial int XCloseDisplay(IntPtr display);

    static PixelPoint? X11Cursor()
    {
        IntPtr display = XOpenDisplay(IntPtr.Zero);
        if (display == IntPtr.Zero)
            return null;
        try
        {
            return XQueryPointer(display, XDefaultRootWindow(display), out _, out _, out int x, out int y, out _, out _, out _)
                ? new PixelPoint(x, y)
                : null;
        }
        finally
        {
            XCloseDisplay(display);
        }
    }
}
