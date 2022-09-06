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

    public GlWindowArb (ContextConfigurationARB? configuration = null) : base() {
        RenderingContext = CreateContextARB(Dc, configuration ?? ContextConfigurationARB.Default);
        LastSync = Stopwatch.GetTimestamp();
    }

    protected override void OnKeyUp (Key k) {
        switch (k) {
            case Key.Escape:
                User32.PostQuitMessage(0);
                return;
        }
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
    public override void Dispose () {
        if (!disposed) {
            disposed = true;
            ReleaseCurrent(Dc);
            DeleteContext(RenderingContext);
            base.Dispose();
        }
    }
    private Vector2i lastCursorLocation = new(-1, -1);
    protected override void OnButtonDown (MouseButton depressed, PointShort p) {
        switch (depressed) {
            case MouseButton.Right:
            lastCursorLocation = CursorLocation;
                return;
        }
    }
}
