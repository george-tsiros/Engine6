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
    protected override void OnIdle () {
        Invalidate();
    }

    protected override void OnPaint (in Rectangle r) {
        Render();
        Gdi32.SwapBuffers(Dc);
        LastSync = Stopwatch.GetTimestamp();
        ++FramesRendered;
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
}
