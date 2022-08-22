namespace Win32;

using System;

public unsafe struct PaintStruct {
    public IntPtr hdc;
    public IntPtr erase;
    public Rectangle paint;
    public IntPtr restore;
    public IntPtr incUpdate;
    public fixed byte reserved[22];
}
