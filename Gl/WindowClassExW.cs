namespace Gl;

using System;
using System.Runtime.InteropServices;
internal delegate IntPtr WndProc (IntPtr hWnd, WinMessage msg, IntPtr wparam, IntPtr lparam);

internal unsafe struct WindowClassExW {
    public uint size;
    public uint style;
    public WndProc wndProc;
    public int cbClsExtra;
    public int cbWndExtra;
    public IntPtr hInstance;
    public IntPtr hIcon;
    public IntPtr hCursor;
    public IntPtr hbrBackground;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string lpszmenuname;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string classname;
    public IntPtr hIconsm;
    public static WindowClassExW Create () => new() { size = (uint)Marshal.SizeOf<WindowClassExW>() };
}
