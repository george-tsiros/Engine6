namespace Engine6;

using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Win32;


public class GdiWindow:Window {

    private const WindowPosFlags SelfMoveFlags = WindowPosFlags.NoSize | WindowPosFlags.NoSendChanging | WindowPosFlags.NoRedraw | WindowPosFlags.NoZOrder;
    private Vector2i lastCursorLocation = new(-1, -1);
    private Dib dib;

    public GdiWindow (Vector2i? size = null) : base(size) { }

    protected override void OnKeyUp (Key k) {
        switch (k) {
            case Key.Escape:
                User32.PostQuitMessage(0);
                return;
        }
        base.OnKeyUp(k);
    }

    protected override void OnButtonDown (MouseButton depressed) {
        switch (depressed) {
            case MouseButton.Right:
            lastCursorLocation = CursorLocation;
                return;
        }
        base.OnButtonDown(depressed);
    }

    bool changingWindowPosition;
    protected override void OnMouseMove (in Vector2i p) {
        var d = p - lastCursorLocation;
        if (!changingWindowPosition && Buttons.HasFlag(MouseButton.Right)) {
            changingWindowPosition = true;
            User32.SetWindowPos(nativeWindow.WindowHandle, IntPtr.Zero, Rect.Left + d.X, Rect.Top + d.Y, 0, 0, SelfMoveFlags);
            changingWindowPosition = false;
        }
    }

    protected override void OnLoad () {
        User32.SetWindow(nativeWindow.WindowHandle, WindowStyle.Overlapped);
        dib = new(Dc, Rect.Width, Rect.Height);
    }

    protected unsafe override void OnPaint () {
        if (dib is null)
            return;
        dib.ClearU32(Color.Black);
        var y = -Font.Height;
        dib.DrawString(Rect.ToString(), Font, 0, y += Font.Height, Color.Cyan);
        Blit(Dc, Rect, dib);
    }

    private unsafe static void Blit (DeviceContext dc, Rectangle rect, Dib dib) {
        if (rect.Width != dib.Width || rect.Height != dib.Height)
            throw new ArgumentOutOfRangeException(nameof(dib), "not same size");
        _ = Gdi32.StretchDIBits((IntPtr)dc, 0, 0, rect.Width, rect.Height, 0, 0, dib.Width, dib.Height, dib.Pixels, dib.Info, 0, 0xcc0020);
    }
}
