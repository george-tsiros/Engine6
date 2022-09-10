namespace Gl;

using System;
using System.Diagnostics;
using Win32;
using static Opengl;

public class GlWindow:Window {

    protected long Ticks () => Stopwatch.GetTimestamp() - StartTicks;
    protected nint RenderingContext = 0;
    protected long FramesRendered { get; private set; } = 0l;
    protected long LastSync { get; private set; } = 0l;
    private const double FPS = 60;
    private const double TframeSeconds = 1 / FPS;
    private const double TframeTicks = 1e7 * TframeSeconds;
    private readonly long StartTicks;
    private bool disposed = false;
    private bool swapPending = false;

    public GlWindow (ContextConfiguration? configuration = null) : base(WindowStyle.Maximize) {
        StartTicks = Stopwatch.GetTimestamp();
        RenderingContext = CreateSimpleContext(Dc, configuration ?? ContextConfiguration.Default);
        SetSwapInterval(-1);
    }

    protected override void OnKeyUp (Key k) {
        switch (k) {
            case Key.Escape:
                User32.PostQuitMessage(0);
                return;
        }
    }

    protected override void OnIdle () {
        if (swapPending) {
            // for, say, 72 Hz there's Tframe = 1 / 72 seconds between refreshes. 
            // we wait until 5% of that time remains before we swap.
            if (LastSync + 0.9 * TframeTicks < Ticks()) {
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
}