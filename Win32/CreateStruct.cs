namespace Win32;

using System;

public struct CreateStruct {
    public IntPtr lpCreateParams;
    public IntPtr hInstance;
    public IntPtr hMenu;
    public IntPtr hwndParent;
    public int cy;
    public int cx;
    public int y;
    public int x;
    public int style;
    public IntPtr lpszName;
    public IntPtr lpszClass;
    public ulong dwExStyle;
}
