using System;

namespace Win32;

public class GdiWindow:Window {
    protected override WindowStyle Style => WindowStyle.Visible;
    Dib dib;
    public GdiWindow () {
        dib = new(Dc, Rect.Width, Rect.Height);
    }

    protected override void OnKeyUp (Keys k) {
        switch (k) {
            case Keys.Escape:
                User32.PostQuitMessage(0);
                return;
        }
        base.OnKeyUp(k);
    }

    protected override void OnPaint (IntPtr dc, Rect rect) {
        _ = Gdi32.StretchDIBits(dc, 0, 0, rect.Width, rect.Height, 0, 0, dib.Width, dib.Height, dib.Bits, in dib.Info, 0, 0xcc0020);
    }
}
