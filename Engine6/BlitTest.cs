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
    private bool leftIsDown;
    private Framebuffer fb;
    private Sampler2D fbsampler;
    private Renderbuffer rb;
    private Model Obj;
    private Matrix4d Projection;
    private Vector3d[] ClipSpace;
    private Vector2i[] ScreenSpace;
    private Vector3d[] ModelSpace;
    private double[] FaceZ;
    private (double depth, int index)[] FacesAndDots;
    Vector3d cameraPosition = new(10, 0, 30);
    string txt = "asdgf";

    Vector2i lastCursorPosition = new(-1, -1);
    Vector3d modelPosition = new();
    double phi = 0, theta = 0;
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
        Obj = m ?? new Model(TeapotFilepath);
        Projection = Matrix4d.Project(double.Pi / 4.0, (double)Width / Height, 0.1, 100.0);
        ClipSpace = new Vector3d[Obj.Vertices.Count];
        ScreenSpace = new Vector2i[Obj.Vertices.Count];
        ModelSpace = new Vector3d[Obj.Vertices.Count];
        FaceZ = new double[Obj.Faces.Count];
        FacesAndDots = new (double, int)[Obj.Faces.Count];

        State.SwapInterval = 1;
        KeyDown += KeyDown_self;
        Load += Load_self;
        //ButtonDown += ButtonDown_self;
        //ButtonUp += ButtonUp_self;
        MouseMove += MouseMove_self;
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

    private void MouseMove_self (object sender, Vector2i e) {
        var d = e - lastCursorPosition;
        lastCursorPosition = e;
        switch (Buttons) {
            case Buttons.Left:
                theta = Extra.ModuloTwoPi(theta, 0.01 * d.X);
                phi = double.Clamp(phi + 0.01 * d.Y, -double.Pi / 2, double.Pi / 2);
                break;
            case Buttons.Right:
                modelPosition += new Vector3d(d.X, d.Y, 0);
                break;
        }
    }

    //void ButtonDown_self (object sender, Buttons buttons) {
    //        lastCursorPosition = CursorLocation;
    //}
    //void ButtonUp_self (object sender, Buttons buttons) {
    //    if (buttons == Buttons.Left)
    //        leftIsDown = false;
    //}

    private static readonly string[] syncs = "free sink,no sync at all,vsync".Split(',');
    protected override void Render () {

        var textRow = -Font.Height;
        raster.ClearU32(Color.Black);
        var faceCount = Obj.Faces.Count;
        var vertexCount = Obj.Vertices.Count;
        var rotation = Matrix4d.RotationX(phi) * Matrix4d.RotationY(theta);
        var translation = Matrix4d.Translate(modelPosition);
        for (var i = 0; i < vertexCount; ++i) {
            var vertex = new Vector4d(Obj.Vertices[i], 1);
            var modelSpace = vertex * rotation * translation;
            ModelSpace[i] = modelSpace.Xyz();
            var position = modelSpace * Matrix4d.Translate(-cameraPosition);
            var projected = position * Projection;
            var n = new Vector3d(-projected.X / projected.W, -projected.Y / projected.W, -projected.Z / projected.W);
            ClipSpace[i] = n;
            ScreenSpace[i] = NormalizedToScreen(n, Size);
        }
        var drawn = 0;
        var light = Vector3d.Normalize(new(0, 1, 1));
        for (var i = 0; i < faceCount; ++i) {
            var (a, b, c) = Obj.Faces[i];
            var na = ClipSpace[a];
            if (!IsInside(na))
                continue;
            var nb = ClipSpace[b];
            if (!IsInside(nb))
                continue;
            var nc = ClipSpace[c];
            if (!IsInside(nc))
                continue;
            if (Vector3d.Cross(nb - na, nc - nb).Z < 0)
                continue;
            var sb = ModelSpace[b];
            var cross = Vector3d.Cross(sb - ModelSpace[a], ModelSpace[c] - sb);

            FaceZ[drawn] = -na.Z - nb.Z - nc.Z;
            FacesAndDots[drawn] = ((float)Vector3d.Dot(Vector3d.Normalize(cross), light), i);
            ++drawn;
        }
        var t0 = Stopwatch.GetTimestamp();
        if (drawn > 0) {
            if (drawn > 1)
                Array.Sort(FaceZ, FacesAndDots, 0, drawn);

            for (var i = 0; i < drawn; ++i) {
                var (d, idx) = FacesAndDots[i];
                var intensity = int.Clamp((int)Math.Floor(d * 256), byte.MinValue, byte.MaxValue);
                var (a, b, c) = Obj.Faces[idx];
                raster.TriangleU32(ScreenSpace[a], ScreenSpace[b], ScreenSpace[c], Color.FromRgb(intensity, intensity, intensity));
            }
        }
        var t1 = Stopwatch.GetTimestamp();
        raster.DrawString($"font height: {Font.Height} (EmSize {Font.EmSize})", Font, 0, textRow += Font.Height);
        raster.DrawString(syncs[1 + State.SwapInterval], Font, 0, textRow += Font.Height);
        raster.DrawString(modelPosition.ToString(), Font, 0, textRow += Font.Height);
        raster.DrawString(txt, Font, 0, textRow += Font.Height);
        txt = ((double)(t1 - t0) / Stopwatch.Frequency).ToEng();
        var (cx, cy) = CursorLocation;
        if (0 <= cx && cx < Width && 0 <= cy && cy < Height) {
            raster.LineU32(CursorLocation, CursorLocation + new Vector2i(9, 0), Color.Green); // 10 pixels lit
            raster.LineU32(CursorLocation, CursorLocation + new Vector2i(0, -9), Color.Green);
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

    public static Vector2i NormalizedToScreen (in Vector3d n, in Vector2i size) =>
        new((int)((n.X + 1) * 0.5 * size.X), (int)((n.Y + 1) * 0.5 * size.Y));

    public static bool IsInside (in Vector3d v) =>
        -1 < v.Z && v.Z < 1;

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
