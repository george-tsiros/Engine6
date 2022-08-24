namespace Gl;

using System;
using System.Diagnostics;
using Win32;
using Common;

public class GlWindow:Window {
    private const PixelFlags RequiredFlags = PixelFlags.None
        | PixelFlags.DrawToWindow
        | PixelFlags.DoubleBuffer
        | PixelFlags.SupportOpengl
        //| PixelFlags.SwapExchange
        //| PixelFlags.SupportComposition
        ;
    private const PixelFlags RejectedFlags = PixelFlags.None
        | PixelFlags.GenericAccelerated
        | PixelFlags.GenericFormat
        ;

    protected IntPtr RenderingContext;
    protected long FramesRendered { get; private set; }
    protected long LastSync { get; private set; }
    public GlWindow () {
        RenderingContext = Opengl.CreateSimpleContext(Dc, x => x.colorBits == 32 && x.depthBits >= 24 && (x.flags & RequiredFlags) == RequiredFlags && (x.flags & RejectedFlags) == 0);
        Opengl.MakeCurrent((IntPtr)Dc, RenderingContext);
        LastSync = Stopwatch.GetTimestamp();
        //_ = User32.ShowCursor(false);
    }

    protected override void OnKeyUp (Keys k) {
        switch (k) {
            case Keys.Escape:
                User32.PostQuitMessage(0);
                return;
        }
    }

    private bool skipMove;
    protected override void OnMouseMove (in Vector2i p) {
        if (!skipMove) {
            skipMove = true;
            var cs = Rect.Center;
            _ = User32.SetCursorPos(cs.X, cs.Y);
            base.OnMouseMove(new(p.X - cs.X, cs.Y - p.Y));
        } else
            skipMove = false;
    }

    protected override void OnIdle () {
        Invalidate();
    }

    protected override void OnPaint (IntPtr dc, in Rectangle rect) {
        Render();
        Gdi32.SwapBuffers((IntPtr)Dc);
        LastSync = Stopwatch.GetTimestamp();
        ++FramesRendered;
    }

    protected virtual void Render () {
        Opengl.ClearColor(0.5f, 0.5f, 0.5f, 1f);
        Opengl.Clear(BufferBit.ColorDepth);
    }

    private bool disposed = false;
    public override void Dispose (bool dispose) {
        if (dispose && !disposed) {
            disposed = true;
            Opengl.ReleaseCurrent((IntPtr)Dc);
            Opengl.DeleteContext(RenderingContext);
            Dispose();
        }
    }
}
