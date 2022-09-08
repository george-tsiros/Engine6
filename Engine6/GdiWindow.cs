namespace Engine6;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Common;
using Win32;

public class GdiWindow:Window {

    private Dib dib;

    protected override void OnKeyUp (Key k) {
        switch (k) {
            case Key.Escape:
                User32.PostQuitMessage(0);
                return;
        }
    }

    protected override void OnLoad () {
        Resize();
    }
    private void Resize () {
        dib?.Dispose();
        dib = new(Dc, ClientSize);
    }

    private readonly List<string> q = new();
    private void Append (string str) {
        var maxLines = ClientSize.Y / Font.Height;
        var overflow = q.Count + 1 - maxLines;
        if (0 < overflow)
            q.RemoveRange(0, overflow);
        q.Add(str);
    }

    protected override void OnSizing (SizingEdge edge, ref Rectangle r) =>
        Invalidate();

    protected unsafe override void OnPaint (in Rectangle _) {
        var size = ClientSize;
        Append(size.ToString());
        if (dib is null || dib.Width != size.X || dib.Height != size.Y)
            Resize();
        dib.ClearU32(Color.Black);

        for (var (i, y) = (0, 0); i < q.Count && y < size.Y; ++i, y += Font.Height)
            dib.DrawString(q[i], Font, 0, y, Color.Cyan);

        Blit(Dc, new(new(), size), dib);
    }

    private unsafe static void Blit (DeviceContext dc, in Rectangle rect, Dib dib) {
        if (rect.Width != dib.Width || rect.Height != dib.Height)
            throw new ArgumentOutOfRangeException(nameof(dib), "not same size");
        _ = Gdi32.StretchDIBits((IntPtr)dc, 0, 0, rect.Width, rect.Height, 0, 0, dib.Width, dib.Height, dib.Pixels, dib.Info, 0, 0xcc0020);
    }

    //protected override void OnEnterSizeMove () => Append(nameof(OnEnterSizeMove));
    //protected override void OnExitSizeMove () => Append(nameof(OnExitSizeMove));
    //protected override void OnSize (SizeType type, Vector2i size) => Append($"{nameof(OnSize)}: {type}, {size}");
    //protected override void OnSizing (SizingEdge edge, ref Rectangle r) => Append($"{nameof(OnSizing)}: {edge}, {r}");
    //protected override void OnWindowPosChanged (ref WindowPos p) => Append($"{nameof(OnWindowPosChanged)}: {p}");
    //protected override void OnWindowPosChanging (ref WindowPos p) => Append($"{nameof(OnWindowPosChanging)}: {p}");
}
