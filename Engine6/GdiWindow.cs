namespace Engine6;
using System.Diagnostics;
using Win32;

public class GdiWindow:Window {

    protected Dib Dib { get; private set; }

    protected override void OnSize (in SizeArgs _) {
        User32.InvalidateWindow(this);
    }

    protected override void OnPaint (in PaintArgs _) {
        Resize();
        Dib.ClearU32(Color.Black);
        Blit(Dc, new(new(), ClientSize), Dib);
    }

    protected void Resize () {
        var size = ClientSize;
        if (Dib is not null && Dib.Size != size) {
            Dib.Dispose();
            Dib = null;
        }
        if (Dib is null && 0 != size.X && 0 != size.Y) {
            Dib = new(Dc, size);
        }
    }

    protected static void Blit (DeviceContext dc, in Rectangle rect, Dib dib) {
        Debug.Assert(rect.Size == dib.Size);
        _ = Gdi32.StretchDIBits(dc, rect, new(new(), dib.Size), dib, RasterOperation.SrcCopy);
    }
}
