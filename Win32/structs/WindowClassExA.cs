namespace Win32;

using System;
using System.Runtime.InteropServices;

public struct WindowClassExA {
    public uint size;
    public ClassStyle style;
    public WndProc wndProc;
    public int cbClsExtra;
    public int cbWndExtra;
    public IntPtr hInstance;
    public IntPtr hIcon;
    public IntPtr hCursor;
    public IntPtr hbrBackground;
    [MarshalAs(UnmanagedType.LPStr)]
    public string lpszmenuname;
    [MarshalAs(UnmanagedType.LPStr)]
    public string classname;
    public IntPtr hIconsm;
    public WindowClassExA () {
        size = (uint)Marshal.SizeOf<WindowClassExA>();
    }
}
