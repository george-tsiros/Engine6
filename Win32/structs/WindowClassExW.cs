namespace Win32;

using System;
using System.Runtime.InteropServices;

public struct WindowClassExW {
    public uint size = (uint)Marshal.SizeOf<WindowClassExW>();
    public ClassStyle style = ClassStyle.None;
    public WndProc wndProc = null;
    public int cbClsExtra = 0;
    public int cbWndExtra = 0;
    public IntPtr hInstance = IntPtr.Zero;
    public IntPtr hIcon = IntPtr.Zero;
    public IntPtr hCursor = IntPtr.Zero;
    public IntPtr hbrBackground = IntPtr.Zero;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string lpszmenuname = null;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string classname = null;
    public IntPtr hIconsm = IntPtr.Zero;
    public WindowClassExW () { }
}
