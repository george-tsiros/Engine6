namespace Engine6;

using System;
using System.Diagnostics;
using Common;
using Win32;

public class GdiWindow:Window {

    private const WindowPosFlags SelfMoveFlags = WindowPosFlags.NoSize | WindowPosFlags.NoSendChanging | WindowPosFlags.NoRedraw | WindowPosFlags.NoZOrder;
    private Vector2i lastCursorLocation = new(-1, -1);
    private Dib dib;

    public GdiWindow () : base() {
        var count = Gdi32.GetPixelFormatCount(Dc);
        PixelFormatDescriptor pfd = new() {
            size= PixelFormatDescriptor.Size,
            version = 1,
        };
        for (var i = 1; i <= count; ++i) {
            Gdi32.DescribePixelFormat(Dc, i, ref pfd);
            Debug.WriteLine(pfd);
        }
    }

    protected override void OnKeyUp (Key k) {
        switch (k) {
            case Key.Escape:
                User32.PostQuitMessage(0);
                return;
        }
    }

    protected override void OnButtonDown (MouseButton depressed, PointShort p) {
        switch (depressed) {
            case MouseButton.Right:
            lastCursorLocation = CursorLocation;
                return;
        }
    }

    protected override void OnLoad () {
        User32.SetWindow(Handle, WindowStyle.Overlapped);
    }

    protected unsafe override void OnPaint () {
        var size = Size;
        if (dib is null || dib.Width != size.X || dib.Height != size.Y) {
            dib?.Dispose();
            dib = new(Dc, size.X, size.Y);
        }
        dib.ClearU32(Color.Black);
        var y = -Font.Height;
        dib.DrawString(size.ToString(), Font, 0, y += Font.Height, Color.Cyan);
        Blit(Dc, new(new(),size), dib);
    }

    private unsafe static void Blit (DeviceContext dc,in Rectangle rect, Dib dib) {
        if (rect.Width != dib.Width || rect.Height != dib.Height)
            throw new ArgumentOutOfRangeException(nameof(dib), "not same size");
        _ = Gdi32.StretchDIBits((IntPtr)dc, 0, 0, rect.Width, rect.Height, 0, 0, dib.Width, dib.Height, dib.Pixels, dib.Info, 0, 0xcc0020);
    }
}
