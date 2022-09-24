namespace Gl;

using Win32;
using System.Diagnostics;
using System;
using static Common.Maths;
using static GlContext;
using System.Collections.Generic;

public class GlWindow:Window {

    public GlWindow (ContextConfiguration? configuration = null, WindowStyle style = WindowStyle.Popup, WindowStyleEx styleEx = WindowStyleEx.None) : base(style, styleEx) {
        Ctx = new(Dc, configuration ?? ContextConfiguration.Default);
        timer = Stopwatch.StartNew();
    }

    protected virtual void Render (double dt_seconds) {
        ClearColor(0.5f, 0.5f, 0.5f, 1f);
        Clear(BufferBit.ColorDepth);
    }

    protected long Ticks () => timer.ElapsedTicks;
    protected long FramesRendered { get; private set; } = 0l;
    protected long LastSync { get; private set; } = 0l;
    protected GlContext Ctx;

    protected const double FPScap = 140;
    protected const double TframeSeconds = 1 / FPScap;
    protected static readonly double TicksPerSecond = Stopwatch.Frequency;
    protected static readonly double TframeTicks = TicksPerSecond * TframeSeconds;
    private readonly Stopwatch timer;
    private bool disposed = false;

    protected override void OnFocusChanged (in FocusChangedArgs e) {
        if (e.Focused)
            timer.Start();
        else
            timer.Stop();
        _ = User32.ShowCursor(!e.Focused);
        User32.RegisterMouseRaw(e.Focused ? this : null);
    }

    // we know base methods are empty
    protected override void OnIdle () {
        if (LastSync + TframeTicks < timer.ElapsedTicks) {
            var dt = 0.0;
            if (0 < FramesRendered) {
                Gdi32.SwapBuffers(Dc);
                var now = timer.ElapsedTicks;
                if (IsFocused)
                    dt = (now - LastSync) / TicksPerSecond;
                LastSync = now;
            }
            Render(dt);
            ++FramesRendered;
        }
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
