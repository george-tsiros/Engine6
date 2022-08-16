namespace Win32;
using System;

public struct WindowPos {
    public nint window;
    public nint insertAfter;
    public int left;
    public int top;
    public int width;
    public int height;
    public WindowPosFlags flags;
    public override string ToString () => $"0x{window:x} after 0x{insertAfter:x} @({left},{top}), {width}x{height}, {flags}";
}
