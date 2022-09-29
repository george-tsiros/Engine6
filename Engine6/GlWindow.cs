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
        timer = Stopwatch.StartNew();
        ClientSize = DefaultWindowedSize;
        backupWindowStyleEx = User32.GetWindowStyleEx(this);

        Recyclables.Add(presentation = new());
        Recyclables.Add(presentationVertices = new(PresentationQuad));
        Recyclables.Add(quadArray = new());
    }

    protected virtual void Render (double dt_seconds) {
        ClearColor(0.5f, 0.5f, 0.5f, 1f);
        Clear(BufferBit.ColorDepth);
    }

    protected GlContext Ctx;
    protected long Ticks () => timer.ElapsedTicks;
    protected long FramesRendered { get; private set; } = 0l;
    protected long LastSync { get; private set; } = 0l;
    protected double TframeTicks () => TicksPerSecond * TframeSeconds();
    protected bool GuiActive { get; private set; } = true;

    private double FPScap = 60;
    private double TframeSeconds () => 1 / FPScap;
    private static readonly double TicksPerSecond = Stopwatch.Frequency;
    private readonly Stopwatch timer;
    private bool disposed = false;
    private Presentation presentation;
    private VertexArray quadArray;
    private BufferObject<Vector2> presentationVertices;
    private Sampler2D guiSampler;
    private Raster guiRaster;
    private static readonly Vector2i DefaultWindowedSize = new(800, 600);
    private bool fullscreen = false;
    private WindowStyleEx backupWindowStyleEx;
    private Rectangle windowedRectangle;

    private void ToggleFullscreen () {
        foreach (var x in Disposables)
            x.Dispose();

        if (fullscreen) {
            _ = User32.SetWindowStyleEx(this, backupWindowStyleEx);
            User32.SetWindowPos(this, WindowPosFlags.None);
            User32.MoveWindow(this, windowedRectangle);
            FPScap = 60;
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
                FPScap = monitorInfo.dmDisplayFrequency;
                title = Encoding.ASCII.GetBytes($"fullscreen, {monitorInfo.dmDisplayFrequency} Hz");
                fullscreen = true;
            } else {
                title = toggleFailedText;
                SetGuiActive(true);
            }
        }
        OnLoad();
    }

    protected override void OnKeyDown (Key key, bool repeat) {
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
        base.OnKeyDown(key, repeat);
    }

    private void SetGuiActive (bool active) {
        if (GuiActive == active)
            return;
        GuiActive = active;
        _ = User32.ShowCursor(GuiActive);
        User32.RegisterMouseRaw(GuiActive ? null : this);
        if (!GuiActive) {
            var r = GetWindowRectangle().Center;
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
        if (LastSync + TframeTicks() < timer.ElapsedTicks) {
            var dt = 0.0;
            if (0 < FramesRendered) {
                Gdi32.SwapBuffers(Dc);
                var now = timer.ElapsedTicks;
                if (IsFocused && !GuiActive) {
                    dt = (now - LastSync) / TicksPerSecond;
                    dt = dt * dt / TframeSeconds();
                }
                LastSync = now;
            }
            Render(dt);
            if (GuiActive) {
                guiRaster.ClearU32(Color.FromArgb(0x7f, 0x40, 0x40, 0x40));
                guiRaster.DrawString(title, PixelFont, 3, 3, ~0u, 0x8080807fu);
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

            ++FramesRendered;
        }
    }
    private byte[] title = helpText;
    private static readonly byte[] helpText = Encoding.ASCII.GetBytes("tab grabs/releases cursor alt-enter toggles fullscreen, esc quits");
    private static readonly byte[] toggleFailedText = Encoding.ASCII.GetBytes("failed to switch to fullscreen");
    private static readonly byte[] windowedText = Encoding.ASCII.GetBytes("windowed");
    private static readonly byte[] fullscreenText = Encoding.ASCII.GetBytes("fullscreen");
    protected static readonly Vector2[] PresentationQuad = { new(-1, -1), new(1, -1), new(1, 1), new(-1, -1), new(1, 1), new(-1, 1), };

    protected override void OnLoad () {
        base.OnLoad();
        quadArray.Assign(presentationVertices, presentation.VertexPosition);
        guiSampler = new(ClientSize, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest, Wrap = Wrap.ClampToEdge };
        guiRaster = new(guiSampler.Size, 4, 1);
        BlendFunc(BlendSourceFactor.One, BlendDestinationFactor.SrcColor);
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
