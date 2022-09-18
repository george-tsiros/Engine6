namespace Engine6;
using Common;
using System;
using System.Diagnostics;
using Win32;

public class GdiWindow:Window {

    public GdiWindow () : base() {
        Paint += OnPaint;
        Size += OnSize;
        Idle += OnIdle;
    }

    private void OnIdle (object sender, EventArgs _) {
        ++idleCount;
        using var text = idleCount.ToAscii();
        User32.SetWindowText(this, text);
    }

    private int idleCount;
    private Dib dib;

    private void OnSize (object sender, SizeEventArgs _) {
        User32.InvalidateWindow(this);
    }

    private void OnPaint (object sender, PaintEventArgs _) {
        Resize();
        var size = ClientSize;
        dib.ClearU32(Color.Black);
        Blit(Dc, new(new(), size), dib);
    }

    private void Resize () {
        if (dib is not null && dib.Size != ClientSize) {
            dib.Dispose();
            dib = null;
        }
        if (dib is null) {
            dib = new(Dc, ClientSize);
        }
    }

    private static void Blit (DeviceContext dc, in Rectangle rect, Dib dib) {
        Debug.Assert(rect.Size == dib.Size);
        _ = Gdi32.StretchDIBits(dc, rect, new(new(), dib.Size), dib, RasterOperation.SrcCopy);
    }
}
