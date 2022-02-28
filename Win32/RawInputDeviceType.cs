namespace Win32;

using System;

public unsafe struct PaintStruct {
    public IntPtr hdc;
    public IntPtr erase;
    public Rect paint;
    public IntPtr restore;
    public IntPtr incUpdate;
    public fixed byte reserved[22];
}
[Flags]
public enum RawInputDeviceType:uint {
    Mouse,
    Keyboard,
    Hid,
}

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
