namespace Win32;

using System;
using System.Runtime.InteropServices;

public unsafe struct WindowClassExW {
    public uint size;
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
    public IntPtr hIconsm;
    public static WindowClassExW Create () 
        => new() { size = (uint)Marshal.SizeOf<WindowClassExW>() };
}
