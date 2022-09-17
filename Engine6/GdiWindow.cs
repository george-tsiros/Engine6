namespace Engine6;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Win32;

public class GdiWindow:Window {

    public GdiWindow () : base() {
        KeyUp += OnKeyUp;
        Paint += OnPaint;
        Size += OnSize;
    }

    private Dib dib;

    private readonly List<string> q = new();

    private long lastTicks = 0l;

    private void OnSize (object sender, SizeEventArgs e) {
        User32.InvalidateWindow(this);
    }

    private void OnPaint (object sender, PaintEventArgs args) {
        var t0 = Stopwatch.GetTimestamp();
        Resize();
        var size = ClientSize;
        Append(size.ToString());
        User32.SetWindowText(this, TimeSpan.FromTicks(lastTicks).ToString());
        dib.ClearU32(Color.Black);

        for (var (i, y) = (0, 0); i < q.Count && y < size.Y; ++i, y += Font.Height)
            dib.DrawString(q[i], Font, 0, y, Color.Cyan);

        Blit(Dc, new(new(), size), dib);
        lastTicks = Stopwatch.GetTimestamp() - t0;
    }

    private void OnKeyUp (object sender, KeyEventArgs e) {
        switch (e.Key) {
            case Key.Escape:
                User32.PostQuitMessage(0);
                return;
        }
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

    private void Append (string str) {
        var maxLines = ClientSize.Y / Font.Height;
        var overflow = q.Count + 1 - maxLines;
        if (0 < overflow)
            q.RemoveRange(0, overflow);
        q.Add(str);
    }

    private unsafe static void Blit (DeviceContext dc, in Rectangle rect, Dib dib) {
        Debug.Assert(rect.Size == dib.Size);
        _ = Gdi32.StretchDIBits(dc, rect, new(new(), dib.Size), dib, RasterOperation.SrcCopy);
    }
}
