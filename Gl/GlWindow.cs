namespace Gl;

using Win32;
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

    protected readonly GlContext Ctx;
    public GlWindow (ContextConfiguration? configuration = null) : base(WindowStyle.Popup, WindowStyleEx.TopMost) {
        Ctx = new(Dc, configuration ?? ContextConfiguration.Default);
        StartTicks = Stopwatch.GetTimestamp();
        Idle += OnIdle;
        FocusChanged += OnFocusChanged;
    }

    private void OnFocusChanged (object sender, FocusChangedEventArgs e) {
        _ = User32.ShowCursor(!e.Focused);

        if (e.Focused)
            User32.RegisterMouseRaw(Handle);
        else
            User32.UnregisterMouseRaw();
    }

    void OnIdle (object sender, EventArgs _) {
        if (LastSync + 0.9 * TframeTicks < Ticks()) {
            if (0 < FramesRendered) {
                Gdi32.SwapBuffers(Dc);
                LastSync = Ticks();
            }
            Render();
            ++FramesRendered;
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
