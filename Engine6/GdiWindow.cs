namespace Engine6;

using System;
using System.Diagnostics;
using Common;
using Win32;

public class GdiWindow:Window {

    private Dib dib;

    protected override void OnKeyUp (Key k) {
        switch (k) {
            case Key.Escape:
                User32.PostQuitMessage(0);
                return;
        }
    }

    public GdiWindow (WindowStyle? style = null) : base(style) { }

    protected override void OnLoad () {
    }

    protected unsafe override void OnPaint (in Rectangle r) {
        var size = ClientSize;
        if (dib is null || dib.Width != size.X || dib.Height != size.Y) {
            dib?.Dispose();
            dib = new(Dc, size.X, size.Y);
        }
        dib.ClearU32(Color.Black);
        var y = -Font.Height;
        dib.DrawString(size.ToString(), Font, 0, y += Font.Height, Color.Cyan);
        Blit(Dc, new(new(), size), dib);
    }

    private unsafe static void Blit (DeviceContext dc, in Rectangle rect, Dib dib) {
        if (rect.Width != dib.Width || rect.Height != dib.Height)
            throw new ArgumentOutOfRangeException(nameof(dib), "not same size");
        _ = Gdi32.StretchDIBits((IntPtr)dc, 0, 0, rect.Width, rect.Height, 0, 0, dib.Width, dib.Height, dib.Pixels, dib.Info, 0, 0xcc0020);
    }
}
