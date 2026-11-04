using System.Runtime.InteropServices;
using System.Windows;

namespace Rotiva.Interop;

internal static class NativeMethods
{
    public const int WH_KEYBOARD_LL = 13;
    public const int WH_MOUSE_LL = 14;
    public const int WM_KEYDOWN = 0x0100;
    public const int WM_KEYUP = 0x0101;
    public const int WM_SYSKEYDOWN = 0x0104;
    public const int WM_SYSKEYUP = 0x0105;
    public const int WM_MOUSEMOVE = 0x0200;
    public const int WM_LBUTTONDOWN = 0x0201;
    public const int WM_LBUTTONUP = 0x0202;
    public const int VK_SHIFT = 0x10;
    public const int VK_LSHIFT = 0xA0;
    public const int VK_RSHIFT = 0xA1;
    public const int VK_LBUTTON = 0x01;
    public const int VK_RBUTTON = 0x02;
    public const int VK_ESCAPE = 0x1B;

    const uint MONITOR_DEFAULTTONEAREST = 2;
    const int GWL_EXSTYLE = -20;
    const int WS_EX_TRANSPARENT = 0x00000020;
    const int WS_EX_TOOLWINDOW = 0x00000080;
    const int WS_EX_NOACTIVATE = 0x08000000;

    public delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHOOKSTRUCT
    {
        public POINT Point;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KBDLLHOOKSTRUCT
    {
        public uint VkCode;
        public uint ScanCode;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public readonly Int32Rect ToInt32Rect() => new(Left, Top, Right - Left, Bottom - Top);
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MONITORINFO
    {
        public int Size;
        public RECT Monitor;
        public RECT Work;
        public uint Flags;
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetWindowsHookEx(int idHook, HookProc callback, IntPtr module, uint threadId);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWindowsHookEx(IntPtr hook);

    [DllImport("user32.dll")]
    public static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr GetModuleHandle(string? moduleName);

    [DllImport("shell32.dll")]
    static extern void SHChangeNotify(int eventId, uint flags, IntPtr item1, IntPtr item2);

    /// <summary>Minta Explorer memuat ulang menu klik kanan (SHCNE_ASSOCCHANGED).</summary>
    public static void NotifyAssociationsChanged() => SHChangeNotify(0x08000000, 0, IntPtr.Zero, IntPtr.Zero);

    /// <summary>Urutan nama ala Explorer: "foto 2" sebelum "foto 10".</summary>
    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
    public static extern int StrCmpLogicalW(string a, string b);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(out POINT point);

    [DllImport("user32.dll")]
    static extern IntPtr MonitorFromPoint(POINT point, uint flags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetMonitorInfo(IntPtr monitor, ref MONITORINFO info);

    [DllImport("user32.dll")]
    static extern int GetWindowLong(IntPtr hWnd, int index);

    [DllImport("user32.dll")]
    static extern int SetWindowLong(IntPtr hWnd, int index, int value);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int key);

    /// <summary>Tombol keyboard/mouse sedang ditekan (di mana pun, tanpa perlu fokus).</summary>
    public static bool IsKeyDown(int key) => (GetAsyncKeyState(key) & 0x8000) != 0;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    static extern int GetCurrentPackageFullName(ref int length, char[]? name);

    const int APPMODEL_ERROR_NO_PACKAGE = 15700;

    /// <summary>True kalau berjalan dari paket MSIX (Microsoft Store), bukan dari installer biasa.</summary>
    public static bool IsPackaged { get; } = DetectPackage();

    static bool DetectPackage()
    {
        int length = 0;
        return GetCurrentPackageFullName(ref length, null) != APPMODEL_ERROR_NO_PACKAGE;
    }

    /// <summary>Batas monitor tempat kursor berada, dalam piksel layar.</summary>
    public static Int32Rect GetMonitorBoundsAtCursor() => MonitorAtCursor().Monitor.ToInt32Rect();

    /// <summary>Area kerja (tanpa taskbar) monitor tempat kursor berada, dalam piksel layar.</summary>
    public static Int32Rect GetWorkAreaAtCursor() => MonitorAtCursor().Work.ToInt32Rect();

    static MONITORINFO MonitorAtCursor()
    {
        GetCursorPos(out var cursor);
        var info = new MONITORINFO { Size = Marshal.SizeOf<MONITORINFO>() };
        GetMonitorInfo(MonitorFromPoint(cursor, MONITOR_DEFAULTTONEAREST), ref info);
        return info;
    }

    /// <summary>
    /// Jendela tidak mengambil fokus dan tidak muncul di Alt+Tab. Dengan
    /// <paramref name="clickThrough"/>, klik mouse juga tembus ke jendela di bawahnya.
    /// </summary>
    public static void MakeOverlayWindow(IntPtr hWnd, bool clickThrough)
    {
        int style = GetWindowLong(hWnd, GWL_EXSTYLE) | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
        if (clickThrough)
            style |= WS_EX_TRANSPARENT;
        SetWindowLong(hWnd, GWL_EXSTYLE, style);
    }

    /// <summary>Nyalakan/matikan klik tembus pada jendela yang sudah dibuat dengan <see cref="MakeOverlayWindow"/>.</summary>
    public static void SetClickThrough(IntPtr hWnd, bool clickThrough)
    {
        int style = GetWindowLong(hWnd, GWL_EXSTYLE);
        style = clickThrough ? style | WS_EX_TRANSPARENT : style & ~WS_EX_TRANSPARENT;
        SetWindowLong(hWnd, GWL_EXSTYLE, style);
    }

}
