namespace Gl;

using System;
using System.Diagnostics;
using Win32;
using Common;

public class GlWindow:Window {
    private const PixelFlag RequiredFlags = PixelFlag.None
        | PixelFlag.DrawToWindow
        | PixelFlag.DoubleBuffer
        | PixelFlag.SupportOpengl
        //| PixelFlags.SwapExchange
        //| PixelFlags.SupportComposition
        ;
    private const PixelFlag RejectedFlags = PixelFlag.None
        | PixelFlag.GenericAccelerated
        | PixelFlag.GenericFormat
        ;

    protected IntPtr RenderingContext;
    protected long FramesRendered { get; private set; }
    protected long LastSync { get; private set; }
    public GlWindow (Vector2i? size = null) : base(size) {
        RenderingContext = Opengl.CreateSimpleContext(Dc, RequiredFlags, RejectedFlags);
        Opengl.MakeCurrent((IntPtr)Dc, RenderingContext);
        LastSync = Stopwatch.GetTimestamp();
        //_ = User32.ShowCursor(false);
    }

    protected override void OnKeyUp (Key k) {
        switch (k) {
            case Key.Escape:
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

    //protected override void OnIdle () {
    //    Invalidate();
    //}

    protected override void OnPaint (IntPtr dc, in Rectangle rect) {
        Render();
        Gdi32.SwapBuffers((IntPtr)Dc);
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
            Opengl.ReleaseCurrent((IntPtr)Dc);
            Opengl.DeleteContext(RenderingContext);
            Dispose();
        }
    }
}
