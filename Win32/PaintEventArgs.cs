namespace Win32;

using System;

public class PaintEventArgs:EventArgs {
    public readonly IntPtr Dc;
    public readonly Rect Rect;
    public PaintEventArgs (IntPtr dc, Rect rect) {
        Dc = dc;
        Rect = rect;
    }
}
