namespace Engine6;

using Common;
using System;
using System.Diagnostics;
using Win32;

public class GdiWindow:Window {
    protected override WindowStyle Style => WindowStyle.ClipPopup;

    private Dib dib;

    protected override void OnKeyUp (Key k) {
        switch (k) {
            //case Keys.Space:
            //    dib.ClearU32(Color.Black);
            //    Invalidate();
            //    return;
            case Key.Escape:
                User32.PostQuitMessage(0);
                return;
        }
    }

    protected override void OnLoad () {
        dib = new(Dc, Rect.Width, Rect.Height);
        User32.SetWindow(WindowHandle, WindowStyle.Overlapped);
    }

    protected override void OnMouseLeave () {
    }

    private void MouseEnter () {
    }

    protected unsafe override void OnMouseMove (in Vector2i p) {
    }

    protected override void OnButtonDown (MouseButton depressed) {
    }

    protected override void OnButtonUp (MouseButton released) {
        if (released.HasFlag(MouseButton.Left))
            Invalidate();
    }

    protected override void OnSize (ResizeType type, in Vector2i clientSize) {
    }

    unsafe protected override void OnPaint (IntPtr dc, in Rectangle drawRegion) {
        if (dib is null)
            return;
        fixed (BitmapInfo* p = &dib.Info) {
            var wat = Gdi32.StretchDIBits(dc, 0, 0, drawRegion.Width, drawRegion.Height, 0, 0, dib.Width, dib.Height, dib.Pixels, p, 0, 0xcc0020);
        }
    }
}
