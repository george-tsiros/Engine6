namespace Engine;

using System;
using System.Numerics;
using Shaders;
using System.Diagnostics;
using Gl;
using Win32;
using static Gl.Opengl;
using static Gl.Utilities;
using System.Threading;

internal class BlitTest:GlWindow {
    private Raster raster;
    private Sampler2D sampler;
    private VertexArray quad;
    private VertexBuffer<Vector4> quadBuffer;
    private bool leftDown;
    private Framebuffer fb;
    private Sampler2D fbsampler;
    private Renderbuffer rb;
    public Font Font { get; set; }
    private Model model;

    private static void Log (object ob) =>
#if DEBUG
        Debug
#else
        Console
#endif
        .WriteLine(ob);

    public BlitTest (Vector2i size, Model m = null) : base(size) {
        Font ??= new("ubuntu mono", 18f);
        Debug.Assert(Stopwatch.Frequency == 10_000_000);
        Text = "asdfg";
        const string TeapotFilepath = @"data\teapot.obj";
        model = m ?? new Model(TeapotFilepath);
        State.SwapInterval = 1;
        KeyDown += KeyDown_self;
        Load += Load_self;
        ButtonDown += ButtonDown_self;
        ButtonUp += ButtonUp_self;
    }

    void Load_self (object sender, EventArgs args) {
        rb = new(Size, RenderbufferFormat.Depth24Stencil8);

        fbsampler = new(Size, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest };

        fb = new();

        fb.Attach(rb, FramebufferAttachment.DepthStencil);

        fb.Attach(fbsampler, FramebufferAttachment.Color0);
        NamedFramebufferDrawBuffer(fb, DrawBuffer.Color0);

        quad = new();
        State.Program = PassThrough.Id;
        quadBuffer = new(Quad.Vertices);
        quad.Assign(quadBuffer, PassThrough.VertexPosition);
        raster = new(Size, 4, 1);

        sampler = new(Size, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest };
        Disposables.Add(raster);
        Disposables.Add(sampler);
        Disposables.Add(quad);
        Disposables.Add(quadBuffer);
        CursorVisible = false;
    }

    void ButtonDown_self (object sender, Buttons buttons) {
        if (buttons == Buttons.Left)
            leftDown = true;
    }
    void ButtonUp_self (object sender, Buttons buttons) {
        if (buttons == Buttons.Left)
            leftDown = false;
    }

    private static readonly string[] syncs = "free sink,no sync at all,vsync".Split(',');
    Random random = new();
    protected override void Render () {

        var textRow = -Font.Height;
        raster.ClearU32(Color.Black);
        raster.DrawString($"font height: {Font.Height} (EmSize {Font.EmSize})", Font, 0, textRow += Font.Height);
        raster.DrawString(syncs[1 + State.SwapInterval], Font, 0, textRow += Font.Height);
        raster.DrawString(CursorLocation.ToString(), Font, 0, textRow += Font.Height);
        //raster.LineU32(new(), new(100, 0), Color.White); // 0..99 , 100 pixels lit
        raster.TriangleU32(new(), new(10, 10), new(0, 10), Color.White);
        var (cx, cy) = CursorLocation;
        if (0 <= cx && cx < Width && 0 <= cy && cy < Height) {
            raster.LineU32(CursorLocation, CursorLocation + new Vector2i(9, 0), Color.Green); // 10 pixels lit
            raster.LineU32(CursorLocation, CursorLocation + new Vector2i(0, -9), Color.Green);
            var shadow = CursorLocation + new Vector2i(1, -1);
            raster.LineU32(shadow, shadow + new Vector2i(9, 0), Color.Black); // 10 pixels lit
            raster.LineU32(shadow, shadow + new Vector2i(0, -9), Color.Black);
        }

        sampler.Upload(raster);
        State.Framebuffer = fb;
        Viewport(0, 0, Width, Height);
        Clear(BufferBit.ColorDepth);
        State.Program = PassThrough.Id;
        State.VertexArray = quad;
        State.DepthFunc = DepthFunction.Always;
        State.DepthTest = true;
        State.CullFace = true;
        sampler.BindTo(1);
        PassThrough.Tex(1);
        DrawArrays(Primitive.Triangles, 0, 6);

        State.Framebuffer = 0;
        Viewport(0, 0, Width, Height);
        Clear(BufferBit.ColorDepth);

        fbsampler.BindTo(1);
        PassThrough.Tex(1);
        DrawArrays(Primitive.Triangles, 0, 6);
    }

    void AdjustFont (float delta) {
        var fh = Font.EmSize;
        var nh = float.Clamp(Font.EmSize + delta, 12, 36);
        if (fh == nh)
            return;
        Font = new(Font.FamilyName, nh);
    }

    void KeyDown_self (object sender, Keys k) {
        switch (k) {
            case Keys.Space:
                var s = State.SwapInterval + 1;
                if (s > 1)
                    s = -1;
                State.SwapInterval = s;
                return;
            case Keys.OemMinus:
                AdjustFont(-1f);
                return;
            case Keys.Oemplus:
                AdjustFont(+1f);
                return;
        }
    }
}
