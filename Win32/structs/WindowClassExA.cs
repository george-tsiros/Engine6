namespace Win32;

using System;
using System.Runtime.InteropServices;

public struct WindowClassExA {
    public uint size = (uint)Marshal.SizeOf<WindowClassExA>();
    public ClassStyle style = ClassStyle.HRedraw | ClassStyle.VRedraw;
    public WndProc wndProc = null;
    public int cbClsExtra = 0;
    public int cbWndExtra = 0;
    public IntPtr hInstance = IntPtr.Zero;
    public IntPtr hIcon = IntPtr.Zero;
    public IntPtr hCursor = IntPtr.Zero;
    public IntPtr hbrBackground = IntPtr.Zero;
    [MarshalAs(UnmanagedType.LPStr)]
    public string lpszmenuname = null;
    [MarshalAs(UnmanagedType.LPStr)]
    public string classname = null;
    public IntPtr hIconsm = IntPtr.Zero;
    public WindowClassExA () { }
}
