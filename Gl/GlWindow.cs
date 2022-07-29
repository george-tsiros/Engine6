namespace Gl;

using System;
using System.Diagnostics;
using Win32;

public class GlWindow:SimpleWindow {
    const PixelFlags RequiredFlags = PixelFlags.None
        | PixelFlags.DrawToWindow
        | PixelFlags.DoubleBuffer
        | PixelFlags.SupportOpengl
        | PixelFlags.SwapExchange
        | PixelFlags.SupportComposition
        ;

    const PixelFlags RejectedFlags = PixelFlags.None
        | PixelFlags.GenericAccelerated
        | PixelFlags.GenericFormat
        ;

    const int FrameTimeCount = 255;
    protected IntPtr DeviceContext;
    protected IntPtr RenderingContext;
    protected long FramesRendered { get; private set; }
    //protected long RenderTicks { get; private set; }
    //protected readonly long[] FrameTicks = new long[FrameTimeCount];
    //protected int FrameIndex { get; private set; }
    //private long lastTicks = long.MaxValue;
    protected long LastSync { get; private set; }
    public GlWindow (Vector2i size) : base(size) {
        DeviceContext = User.GetDC(WindowHandle);
        RenderingContext = Opengl.CreateSimpleContext(DeviceContext, x => x.colorBits == 32 && x.depthBits >= 24 && (x.flags & RequiredFlags) == RequiredFlags && (x.flags & RejectedFlags) == 0);
        Demand(Opengl.MakeCurrent(DeviceContext, RenderingContext), "failed to make context current");
    }

    protected override void Paint () {
        //long t0 = Stopwatch.GetTimestamp();
        //var dticks = t0 - lastTicks;
        //lastTicks = t0;
        Render();
        //RenderTicks = Stopwatch.GetTimestamp() - t0;
        //State.Framebuffer = 0;
        //Opengl.Enable(Capability.ScissorTest);
        //Opengl.ClearColor(0.5f, 0.5f, 0.5f, 1);
        //Opengl.Scissor(0, 0, 100, 100);
        //Opengl.Clear(BufferBit.ColorDepth);
        //Opengl.Disable(Capability.ScissorTest);
        var swapOk = Gdi.SwapBuffers(DeviceContext);
        LastSync = Stopwatch.GetTimestamp();
        ++FramesRendered;
        Demand(swapOk);
        //FrameTicks[FrameIndex] = RenderTicks;
        //if (++FrameIndex == FrameTimeCount)
        //    FrameIndex = 0;
    }

    protected virtual void Render () {
        Opengl.ClearColor(0.5f, 0.5f, 0.5f, 1f);
        Opengl.Clear(BufferBit.ColorDepth);
    }

    public override void Run () {
        base.Run();
        Demand(Opengl.MakeCurrent(IntPtr.Zero, IntPtr.Zero));
        Demand(Opengl.DeleteContext(RenderingContext));
        Demand(User.ReleaseDC(WindowHandle, DeviceContext));
    }
}
