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

    private const double FPScap = 100;
    private const double TframeSeconds = 1 / FPScap;
    private static readonly double TicksPerSecond = Stopwatch.Frequency;
    private static readonly double TframeTicks = TicksPerSecond * TframeSeconds;
    private readonly long StartTicks;
    private bool disposed = false;
    private bool swapPending = false;
    
    public GlWindow (ContextConfiguration? configuration = null) : base() {
        User32.SetWindow(Handle, WindowStyle.Overlapped);
        RenderingContext = CreateContext(Dc, configuration ?? ContextConfiguration.Default);
        SetSwapInterval(0);
        StartTicks = Stopwatch.GetTimestamp();
        Idle += OnIdle;
        //foreach (var e in SupportedExtensions)
        //    Debug.WriteLine(e);
    }
    string Stats () {
        var sum = 0l;
        for (var i = 0; i < 100; ++i)
            sum += foo[i];
        var mean = sum / 100.0;
        var std = 0.0;
        for (var i = 0; i < 100; ++i) {
            var x = foo[i] - mean;
            std += x * x; 
        }
        return Common.Maths.DoubleSqrt(std/100.0).ToString();
    }
    private readonly long[] foo = new long[100];
    protected int index=0;
    void OnIdle (object sender, EventArgs _) {
        if (swapPending) {
            if (LastSync + TframeTicks < Ticks()) {
                Gdi32.SwapBuffers(Dc);
                var t = Ticks();
                foo[index] = LastSync - t;
                if (++index == 100) {
                    Debug.WriteLine(Stats());
                    index = 0;
                }
                LastSync = t;
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
