namespace Win32;

using System;

public unsafe struct PaintStruct {
    public IntPtr deviceContext;
    public int erase;
    public Rectangle rect;
    public int restore;
    public int incUpdate;
    public fixed byte reserved[32];
}
