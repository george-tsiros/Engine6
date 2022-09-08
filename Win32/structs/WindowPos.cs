namespace Win32;

using System;
using static Common.Functions;
public struct WindowPos {
    public IntPtr window;
    public IntPtr insertAfter;
    public int x;
    public int y;
    public int w;
    public int h;
    public WindowPosFlags flags;
    public override string ToString () => $"{x}, {y}, {w} x {h}, {string.Join(", ", ToFlags(flags))}";
}
