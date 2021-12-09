namespace Win32;
using System;

public struct WindowPos {
    public IntPtr window;
    public IntPtr insertAfter;
    public int x;
    public int y;
    public int cx;
    public int cy;
    public WindowPosFlags flags;
}
