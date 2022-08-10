namespace Gl;

using System;
using System.Diagnostics;
using Win32;
using Linear;

public class GlWindow:SimpleWindow {
    const PixelFlags RequiredFlags = PixelFlags.None
        | PixelFlags.DrawToWindow
        | PixelFlags.DoubleBuffer
        | PixelFlags.SupportOpengl
        //| PixelFlags.SwapExchange
        //| PixelFlags.SupportComposition
        ;

    const PixelFlags RejectedFlags = PixelFlags.None
        | PixelFlags.GenericAccelerated
        | PixelFlags.GenericFormat
        ;

    protected IntPtr DeviceContext;
    protected IntPtr RenderingContext;
    protected long FramesRendered { get; private set; }
    protected long LastSync { get; private set; }
    public GlWindow (Vector2i size) : base(size) {
        DeviceContext = User.GetDC(WindowHandle);
        RenderingContext = Opengl.CreateSimpleContext(DeviceContext, x => x.colorBits == 32 && x.depthBits >= 24 && (x.flags & RequiredFlags) == RequiredFlags && (x.flags & RejectedFlags) == 0);
        Demand(Opengl.MakeCurrent(DeviceContext, RenderingContext), "failed to make context current");
        LastSync = Stopwatch.GetTimestamp();
    }

    protected override void OnKeyUp (Keys k) {
        switch (k) {
            case Keys.Escape:
                User.PostQuitMessage(0);
                return;
        }
        base.OnKeyUp(k);
    }

    protected override void OnPaint () {
        Render();
        var swapOk = Gdi.SwapBuffers(DeviceContext);
        LastSync = Stopwatch.GetTimestamp();
        ++FramesRendered;
        Demand(swapOk);
    }

    protected virtual void Render () {
        Opengl.ClearColor(0.5f, 0.5f, 0.5f, 1f);
        Opengl.Clear(BufferBit.ColorDepth);
    }

    bool disposed = false;
    public override void Dispose (bool dispose) {
        if (dispose && !disposed) {
            disposed = true;
            Demand(Opengl.MakeCurrent(IntPtr.Zero, IntPtr.Zero));
            Demand(Opengl.DeleteContext(RenderingContext));
            Demand(User.ReleaseDC(WindowHandle, DeviceContext));
        }
    }
}
