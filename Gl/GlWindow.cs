namespace Gl;

using Win32;
//using static RenderingContext;
using System.Diagnostics;
using System;
using static GlContext;
public class GlWindow:Window {

    protected long Ticks () => Stopwatch.GetTimestamp() - StartTicks;
    protected long FramesRendered { get; private set; } = 0l;
    protected long LastSync { get; private set; } = 0l;

    private const double FPScap = 60;
    private const double TframeSeconds = 1 / FPScap;
    private static readonly double TicksPerSecond = Stopwatch.Frequency;
    private static readonly double TframeTicks = TicksPerSecond * TframeSeconds;
    private readonly long StartTicks;
    private bool disposed = false;
    private bool swapPending = false;
    protected readonly GlContext Ctx;
    public GlWindow (ContextConfiguration? configuration = null) : base() {
        Ctx = new(Dc, configuration ?? ContextConfiguration.Default);
        User32.SetWindow(Handle, WindowStyle.Overlapped);
        StartTicks = Stopwatch.GetTimestamp();
        Idle += OnIdle;
    }

    void OnIdle (object sender, EventArgs _) {
        if (swapPending) {
            if (LastSync + TframeTicks < Ticks()) {
                Gdi32.SwapBuffers(Dc);
                LastSync = Ticks();
                ++FramesRendered;
                swapPending = false;
            }
        } else {
            swapPending = true;
            Render();
        }
    }

    protected virtual void Render () {
        ClearColor(0.5f, 0.5f, 0.5f, 1f);
        Clear(BufferBit.ColorDepth);
    }

    public override void Dispose () {
        if (!disposed) {
            disposed = true;
            Ctx.Dispose();
            GC.SuppressFinalize(this);
            base.Dispose();
        }
    }
}
