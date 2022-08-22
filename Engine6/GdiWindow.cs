namespace Engine6;

using System;
using System.Diagnostics;
using Win32;

public class GdiWindow:Window {
    protected override WindowStyle Style => WindowStyle.Visible;

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
        for (var y = 0; y < dib.Height; ++y)
            for (var x = 0; x < dib.Width; ++x) {
                if (x == 0 || y == 0 || x == dib.Width - 1 || y == dib.Height - 1)
                    dib.Raw[y * dib.Stride + x] = 0xffff00ff;
                else
                    dib.Raw[y * dib.Stride + x] = 0xff00ff00;
            }
        frameDelay = Stopwatch.Frequency / 10;
        User32.SetWindow(WindowHandle, WindowStyle.Overlapped);
    }
    long frameDelay;
    long lastTicks;
    protected override void OnIdle () {
        if (lastTicks+frameDelay< Stopwatch.GetTimestamp())
            Invalidate();
    }

    unsafe protected override void OnPaint (IntPtr dc, in Rectangle rect) {
        Debug.WriteLine("paint");
        fixed (BitmapInfo* p=&dib.Info) {
            var wat = Gdi32.StretchDIBits(dc, 0, 0, rect.Width, rect.Height, 0, 0, dib.Width, dib.Height, dib.Raw, p, 0, 0xcc0020);
            Debug.Assert(0 != wat);
        }
        lastTicks = Stopwatch.GetTimestamp();
    }
}
