namespace Win32;

using System;
using System.Runtime.InteropServices;

public struct WindowClassW {
    public ClassStyle style;
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
}
