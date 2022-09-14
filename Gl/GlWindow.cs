namespace Gl;

using Win32;
using static RenderingContext;
using System.Diagnostics;
using System;

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

    public GlWindow (ContextConfiguration? configuration = null) : base() {
        Create(Dc, configuration ?? ContextConfiguration.Default);
        User32.SetWindow(Handle, WindowStyle.Overlapped);
        SetSwapInterval(-1);
        StartTicks = Stopwatch.GetTimestamp();
        Idle += OnIdle;
    }

    int statCount = 0;
    string Stats () {
        var sum = 0l;
        for (var i = 0; i < StatFrameCount; ++i)
            sum += foo[i];
        var mean = (double)sum / StatFrameCount;
        var std = 0.0;
        for (var i = 0; i < StatFrameCount; ++i) {
            var x = foo[i] - mean;
            std += x * x;
        }
        ++statCount;
        if (statCount == 20) {
            var x = GetSwapInterval();
            var y = x == 1 ? -1 : x + 1;
            Debug.WriteLine($"swap {y}");
            SetSwapInterval(y);
            statCount = 0;
        }
        return Common.Maths.DoubleSqrt(std / StatFrameCount).ToString();
    }

    const int StatFrameCount = 200;
    private readonly long[] foo = new long[StatFrameCount];
    protected int index = 0;
    void OnIdle (object sender, EventArgs _) {
        if (swapPending) {
            if (LastSync + TframeTicks < Ticks()) {
                Gdi32.SwapBuffers(Dc);
                var t = Ticks();
                foo[index] = LastSync - t;
                if (++index == StatFrameCount) {
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
            Close();
            GC.SuppressFinalize(this);
            base.Dispose();
        }
    }
}
