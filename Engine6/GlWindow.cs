namespace Engine6;

using Win32;
using System.Diagnostics;
using System;
using Gl;
using static Gl.GlContext;
using System.Numerics;
using Shaders;

public class GlWindow:Window {

    public GlWindow (ContextConfiguration? configuration = null, WindowStyle style = WindowStyle.Popup, WindowStyleEx styleEx = WindowStyleEx.None) : base(style, styleEx) {
        Ctx = new(Dc, configuration ?? ContextConfiguration.Default);
        timer = Stopwatch.StartNew();
        tex = new();
        BindVertexArray(quadArray = new());
        var quad = new Vector2[] { new(-1, -1), new(1, -1), new(1, 1), new(-1, -1), new(1, 1), new(-1, 1) };
        quadArray.Assign(quadBuffer = new(quad), tex.VertexPosition);
        guiSampler = new(ClientSize, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest, Wrap = Wrap.ClampToEdge };
        guiSampler.BindTo(0);
        tex.Tex0(0);
        guiRaster = new(guiSampler.Size, 4, 1);
        guiRaster.ClearU32(Color.Transparent);
        guiRaster.FillRectU32(new(new(), new(100, 100)), ~0u);
        Disposables.Add(quadArray);
        Disposables.Add(quadBuffer);
        Disposables.Add(tex);
        Disposables.Add(guiSampler);
        Disposables.Add(guiRaster);

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
    public bool GuiActive { get; private set; } = true;
    private bool pause;

    private Tex tex;
    private VertexArray quadArray;
    private BufferObject<Vector2> quadBuffer;
    private Sampler2D guiSampler;
    private Raster guiRaster;


    protected override void OnKeyDown (Key key, bool repeat) {
        switch (key) {
            case Key.Pause:
                pause = !pause;
                return;
            case Key.Tab:
                SetGuiActive(!GuiActive);
                return;
            case Key.Escape:
                User32.PostQuitMessage(0);
                return;
        }
        base.OnKeyDown(key, repeat);
    }

    private void SetGuiActive (bool active) {
        if (GuiActive != active) {
            Debug.WriteLine($"was {GuiActive}, changing to {active} ");
            GuiActive = active;
            _ = User32.ShowCursor(GuiActive);
            User32.RegisterMouseRaw(GuiActive ? null : this);
            if (!GuiActive) {
                var r = Rect.Center;
                User32.SetCursorPos(r.X, r.Y);
            }
        } else {
            Debug.WriteLine($"already {GuiActive} == {active}");
        }
    }

    protected override void OnFocusChanged () {
        Debug.WriteLine(DateTime.Now.ToString("ss.fff"));
        if (IsFocused) {
            timer.Start();
        } else {
            timer.Stop();
            SetGuiActive(true);
        }
    }

    //  focused rawInput    event       >   focused rawInput    what i should do
    //  1       1           focus loss      0       0           disable raw, show cursor
    //  1       1           tab pressed     1       0           disable raw, show cursor
    //  1       0           tab pressed     1       1           enable raw, hide cursor

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
            if (GuiActive) {
                Viewport(new(), ClientSize);
                BindVertexArray(quadArray);
                UseProgram(tex);
                guiSampler.Upload(guiRaster);
                Disable(Capability.DepthTest);
                DrawArrays(Primitive.Triangles, 0, 6);
            }

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
