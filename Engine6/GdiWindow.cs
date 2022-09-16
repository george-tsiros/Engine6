namespace Engine6;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Common;
using Win32;

public class GdiWindow:Window {

    private Dib dib;
    public GdiWindow () : base() {
        Resize();
        Load += OnLoad;
        KeyUp += OnKeyUp;
        Size += OnSize;
        Paint += OnPaint;
    }

    private void GdiWindow_KeyUp (object sender, KeyEventArgs e) {
        throw new NotImplementedException();
    }

    private void OnPaint (object sender, PaintEventArgs args) {
        var size = ClientSize;
        Append(size.ToString());
        if (dib is null || dib.Size != size)
            Resize();
        dib.ClearU32(Color.Black);

        for (var (i, y) = (0, 0); i < q.Count && y < size.Y; ++i, y += Font.Height)
            dib.DrawString(q[i], Font, 0, y, Color.Cyan);

        Blit(Dc, new(new(), size), dib);
    }

    private void OnSize (object sender, SizeEventArgs e) {
        Resize();
    }

    private void OnKeyUp (object sender, KeyEventArgs e) {
        switch (e.Key) {
            case Key.Escape:
                User32.PostQuitMessage(0);
                return;
        }
    }

    private void OnLoad (object sender, EventArgs e) {
        Resize();
    }

    private void Resize () {
        if (dib is not null && dib.Size != ClientSize) {
            dib.Dispose();
            dib = null;
        } 
        if (dib is null) {
            dib = new(Dc, ClientSize);
            User32.InvalidateWindow(Handle);
        }
    }

    private readonly List<string> q = new();

    private void Append (string str) {
        var maxLines = ClientSize.Y / Font.Height;
        var overflow = q.Count + 1 - maxLines;
        if (0 < overflow)
            q.RemoveRange(0, overflow);
        q.Add(str);
    }



    private unsafe static void Blit (DeviceContext dc, in Rectangle rect, Dib dib) {
        if (rect.Width != dib.Width || rect.Height != dib.Height)
            throw new ArgumentOutOfRangeException(nameof(dib), "not same size");
        _ = Gdi32.StretchDIBits((IntPtr)dc, 0, 0, rect.Width, rect.Height, 0, 0, dib.Width, dib.Height, dib.Pixels, dib.Info, 0, 0xcc0020);
    }

}
