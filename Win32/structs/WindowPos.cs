using System;

namespace Win32;

public struct WindowPos {
    public IntPtr window;
    public IntPtr insertAfter;
    public int x;
    public int y;
    public int w;
    public int h;
    public WindowPosFlags flags;
    public override string ToString () => $"0x{window:x} after 0x{insertAfter:x} @({x},{y}), {w}x{h}, {flags}";
}
