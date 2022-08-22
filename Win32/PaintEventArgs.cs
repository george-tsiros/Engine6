namespace Win32;

using System;

public class PaintEventArgs:EventArgs {
    public readonly IntPtr Dc;
    public readonly Rectangle Rect;
    public PaintEventArgs (IntPtr dc, Rectangle rect) {
        Dc = dc;
        Rect = rect;
    }
}
