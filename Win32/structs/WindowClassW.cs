namespace Win32;

using System.Runtime.InteropServices;

public struct WindowClassW {
    public ClassStyle style;
    public WndProc wndProc;
    public int cbClsExtra;
    public int cbWndExtra;
    public nint hInstance;
    public nint hIcon;
    public nint hCursor;
    public nint hbrBackground;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string lpszmenuname;
    [MarshalAs(UnmanagedType.LPWStr)]
    public string classname;
}
