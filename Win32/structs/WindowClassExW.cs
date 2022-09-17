namespace Win32;

using System.Runtime.InteropServices;

public struct WindowClassExW {
    public uint size = (uint)Marshal.SizeOf<WindowClassExW>();
    public ClassStyle style = ClassStyle.None;
    public WndProc wndProc = null;
    public int cbClsExtra = 0;
    public int cbWndExtra = 0;
    public nint hInstance = 0;
    public nint hIcon = 0;
    public nint hCursor = 0;
    public nint hbrBackground = 0;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string lpszmenuname = null;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string classname = null;
    public nint hIconsm = 0;
    public WindowClassExW () { }
}
