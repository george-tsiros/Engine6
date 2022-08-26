namespace Engine6;

using Common;
using System;
using System.Collections.Generic;
using Win32;


public class GdiWindow:Window {
    protected override WindowStyle Style => WindowStyle.ClipPopup;

    public GdiWindow (Vector2i? size = null) : base(size) { }

    private Dib dib;

    protected override void OnKeyUp (Key k) {
        switch (k) {
            case Key.Down:
                //++rowOffset;
                break;
            case Key.Up:
                break;
            case Key.PageDown:
                break;
            case Key.PageUp:
                break;
            case Key.Escape:
                User32.PostQuitMessage(0);
                break;
            default:
                return;
        }
        Invalidate();
    }

    int rowOffset;
    List<PixelFormatDescriptor> pfds = new();
    List<string> strings = new();
    int rows;
    protected override void OnLoad () {
        dib = new(Dc, Rect.Width, Rect.Height);
        User32.SetWindow(WindowHandle, WindowStyle.Overlapped);
        var count = Gdi32.GetPixelFormatCount(Dc);
        var d = new PixelFormatDescriptor() { size = PixelFormatDescriptor.Size, version = 1 };
        for (var (i, rowIndex) = (1, 0); i <= count; ++i) {
            Gdi32.DescribePixelFormat(Dc, i, ref d);
            if (0 == d.pixelType && 32 == d.colorBits && 24 <= d.depthBits && d.flags.HasFlag(PixelFlag.SupportOpengl) && !d.flags.HasFlag(PixelFlag.GenericAccelerated) && !d.flags.HasFlag(PixelFlag.GenericFormat)) {
                pfds.Add(d);
                strings.Add($"{rowIndex,3}: {d}");
                ++rowIndex;
            }
        }
        rows = Rect.Height / Font.Height;
    }


    Vector2i lastCursorLocation = new(-1, -1);
    protected override void OnButtonDown (MouseButton depressed) {
        if (depressed.HasFlag(MouseButton.Right))
            lastCursorLocation = CursorLocation;
    }
    protected override void OnButtonUp (MouseButton released) {

    }

    protected override void OnMouseMove (in Vector2i p) {
        if (Buttons.HasFlag(MouseButton.Right)) {
            var delta = p - lastCursorLocation;
            lastCursorLocation = p;
            var pp = Rect.Location + delta;
            User32.SetWindowPos(WindowHandle, IntPtr.Zero, pp.X, pp.Y, 0, 0, WindowPosFlags.DeferErase | WindowPosFlags.NoSize | WindowPosFlags.NoZOrder | WindowPosFlags.NoSendChanging);
        }
    }


    unsafe protected override void OnPaint (IntPtr dc, in Rectangle drawRegion) {
        if (dib is null)
            return;
        dib.ClearU32(Color.Black);
        //dib.DrawString(strings.Count.ToString(), Font, 0, 0, Color.Green);
        dib.DrawString(lastCursorLocation.ToString(), Font, 0, 0, Color.Green);
        //for (var (i, y) = (rowOffset, -Font.Height); i < strings.Count && y + Font.Height < Rect.Height; ++i, y += Font.Height) {
        //    dib.DrawString(strings[i], Font, 0, y, Color.Green);
        //}
        var wat = Gdi32.StretchDIBits(dc, 0, 0, drawRegion.Width, drawRegion.Height, 0, 0, dib.Width, dib.Height, dib.Pixels, dib.Info, 0, 0xcc0020);
    }
}
