namespace Gl;

using Win32;
using static Opengl;
using System.Diagnostics;
using System;

public class GlWindow:Window {

    protected long Ticks () => Stopwatch.GetTimestamp() - StartTicks;
    protected nint RenderingContext = 0;
    protected long FramesRendered { get; private set; } = 0l;
    protected long LastSync { get; private set; } = 0l;

    private const double FPScap = 140;
    private const double TframeSeconds = 1 / FPScap;
    private const double TframeTicks = 1e7 * TframeSeconds;
    private readonly long StartTicks;
    private bool disposed = false;
    private bool swapPending = false;

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
            ReleaseCurrent(Dc);
            DeleteContext(RenderingContext);
            GC.SuppressFinalize(this);
            base.Dispose();
        }
    }

    public GlWindow (ContextConfiguration? configuration = null) : base() {
        User32.SetWindow(Handle, WindowStyle.Overlapped);
        RenderingContext = CreateContext(Dc, configuration ?? ContextConfiguration.Default);
        SetSwapInterval(-1);
        StartTicks = Stopwatch.GetTimestamp();
        Idle += OnIdle;
    }
}
