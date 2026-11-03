using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Threading;

namespace Rotiva.Desktop;

/// <summary>
/// Gesture Rotiva di macOS dan Linux: tombol kiri ditahan, kursor sudah bergerak (sebuah drag),
/// lalu Shift ditekan. Tanpa hook global: status tombol dan Shift dibaca berkala dari OS
/// (X11 XQueryPointer di Linux, CGEventSource di macOS), cukup ringan untuk 25×/detik.
/// Di Wayland murni (tanpa XWayland) status global tidak bisa dibaca, jadi gesture tidak aktif.
/// </summary>
sealed partial class DragShiftGesture : IDisposable
{
    const int DragThresholdPx = 8;

    readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(40) };
    readonly IPointerState _state;
    bool _buttonDown;
    bool _shiftDown;
    bool _triggered;
    PixelPoint _pressedAt;

    DragShiftGesture(IPointerState state)
    {
        _state = state;
        _timer.Tick += (_, _) => Poll();
    }

    /// <summary>Shift ditekan saat drag; berisi posisi kursor (piksel layar).</summary>
    public event Action<PixelPoint>? ShiftPressedDuringDrag;

    /// <summary>Tombol kiri dilepas setelah gesture terpicu.</summary>
    public event Action? DragEnded;

    /// <summary>Null kalau OS ini tidak bisa dibaca statusnya (Windows punya aplikasinya sendiri).</summary>
    public static DragShiftGesture? TryCreate(Func<IReadOnlyList<Avalonia.Platform.Screen>> screens)
    {
        try
        {
            IPointerState? state = OperatingSystem.IsMacOS() ? new MacPointerState(screens)
                : OperatingSystem.IsLinux() ? X11PointerState.TryOpen()
                : null;
            return state is null ? null : new DragShiftGesture(state);
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
        {
            return null;
        }
    }

    public bool Enabled
    {
        get => _timer.IsEnabled;
        set => _timer.IsEnabled = value;
    }

    void Poll()
    {
        if (!_state.TryRead(out var position, out bool button, out bool shift))
            return;

        if (button && !_buttonDown)
        {
            _pressedAt = position;
            _triggered = false;
        }
        else if (!button && _buttonDown && _triggered)
        {
            DragEnded?.Invoke();
        }

        // Shift baru ditekan (bukan ditahan sejak sebelum drag) saat tombol ditahan dan kursor sudah bergerak.
        bool moved = Math.Abs(position.X - _pressedAt.X) + Math.Abs(position.Y - _pressedAt.Y) >= DragThresholdPx;
        if (button && moved && shift && !_shiftDown && !_triggered)
        {
            _triggered = true;
            Debug($"gesture at {position}");
            ShiftPressedDuringDrag?.Invoke(position);
        }
        if (button != _buttonDown || shift != _shiftDown)
            Debug($"button={button} shift={shift} at {position} (pressed at {_pressedAt})");

        _buttonDown = button;
        _shiftDown = shift;
    }

    public void Dispose()
    {
        _timer.Stop();
        _state.Dispose();
    }

    // ROTIVA_DEBUG=1: perubahan status tombol/Shift dan gesture ke stderr (untuk tes CI).
    static readonly bool DebugEnabled = Environment.GetEnvironmentVariable("ROTIVA_DEBUG") == "1";

    static void Debug(string message)
    {
        if (DebugEnabled)
            Console.Error.WriteLine($"[rotiva] {message}");
    }

    interface IPointerState : IDisposable
    {
        bool TryRead(out PixelPoint position, out bool leftButton, out bool shift);
    }

    // --- Linux (X11, juga XWayland) ---

    const string X11 = "libX11.so.6";
    const uint ShiftMask = 1 << 0;
    const uint Button1Mask = 1 << 8;

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

    sealed class X11PointerState(IntPtr display) : IPointerState
    {
        readonly IntPtr _root = XDefaultRootWindow(display);

        public static X11PointerState? TryOpen()
        {
            // WSLg: X11 tidak melihat mouse di luar jendela Linux, jadi gesture tidak berguna di sana.
            if (Environment.GetEnvironmentVariable("WSL_DISTRO_NAME") is not null)
                return null;
            IntPtr display = XOpenDisplay(IntPtr.Zero);
            return display == IntPtr.Zero ? null : new X11PointerState(display);
        }

        public bool TryRead(out PixelPoint position, out bool leftButton, out bool shift)
        {
            bool ok = XQueryPointer(display, _root, out _, out _, out int x, out int y, out _, out _, out uint mask);
            position = new PixelPoint(x, y);
            leftButton = ok && (mask & Button1Mask) != 0;
            shift = ok && (mask & ShiftMask) != 0;
            return ok;
        }

        public void Dispose() => XCloseDisplay(display);
    }

    // --- macOS ---

    const string CoreGraphics = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";
    const int CombinedSessionState = 0;
    const uint LeftMouseButton = 0;
    const ulong ShiftFlag = 0x00020000;

    [LibraryImport(CoreGraphics)]
    [return: MarshalAs(UnmanagedType.U1)]
    private static partial bool CGEventSourceButtonState(int stateId, uint button);

    [LibraryImport(CoreGraphics)]
    private static partial ulong CGEventSourceFlagsState(int stateId);

    /// <param name="screens">Layar saat ini: posisi kursor macOS (points) dikonversi ke piksel per layar.</param>
    sealed class MacPointerState(Func<IReadOnlyList<Avalonia.Platform.Screen>> screens) : IPointerState
    {
        public bool TryRead(out PixelPoint position, out bool leftButton, out bool shift)
        {
            leftButton = CGEventSourceButtonState(CombinedSessionState, LeftMouseButton);
            shift = (CGEventSourceFlagsState(CombinedSessionState) & ShiftFlag) != 0;
            var cursor = CursorPosition.Get(screens());
            position = cursor ?? default;
            return cursor is not null;
        }

        public void Dispose() { }
    }
}
