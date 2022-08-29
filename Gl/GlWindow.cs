namespace Gl;

using System;
using System.Diagnostics;
using Win32;
using Common;

public class GlWindow:Window {

    protected IntPtr RenderingContext;
    protected long FramesRendered { get; private set; }
    protected long LastSync { get; private set; }
    public GlWindow (Vector2i? size = null,ContextConfiguration? configuration = null) : base(size) {
        var config = configuration ?? ContextConfiguration.Default;
        RenderingContext = Opengl.CreateSimpleContext(Dc, config);
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
        Opengl.ClearColor(0.5f, 0.5f, 0.5f, 1f);
        Opengl.Clear(BufferBit.ColorDepth);
    }

    private bool disposed = false;
    public override void Dispose (bool dispose) {
        if (dispose && !disposed) {
            disposed = true;
            Opengl.ReleaseCurrent(Dc);
            Opengl.DeleteContext(RenderingContext);
            Dispose();
        }
    }
}
