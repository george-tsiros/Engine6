namespace Engine6;
using Common;
using System;
using System.Diagnostics;
using Win32;

public class EditorWindow:GdiWindow {
    public EditorWindow () : base() {
        KeyDown += OnKeyDown;
    }

    private void OnKeyDown (object sender, KeyEventArgs e) {
    }
}

public class GdiWindow:Window {

    public GdiWindow () : base() {
        Paint += OnPaint;
        Size += OnSize;
    }

    protected Dib Dib { get; private set; }

    private void OnSize (object sender, SizeEventArgs _) {
        User32.InvalidateWindow(this);
    }

    private void OnPaint (object sender, PaintEventArgs _) {
        Resize();
        Dib.ClearU32(Color.Black);
        Blit(Dc, new(new(), ClientSize), Dib);
    }

    private void Resize () {
        var size = ClientSize;
        if (Dib is not null && Dib.Size != size) {
            Dib.Dispose();
            Dib = null;
        }
        if (Dib is null) {
            Dib = new(Dc, size);
        }
    }

    private static void Blit (DeviceContext dc, in Rectangle rect, Dib dib) {
        Debug.Assert(rect.Size == dib.Size);
        _ = Gdi32.StretchDIBits(dc, rect, new(new(), dib.Size), dib, RasterOperation.SrcCopy);
    }
}
