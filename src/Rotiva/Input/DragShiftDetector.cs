using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Rotiva.Interop;

namespace Rotiva.Input;

/// <summary>
/// Memantau mouse dan keyboard di seluruh Windows (low-level hook) untuk mendeteksi
/// Shift yang ditekan saat tombol kiri mouse sedang men-drag sesuatu.
/// Callback hook harus cepat, jadi event-nya dijalankan belakangan lewat Dispatcher.
/// </summary>
internal sealed class DragShiftDetector : IDisposable
{
    const int DragThreshold = 6;

    readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
    // Delegate disimpan di field supaya tidak di-garbage-collect selama hook aktif.
    readonly NativeMethods.HookProc _mouseProc;
    readonly NativeMethods.HookProc _keyboardProc;
    readonly IntPtr _mouseHook;
    readonly IntPtr _keyboardHook;
    bool _buttonDown;
    bool _dragging;
    bool _shiftDown;
    NativeMethods.POINT _downPoint;

    public DragShiftDetector()
    {
        _mouseProc = MouseProc;
        _keyboardProc = KeyboardProc;
        IntPtr module = NativeMethods.GetModuleHandle(null);
        _mouseHook = NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, _mouseProc, module, 0);
        if (_mouseHook == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error());
        _keyboardHook = NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, _keyboardProc, module, 0);
        if (_keyboardHook == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            NativeMethods.UnhookWindowsHookEx(_mouseHook);
            throw new Win32Exception(error);
        }
    }

    /// <summary>Shift baru saja ditekan saat sedang men-drag.</summary>
    public event Action? ShiftPressedDuringDrag;

    /// <summary>Tombol kiri mouse dilepas setelah drag.</summary>
    public event Action? DragEnded;

    public void Dispose()
    {
        NativeMethods.UnhookWindowsHookEx(_mouseHook);
        NativeMethods.UnhookWindowsHookEx(_keyboardHook);
    }

    IntPtr MouseProc(int code, IntPtr wParam, IntPtr lParam)
    {
        if (code >= 0)
        {
            switch ((int)wParam)
            {
                case NativeMethods.WM_LBUTTONDOWN:
                    _buttonDown = true;
                    _dragging = false;
                    _downPoint = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam).Point;
                    break;
                case NativeMethods.WM_MOUSEMOVE when _buttonDown && !_dragging:
                    var point = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam).Point;
                    _dragging = Math.Abs(point.X - _downPoint.X) > DragThreshold
                        || Math.Abs(point.Y - _downPoint.Y) > DragThreshold;
                    break;
                case NativeMethods.WM_LBUTTONUP:
                    bool wasDragging = _dragging;
                    _buttonDown = false;
                    _dragging = false;
                    if (wasDragging)
                        _dispatcher.BeginInvoke(() => DragEnded?.Invoke());
                    break;
            }
        }
        return NativeMethods.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
    }

    IntPtr KeyboardProc(int code, IntPtr wParam, IntPtr lParam)
    {
        if (code >= 0)
        {
            int vk = (int)Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam).VkCode;
            if (vk is NativeMethods.VK_SHIFT or NativeMethods.VK_LSHIFT or NativeMethods.VK_RSHIFT)
            {
                switch ((int)wParam)
                {
                    // Abaikan auto-repeat: hanya tekanan pertama yang dihitung.
                    case NativeMethods.WM_KEYDOWN or NativeMethods.WM_SYSKEYDOWN when !_shiftDown:
                        _shiftDown = true;
                        if (_dragging)
                            _dispatcher.BeginInvoke(() => ShiftPressedDuringDrag?.Invoke());
                        break;
                    case NativeMethods.WM_KEYUP or NativeMethods.WM_SYSKEYUP:
                        _shiftDown = false;
                        break;
                }
            }
        }
        return NativeMethods.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
    }
}
