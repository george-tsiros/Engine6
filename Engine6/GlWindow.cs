namespace Engine6;

using Win32;
using System.Diagnostics;
using System;
using Gl;
using static Gl.GlContext;
using System.Numerics;
using Shaders;
using System.Text;
using Common;
public class GlWindow:Window {

    unsafe public GlWindow (ContextConfiguration? configuration = null, WindowStyle style = WindowStyle.Popup, WindowStyleEx styleEx = WindowStyleEx.None) : base(style, styleEx) {
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
    public bool GuiActive { get; private set; } = true;
    private bool pause;

    private Tex tex;
    private VertexArray quadArray;
    private BufferObject<Vector2> quadBuffer;
    private Sampler2D guiSampler;
    private Raster guiRaster;
    static readonly BlendSourceFactor[] sourceFactors = Enum.GetValues<BlendSourceFactor>();
    static readonly BlendDestinationFactor[] destinationFactors = Enum.GetValues<BlendDestinationFactor>();

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
        if (GuiActive == active)
            return;
        GuiActive = active;
        _ = User32.ShowCursor(GuiActive);
        User32.RegisterMouseRaw(GuiActive ? null : this);
        if (!GuiActive) {
            var r = Rect.Center;
            User32.SetCursorPos(r.X, r.Y);
        }
    }

    protected override void OnFocusChanged () {
        if (IsFocused) {
            timer.Start();
        } else {
            timer.Stop();
            SetGuiActive(true);
        }
    }

    protected override void OnIdle () {
        if (LastSync + TframeTicks < timer.ElapsedTicks) {
            var dt = 0.0;
            if (0 < FramesRendered) {
                Gdi32.SwapBuffers(Dc);
                var now = timer.ElapsedTicks;
                if (IsFocused && !GuiActive)
                    dt = (now - LastSync) / TicksPerSecond;
                LastSync = now;
            }
            Render(dt);
            if (GuiActive) {
                if (tex is null)
                    Prepare();
                Viewport(new(), ClientSize);
                BindVertexArray(quadArray);
                UseProgram(tex);
                guiSampler.Upload(guiRaster);
                Disable(Capability.DepthTest);
                Enable(Capability.Blend);
                DrawArrays(Primitive.Triangles, 0, 6);
            }

            ++FramesRendered;
        }
    }
    static readonly byte[] title = { (byte)'t', (byte)'a', (byte)'b', (byte)' ', (byte)'g', (byte)'r', (byte)'a', (byte)'b', (byte)'s', (byte)'/', (byte)'r', (byte)'e', (byte)'l', (byte)'e', (byte)'a', (byte)'s', (byte)'e', (byte)'s', (byte)' ', (byte)'m', (byte)'o', (byte)'u', (byte)'s', (byte)'e', (byte)',', (byte)' ', (byte)'e', (byte)'s', (byte)'c', (byte)' ', (byte)'q', (byte)'u', (byte)'i', (byte)'t', (byte)'s', };

    private void Prepare () {
        tex = new();
        BindVertexArray(quadArray = new());
        var quad = new Vector2[] { new(-1, -1), new(1, -1), new(1, 1), new(-1, -1), new(1, 1), new(-1, 1) };
        quadArray.Assign(quadBuffer = new(quad), tex.VertexPosition);
        guiSampler = new(ClientSize, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest, Wrap = Wrap.ClampToEdge };
        guiSampler.BindTo(0);
        tex.Tex0(0);
        guiRaster = new(guiSampler.Size, 4, 1);
        guiRaster.ClearU32(Color.FromArgb(0x7f, 0x40, 0x40, 0x40));
        guiRaster.DrawString(title, PixelFont, 3, 3, ~0u, 0x8080807fu); 
                BlendFunc(BlendSourceFactor.One, BlendDestinationFactor.SrcColor);
        Disposables.Add(quadArray);
        Disposables.Add(quadBuffer);
        Disposables.Add(tex);
        Disposables.Add(guiSampler);
        Disposables.Add(guiRaster);
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
