namespace Engine6;
using Common;
using System;
using System.Diagnostics;
using Win32;

public class GdiWindow:Window {

    public GdiWindow () : base() {
        Paint += OnPaint;
        Size += OnSize;
    }

    private Dib dib;

    private void OnSize (object sender, SizeEventArgs _) {
        User32.InvalidateWindow(this);
    }

    private void OnPaint (object sender, PaintEventArgs _) {
        Resize();
        dib.ClearU32(Color.Black);
        Blit(Dc, new(new(), ClientSize), dib);
    }

    private void Resize () {
        var size = ClientSize;
        if (dib is not null && dib.Size != size) {
            dib.Dispose();
            dib = null;
        }
        if (dib is null) {
            dib = new(Dc, size);
        }
    }

    private static void Blit (DeviceContext dc, in Rectangle rect, Dib dib) {
        Debug.Assert(rect.Size == dib.Size);
        _ = Gdi32.StretchDIBits(dc, rect, new(new(), dib.Size), dib, RasterOperation.SrcCopy);
    }
}
