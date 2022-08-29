namespace Gl;

using System;
using System.Diagnostics;
using Common;
using Win32;
using static Opengl;

public class GlWindowArb:Window {

    protected IntPtr RenderingContext;
    protected long FramesRendered { get; private set; }
    protected long LastSync { get; private set; }

    public GlWindowArb (ContextConfigurationARB? configuration = null, Vector2i? size = null) : base(size) {
        RenderingContext = CreateContextARB(Dc, configuration ?? ContextConfigurationARB.Default);
        LastSync = Stopwatch.GetTimestamp();
    }

    protected override void OnKeyUp (Key k) {
        switch (k) {
            case Key.Escape:
                User32.PostQuitMessage(0);
                return;
        }
        base.OnKeyUp(k);
    }

    protected override void OnPaint () {
        Render();
        Gdi32.SwapBuffers(Dc);
        LastSync = Stopwatch.GetTimestamp();
        ++FramesRendered;
        Invalidate();
    }

    protected virtual void Render () {
        ClearColor(0.5f, 0.5f, 0.5f, 1f);
        Clear(BufferBit.ColorDepth);
    }

    private bool disposed = false;
    public override void Dispose (bool dispose) {
        if (dispose && !disposed) {
            disposed = true;
            ReleaseCurrent(Dc);
            DeleteContext(RenderingContext);
            Dispose();
        }
    }
    private Vector2i lastCursorLocation = new(-1, -1);
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
        const WindowPosFlags SelfMoveFlags = WindowPosFlags.NoSize | WindowPosFlags.NoSendChanging | WindowPosFlags.NoRedraw | WindowPosFlags.NoZOrder;
        var d = p - lastCursorLocation;
        if (!changingWindowPosition && Buttons.HasFlag(MouseButton.Right)) {
            changingWindowPosition = true;
            User32.SetWindowPos(nativeWindow.WindowHandle, IntPtr.Zero, Rect.Left + d.X, Rect.Top + d.Y, 0, 0, SelfMoveFlags);
            changingWindowPosition = false;
        }
    }
}
