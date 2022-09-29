namespace Engine6;

using Common;
using System.Diagnostics;
using Win32;

public class GdiWindow:Window {

    public Color BackgroundColor { get; set; }
    protected Dib Dib { get; private set; }

    protected override void OnSize (SizeType sizeType, in Vector2i size) {
        User32.InvalidateWindow(this);
    }

    protected override void OnPaint (nint dc, in PaintStruct ps) {
        Resize();
        if (Dib is null)
            return;
        Dib.ClearU32(BackgroundColor);
        Blit(Dc, ClientSize, Dib);
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

    protected static void Blit (DeviceContext dc, in Vector2i targetSize, Dib dib) {
        Debug.Assert(targetSize == dib.Size);
        _ = Gdi32.StretchDIBits(dc, new(in Vector2i.Zero, targetSize), new(in Vector2i.Zero, dib.Size), dib, RasterOperation.SrcCopy);
    }
}
