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

    public GlWindow (ContextConfiguration? configuration = null, WindowStyle style = WindowStyle.Popup, WindowStyleEx styleEx = WindowStyleEx.None) : base(style, styleEx) {
        Ctx = new(Dc, configuration ?? ContextConfiguration.Default);
        SimulationTime = Stopwatch.StartNew();
        WallTime = Stopwatch.StartNew();
        ClientSize = DefaultWindowedSize;
        backupWindowStyleEx = User32.GetWindowStyleEx(this);

        TotalTicksSinceLastRead = new long[AxisKeys.Length];
        LastPressTimestamp = new long[AxisKeys.Length];

        Reusables.Add(presentation = new());
        Reusables.Add(presentationVertices = new(PresentationQuad));
        Reusables.Add(quadArray = new());
    }

    protected virtual void Render () {
        ClearColor(0.5f, 0.5f, 0.5f, 1f);
        Clear(BufferBit.ColorDepth);
    }

    protected GlContext Ctx;
    protected long FramesRendered { get; private set; } = 0l;
    protected long LastSync { get; private set; }
    protected double TicksPerFrame () => TicksPerSecond * SecondsPerFrame();
    protected bool GuiActive { get; private set; } = true;
    protected static readonly Vector2[] PresentationQuad = { new(-1, -1), new(1, -1), new(1, 1), new(-1, -1), new(1, 1), new(-1, 1), };
    protected virtual Key[] AxisKeys { get; } = Array.Empty<Key>();
    protected double LastFramesInterval { get; private set; }
    protected readonly Stopwatch WallTime;
    protected readonly Stopwatch SimulationTime;
    private double FramesPerSecond = 60;
    private double SecondsPerFrame () => 1 / FramesPerSecond;
    private static readonly long TicksPerSecond = Stopwatch.Frequency;
    private readonly long[] TotalTicksSinceLastRead;
    private readonly long[] LastPressTimestamp;

    private byte[] title = helpText;
    private static readonly byte[] helpText = Encoding.ASCII.GetBytes("tab grabs/releases cursor alt-enter toggles fullscreen, esc quits");
    private static readonly byte[] toggleFailedText = Encoding.ASCII.GetBytes("failed to switch to fullscreen");
    private bool disposed = false;
    private readonly Presentation presentation;
    private readonly VertexArray quadArray;
    private readonly BufferObject<Vector2> presentationVertices;
    private Sampler2D guiSampler;
    private Raster guiRaster;
    private static readonly Vector2i DefaultWindowedSize = new(800, 600);
    private bool fullscreen = false;
    private readonly WindowStyleEx backupWindowStyleEx;
    private Rectangle windowedRectangle;

    protected override void OnLoad () {
        quadArray.Assign(presentationVertices, presentation.VertexPosition);
        guiSampler = new(ClientSize, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest, Wrap = Wrap.ClampToEdge };
        guiRaster = new(guiSampler.Size, 4, 1);
        BlendFunc(BlendSourceFactor.One, BlendDestinationFactor.SrcColor);
        Disposables.Add(guiSampler);
        Disposables.Add(guiRaster);
        SetGuiActive(false);
    }

    protected override void OnIdle () {
        if (0 == LastSync || LastSync + (long)(0.99 * TicksPerFrame()) < WallTime.ElapsedTicks) {
            if (0 < FramesRendered) {
                Gdi32.SwapBuffers(Dc);
                var now = WallTime.ElapsedTicks;
                LastFramesInterval = (now - LastSync) / (double)TicksPerSecond;
                LastSync = now;
            }
            Render();
            if (GuiActive)
                RenderGui();
            ++FramesRendered;
        }
    }

    protected override void OnKeyDown (Key key, bool repeat) {
        var now = SimulationTime.ElapsedTicks;
        if (!repeat)
            switch (key) {
                case Key.Return:
                    if (IsKeyDown(Key.Menu)) {
                        ToggleFullscreen();
                        return;
                    }
                    break;
                case Key.Tab:
                    SetGuiActive(!GuiActive);
                    return;
                case Key.Escape:
                    User32.PostQuitMessage(0);
                    return;
            }
        if (!repeat && IsAxis(key, now, true))
            return;
        base.OnKeyDown(key, repeat);
    }

    protected override void OnKeyUp (Key key) {
        if (IsAxis(key, SimulationTime.ElapsedTicks, false))
            return;
        base.OnKeyUp(key);
    }

    protected override void OnFocusChanged () {
        if (IsFocused) {
            SimulationTime.Start();
        } else {
            SimulationTime.Stop();
            SetGuiActive(true);
        }
    }

    private void ToggleFullscreen () {
        foreach (var x in Disposables)
            x.Dispose();
        Disposables.Clear();

        if (fullscreen) {
            _ = User32.SetWindowStyleEx(this, backupWindowStyleEx);
            User32.SetWindowPos(this, WindowPosFlags.None);
            User32.MoveWindow(this, windowedRectangle);
            FramesPerSecond = 60;
            fullscreen = false;
            title = Encoding.ASCII.GetBytes("windowed, 60 Hz");
        } else {
            windowedRectangle = GetWindowRectangle();

            var monitors = User32.GetMonitorInfo();
            var (maxIndex, maxArea) = (-1, 0);
            for (var i = 0; i < monitors.Length; ++i) {
                var m = monitors[i].entireDisplay;
                var area = m.Width * m.Height;
                if (maxArea < area)
                    (maxIndex, maxArea) = (i, area);
            }
            Debug.Assert(0 <= maxIndex);
            var monitor = monitors[maxIndex];
            if (User32.EnumDisplaySettings(monitor, out var monitorInfo)) {
                User32.MoveWindow(this, monitor.entireDisplay);
                FramesPerSecond = monitorInfo.dmDisplayFrequency;
                title = Encoding.ASCII.GetBytes($"fullscreen, {monitorInfo.dmDisplayFrequency} Hz");
                fullscreen = true;
            } else {
                title = toggleFailedText;
                SetGuiActive(true);
            }
        }
        OnLoad();
    }

    private bool IsAxis (Key key, long now, bool depressed) {
        var i = Array.IndexOf(AxisKeys, key);
        if (i < 0)
            return false;
        if (depressed) {
            Debug.Assert(0 == LastPressTimestamp[i]);
            LastPressTimestamp[i] = now;
        } else {
            TotalTicksSinceLastRead[i] += now - LastPressTimestamp[i];
            LastPressTimestamp[i] = 0;
        }
        return true;
    }

    private long Pop (Key key) {
        var ticks = SimulationTime.ElapsedTicks;
        var i = Array.IndexOf(AxisKeys, key);
        Debug.Assert(0 <= i);
        var total = TotalTicksSinceLastRead[i];
        var t = LastPressTimestamp[i];
        if (0 < t) {
            // simulate button released and pressed the same instant
            total += ticks - t;
            LastPressTimestamp[i] = ticks;
        }
        TotalTicksSinceLastRead[i] = 0;
        return total;
    }

    protected float Axis (Key positive, Key negative) =>
        (float)((Pop(positive) - Pop(negative)) / (double)TicksPerSecond);

    private void SetGuiActive (bool active) {
        if (GuiActive != active) {
            GuiActive = active;
            _ = User32.ShowCursor(GuiActive);
            User32.RegisterMouseRaw(GuiActive ? null : this);
            if (!GuiActive)
                User32.SetCursorPos(GetWindowRectangle().Center);
        }
    }

    private void RenderGui () {
        guiRaster.ClearU32(Color.FromArgb(0x7f, 0x40, 0x40, 0x40));
        using Ascii t = new(FramesRendered.ToString());
        guiRaster.DrawString(t.AsSpan(), PixelFont, 3, 3, ~0u, 0x8080807fu);
        guiSampler.Upload(guiRaster);

        BindDefaultFramebuffer(FramebufferTarget.Draw);
        Disable(Capability.DepthTest);
        Enable(Capability.Blend);
        Viewport(in Vector2i.Zero, ClientSize);

        BindVertexArray(quadArray);
        UseProgram(presentation);

        guiSampler.BindTo(10);
        presentation.Tex0(10);

        DrawArrays(Primitive.Triangles, 0, 6);

        Disable(Capability.Blend);
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
