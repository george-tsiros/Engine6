namespace Gl;

using System;
using System.Diagnostics;
using Win32;

public class GlWindow:SimpleWindow {
    const PixelFlags PfdFlags = PixelFlags.DoubleBuffer | PixelFlags.DrawToWindow | PixelFlags.SupportOpengl | PixelFlags.SwapCopy | PixelFlags.SupportComposition;

    protected IntPtr DeviceContext;
    protected IntPtr RenderingContext;
    protected long FramesRendered { get; private set; }
    protected long RenderTicks { get; private set; }
    protected readonly long[] FrameTicks = new long[100];
    private frameIndex = 0;
    private long lastTicks = long.MaxValue;

    public GlWindow (Vector2i size) : base(size) {
        DeviceContext = User.GetDC(WindowHandle);
        RenderingContext = Opengl.CreateSimpleContext(DeviceContext, x => x.colorBits == 32 && x.depthBits == 24 && x.flags == PfdFlags);
        Demand(Opengl.MakeCurrent(DeviceContext, RenderingContext), "failed to make context current");
    }

    protected override void Paint () {
        long t0 = Stopwatch.GetTimestamp();
        var dticks = t0 - lastTicks;
        lastTicks = t0;
        var dt = dticks > 0 ? (float)((double)dticks / Stopwatch.Frequency) : 0f;
        Render(dt);
        RenderTicks = Stopwatch.GetTimestamp() - t0;
        Demand(Gdi.SwapBuffers(DeviceContext));
        ++FramesRendered;
        FrameTicks[frameIndex] = RenderTicks;
        if (++frameIndex == FrameTicks.Length)
            frameIndex = 0;
    }

    protected virtual void Render (float dt) {
        Opengl.ClearColor(0.5f, 0.5f, 0.5f, 1f);
        Opengl.Clear(BufferBit.Color | BufferBit.Depth);
    }

    public override void Run () {
        base.Run();
        Demand(Opengl.MakeCurrent(IntPtr.Zero, IntPtr.Zero));
        Demand(Opengl.DeleteContext(RenderingContext));
        Demand(User.ReleaseDC(WindowHandle, DeviceContext));
    }
}
