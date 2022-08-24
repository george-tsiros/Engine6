namespace Engine6;

using Common;
using System;
using System.Diagnostics;
using Win32;

public class GdiWindow:Window {
    protected override WindowStyle Style => WindowStyle.ClipPopup;

    private Dib dib;

    protected override void OnKeyUp (Keys k) {
        switch (k) {
            case Keys.Escape:
                User32.PostQuitMessage(0);
                return;
        }
    }

    unsafe protected override void OnLoad () {
        dib = new(Dc, Rect.Width, Rect.Height);
        frameDelay = Stopwatch.Frequency / 10;
        User32.SetWindow(WindowHandle, WindowStyle.Overlapped);
    }
    long frameDelay;
    long lastTicks;
    protected override void OnIdle () {
        if (lastTicks + frameDelay < Stopwatch.GetTimestamp())
            Invalidate();
    }

    protected override void OnMouseMove (in Vector2i p) {
        cursorLocation = p;
    }

    protected override void OnButtonDown (Buttons depressed) {
     //   buttanz |= depressed;
    }
    protected override void OnButtonUp (Buttons released) {
    //    buttanz &= ~released;
    }
    bool leftIsDown;
    bool mouseIsIn;
    Vector2i cursorLocation;
    unsafe protected override void OnPaint (IntPtr dc, in Rectangle rect) {
        if (dib is null)
            return;
        dib.ClearU32(Color.Black);
        fixed (BitmapInfo* p = &dib.Info) {
            var wat = Gdi32.StretchDIBits(dc, 0, 0, rect.Width, rect.Height, 0, 0, dib.Width, dib.Height, dib.Raw, p, 0, 0xcc0020);
            Debug.Assert(0 != wat);
        }
        lastTicks = Stopwatch.GetTimestamp();
    }
}
