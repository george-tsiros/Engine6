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
using static Linear.Maths;
using Linear;


internal class BlitTest:GlWindowArb {
    static readonly string[] syncs = "free sink,no sync at all,vsync".Split(',');
    static void Log (object ob) =>
#if DEBUG
        Debug
#else
        Console
#endif
        .WriteLine(ob);

    static readonly Vector4[] QuadVertices = {
        new(-1f, -1f, 0, 1),
        new(+1f, -1f, 0, 1),
        new(+1f, +1f, 0, 1),
        new(-1f, -1f, 0, 1),
        new(+1f, +1f, 0, 1),
        new(-1f, +1f, 0, 1),
    };

    static Vector2i NormalizedToScreen (in Vector3d n, in Vector2i size) =>
        new((int)((n.X + 1) * 0.5 * size.X), (int)((n.Y + 1) * 0.5 * size.Y));

    static bool IsInside (in Vector3d v) =>
        -1 < v.Z && v.Z < 1;

    Model Obj;
    Vector3d cameraPosition = new(0, 0, 30);
    bool useOpenGl = true;
    Vector2i lastCursorPosition = new(-1, -1);
    Vector3d modelPosition = new();
    double phi = 0, theta = 0;
    readonly Vector3d lightDirection = Vector3d.Normalize(new(0, 0, -1));

    Raster softwareRenderSurface;
    Sampler2D softwareRenderTexture;
    VertexArray quad;
    Framebuffer offscreenFramebuffer;
    Sampler2D offscreenRenderingSurface;
    Renderbuffer offscreenDepthbuffer;
    Matrix4d Projection;
    Vector3d[] ClipSpace;
    Vector2i[] ScreenSpace;
    Vector3d[] ModelSpace;
    double[] FaceZ;
    (double depth, int index)[] FacesAndDots;

    VertexArray vao;
    VertexBuffer<Vector4> quadBuffer;
    VertexBuffer<Vector4> vertexBuffer;
    VertexBuffer<Vector4> normalBuffer;

    public BlitTest (Vector2i size, Model m = null) : base(size) {
        Font ??= new("data/IBM_3270.txt");
        Debug.Assert(Stopwatch.Frequency == 10_000_000);
        Text = "asdfg";
        const string TeapotFilepath = @"data\teapot.obj";
        Obj = m ?? new Model(TeapotFilepath);
        Projection = Matrix4d.Project(dPi / 4.0, (double)Width / Height, 0.1, 100.0);
        ClipSpace = new Vector3d[Obj.Vertices.Count];
        ScreenSpace = new Vector2i[Obj.Vertices.Count];
        ModelSpace = new Vector3d[Obj.Vertices.Count];
        FaceZ = new double[Obj.Faces.Count];
        FacesAndDots = new (double, int)[Obj.Faces.Count];

        State.SwapInterval = 1;
        KeyDown += KeyDown_self;
        Load += Load_self;
        MouseMove += MouseMove_self;
    }

    void Load_self (object sender, EventArgs args) {
        offscreenDepthbuffer = new(Size, RenderbufferFormat.Depth24Stencil8);
        offscreenRenderingSurface = new(Size, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest };
        offscreenFramebuffer = new();
        offscreenFramebuffer.Attach(offscreenDepthbuffer, FramebufferAttachment.DepthStencil);
        offscreenFramebuffer.Attach(offscreenRenderingSurface, FramebufferAttachment.Color0);
        NamedFramebufferDrawBuffer(offscreenFramebuffer, DrawBuffer.Color0);
        quad = new();
        State.Program = PassThrough.Id;
        quadBuffer = new(QuadVertices);
        quad.Assign(quadBuffer, PassThrough.VertexPosition);
        softwareRenderSurface = new(Size, 4, 1);
        softwareRenderTexture = new(Size, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest };

        State.Program = DirectionalFlat.Id;
        var faceCount = Obj.Faces.Count;
        var vertexCount = faceCount * 3;
        var vertices = new Vector4[vertexCount];
        var normals = new Vector4[vertexCount];
        for (var (i, j) = (0, 0); j < vertexCount; ++i, ++j) {
            var (a, b, c) = Obj.Faces[i];
            var (v0, v1, v2) = (Obj.Vertices[a], Obj.Vertices[b], Obj.Vertices[c]);
            normals[j + 2] = normals[j + 1] = normals[j] = new((Vector3)Vector3d.Normalize(Vector3d.Cross(v1 - v0, v2 - v0)), 0);
            vertices[j] = new((Vector3)v0, 1);
            vertices[++j] = new((Vector3)v1, 1);
            vertices[++j] = new((Vector3)v2, 1);
        }

        vertexBuffer = new(vertices);
        normalBuffer = new(normals);
        vao = new();
        vao.Assign(vertexBuffer, DirectionalFlat.VertexPosition);
        vao.Assign(normalBuffer, DirectionalFlat.FaceNormal);
        Disposables.Add(softwareRenderSurface);
        Disposables.Add(softwareRenderTexture);
        Disposables.Add(quad);
        Disposables.Add(quadBuffer);
        Disposables.Add(normalBuffer);
        Disposables.Add(vertexBuffer);
        CursorVisible = false;
    }

    protected override void Render () {
        var t0 = Stopwatch.GetTimestamp();
        State.Framebuffer = offscreenFramebuffer;
        Viewport(0, 0, Width, Height);
        Clear(BufferBit.ColorDepth);

        if (useOpenGl)
            RenderHardware();
        else
            RenderSoftware();
        State.Framebuffer = 0;
        State.VertexArray = quad;
        State.Program = PassThrough.Id;
        Viewport(0, 0, Width, Height);
        Clear(BufferBit.ColorDepth);

        offscreenRenderingSurface.BindTo(1);
        PassThrough.Tex(1);
        DrawArrays(Primitive.Triangles, 0, 6);
        var t1 = Stopwatch.GetTimestamp();
    }

    void RenderHardware () {

        State.Program = DirectionalFlat.Id;
        State.VertexArray = vao;
        State.DepthTest = true;
        State.DepthFunc = DepthFunction.LessEqual;
        State.CullFace = true;
        DirectionalFlat.LightDirection(new((Vector3)lightDirection, 0));
        DirectionalFlat.Model(Matrix4x4.CreateRotationY((float)theta) * Matrix4x4.CreateRotationX(-(float)phi) * Matrix4x4.CreateTranslation((Vector3)modelPosition));
        DirectionalFlat.View(Matrix4x4.CreateTranslation(-(Vector3)cameraPosition));
        DirectionalFlat.Projection(Matrix4x4.CreatePerspectiveFieldOfView(fPi / 4, (float)Width / Height, 0.1f, 100));
        DrawArrays(Primitive.Triangles, 0, 3 * Obj.Faces.Count);


    }

    void RenderSoftware () {
        var textRow = -Font.Height;
        softwareRenderSurface.ClearU32(Color.Black);
        var faceCount = Obj.Faces.Count;
        var vertexCount = Obj.Vertices.Count;
        var rotation = Matrix4d.RotationY(theta) * Matrix4d.RotationX(-phi);
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
            FacesAndDots[drawn] = ((float)Vector3d.Dot(Vector3d.Normalize(cross), -lightDirection), i);
            ++drawn;
        }
        if (drawn > 0) {
            if (drawn > 1)
                Array.Sort(FaceZ, FacesAndDots, 0, drawn);

            for (var i = 0; i < drawn; ++i) {
                var (d, idx) = FacesAndDots[i];
                var intensity = IntClamp((int)DoubleFloor(d * 256), byte.MinValue, byte.MaxValue);
                var (a, b, c) = Obj.Faces[idx];
                softwareRenderSurface.TriangleU32(ScreenSpace[a], ScreenSpace[b], ScreenSpace[c], Color.FromRgb(intensity, intensity, intensity));
            }
        }
        softwareRenderSurface.DrawString($"font height: {Font.Height} (EmSize {Font.EmSize})", Font, 0, textRow += Font.Height);
        softwareRenderSurface.DrawString(syncs[1 + State.SwapInterval], Font, 0, textRow += Font.Height);
        var (cx, cy) = CursorLocation;
        if (0 <= cx && cx < Width && 0 <= cy && cy < Height) {
            softwareRenderSurface.LineU32(CursorLocation, CursorLocation + new Vector2i(9, 0), Color.Green); // 10 pixels lit
            softwareRenderSurface.LineU32(CursorLocation, CursorLocation + new Vector2i(0, -9), Color.Green);
        }
        softwareRenderTexture.Upload(softwareRenderSurface);

        State.Program = PassThrough.Id;
        State.VertexArray = quad;
        State.DepthFunc = DepthFunction.Always;
        State.DepthTest = true;
        State.CullFace = true;
        softwareRenderTexture.BindTo(1);
        PassThrough.Tex(1);
        DrawArrays(Primitive.Triangles, 0, 6);

    }

    void AdjustFont (float delta) {
        var fh = Font.EmSize;
        var nh = FloatClamp(Font.EmSize + delta, 12, 36);
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
            case Keys.Tab:
                useOpenGl = !useOpenGl;
                return;
        }
    }

    void MouseMove_self (object sender, Vector2i e) {
        var d = e - lastCursorPosition;
        lastCursorPosition = e;
        switch (Buttons) {
            case Buttons.Left:
                theta = Extra.ModuloTwoPi(theta, 0.01 * d.X);
                phi = DoubleClamp(phi + 0.01 * d.Y, -dPi / 2, dPi / 2);
                break;
            case Buttons.Right:
                if (cameraPosition.Z > 0) {
                    var recipDistance = 1 / cameraPosition.Z;
                    modelPosition += new Vector3d(recipDistance * d.X, recipDistance * d.Y, 0);
                }
                break;
            case Buttons.ButtonX1:
                if (cameraPosition.Z > 0) {
                    var recipDistance = 1 / cameraPosition.Z;
                    modelPosition += new Vector3d(recipDistance * d.X, 0, -recipDistance * d.Y);
                }
                break;
        }
    }
}
