namespace Engine6;

using System;
using System.Numerics;
using System.Diagnostics;
using Gl;
using Shaders;
using Win32;
using static Gl.Opengl;
using static Common.Maths;
using Common;

enum FooNum {
    Frame = 1,
    Software,
    Hardware,
    Clear,
    Geometry,
    Visibility,
    Rasterization,
    TextureUpload,
}

internal class BlitTest:GlWindow{
    static readonly string[] syncs = "free sink,no sync at all,vsync".Split(',');

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

    Camera camera = new(new(0, 0, 20));
    readonly Vector3i[] Faces;
    readonly Vector4d[] Vertices;
    Vector2i lastCursorPosition = new(-1, -1);
    double phi = 0, theta = 0;
    readonly Vector3d lightDirection = Vector3d.Normalize(-Vector3d.One);

    Raster softwareRenderSurface;
    Sampler2D softwareRenderTexture;
    VertexArray quad;
    Framebuffer offscreenFramebuffer;
    Sampler2D offscreenRenderingSurface;
    Renderbuffer offscreenDepthbuffer;
    Vector3d[] ClipSpace;
    Vector2i[] ScreenSpace;
    Vector3d[] ModelSpace;
    double[] FaceZ;
    (double depth, int index)[] FacesAndDots;

    VertexArray someLines;
    VertexBuffer<Vector4> quadBuffer;
    Perf<FooNum> prf;
    PassThrough passThrough;
    DirectionalFlat directionalFlat;
    Lines lines;

    public BlitTest (Model m = null) {
        Debug.Assert(Stopwatch.Frequency == 10_000_000);
        const string TeapotFilepath = @"data\teapot.obj";
        var model = m ?? new Model(TeapotFilepath);
        Vertices = model.Vertices.ConvertAll(v => new Vector4d(v, 1)).ToArray();
        Faces = model.Faces.ToArray();
        ClipSpace = new Vector3d[model.Vertices.Count];
        ScreenSpace = new Vector2i[model.Vertices.Count];
        ModelSpace = new Vector3d[model.Vertices.Count];
        FaceZ = new double[model.Faces.Count];
        FacesAndDots = new (double, int)[model.Faces.Count];

        State.SwapInterval = 1;
    }

    protected override void OnLoad () {
        var size = Rect.Size;
        offscreenDepthbuffer = new(size, RenderbufferFormat.Depth24Stencil8);
        offscreenRenderingSurface = new(size, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest };
        offscreenFramebuffer = new();
        offscreenFramebuffer.Attach(offscreenDepthbuffer, FramebufferAttachment.DepthStencil);
        offscreenFramebuffer.Attach(offscreenRenderingSurface, FramebufferAttachment.Color0);
        NamedFramebufferDrawBuffer(offscreenFramebuffer, DrawBuffer.Color0);
        quad = new();
        passThrough = new();
        State.Program = passThrough;
        quadBuffer = new(QuadVertices);
        quad.Assign(quadBuffer, passThrough.VertexPosition);
        softwareRenderSurface = new(size, 4, 1);
        softwareRenderTexture = new(size, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest };
        offscreenRenderingSurface.BindTo(1);
        passThrough.Tex(1);
        directionalFlat = new();
        State.Program = directionalFlat;
        var faceCount = Faces.Length;
        var vertexCount = faceCount * 3;
        var vertices = new Vector4[vertexCount];
        var normals = new Vector4[vertexCount];
        for (var (i, j) = (0, 0); j < vertexCount; ++i, ++j) {
            var (a, b, c) = Faces[i];
            var (v0, v1, v2) = (Vertices[a], Vertices[b], Vertices[c]);
            normals[j + 2] = normals[j + 1] = normals[j] = new((Vector3)Vector3d.Normalize(Vector3d.Cross((v1 - v0).Xyz(), (v2 - v0).Xyz())), 0);
            vertices[j] = (Vector4)v0;
            vertices[++j] = (Vector4)v1;
            vertices[++j] = (Vector4)v2;
        }

        someLines = new();
        lines = new();
        someLines.Assign(new VertexBuffer<Vector2i>(new Vector2i[] { new(0, -9), new(0, 0), new(10, 0) }), lines.VertexPosition);

        Disposables.Add(softwareRenderSurface);
        Disposables.Add(softwareRenderTexture);
        Disposables.Add(quad);
        Disposables.Add(lines);
        Disposables.Add(someLines);
        Disposables.Add(quadBuffer);
        Disposables.Add(prf = new("log.bin"));
    }

    protected override void Render () {
        prf.Enter((int)FooNum.Frame);
        var dx = IsKeyDown(Keys.C) ? .1f : 0f;
        if (IsKeyDown(Keys.Z))
            dx -= .1f;
        var dy = IsKeyDown(Keys.ShiftKey) ? .1f : 0f;
        if (IsKeyDown(Keys.ControlKey))
            dy -= .1f;
        var dz = IsKeyDown(Keys.X) ? .1f : 0f;
        if (IsKeyDown(Keys.D))
            dz -= .1f;
        camera.Location += new Vector3(dx, dy, dz);
        State.Framebuffer = offscreenFramebuffer;
        Viewport(new(), Rect.Size);
        Clear(BufferBit.ColorDepth);

        prf.Enter((int)FooNum.Software);
        RenderSoftware();
        prf.Leave();
        State.Framebuffer = 0;
        State.VertexArray = quad;
        State.Program = passThrough;
        Viewport(new(), Rect.Size);
        Clear(BufferBit.ColorDepth);

        DrawArrays(Primitive.Triangles, 0, 6);

        State.VertexArray = someLines;
        State.Program = lines.Id;
        State.DepthTest = false;
        State.CullFace = false;
        lines.Color(new(0, 1, 0, 1));
        lines.RenderSize(Rect.Size);
        lines.Offset(cursorLocation);
        DrawArrays(Primitive.LineStrip, 0, 3);

        prf.Leave();
    }
    Vector2i cursorLocation = new(-1, -1);

    void RenderSoftware () {
        var textRow = -Font.Height;
        prf.Enter((int)FooNum.Clear);
        softwareRenderSurface.ClearU32(Color.Black);
        prf.Leave();
        prf.Enter((int)FooNum.Geometry);
        var faceCount = Faces.Length;
        var vertexCount = Vertices.Length;
        var model = Matrix4d.RotationY(theta) * Matrix4d.RotationX(-phi);
        var translation = Matrix4d.Translate(-camera.Location);
        var projection = Matrix4d.Project(dPi / 4, (double)Rect.Width / Rect.Height, 1, 100);
        for (var i = 0; i < vertexCount; ++i) {
            var modelSpace = Vertices[i] * model;
            ModelSpace[i] = modelSpace.Xyz();
            var projected = modelSpace * translation * projection;
            var n = projected.Xyz() / projected.W;
            ClipSpace[i] = n;
            ScreenSpace[i] = NormalizedToScreen(n, Rect.Size);
        }
        prf.Leave();
        prf.Enter((int)FooNum.Visibility);
        var drawn = 0;
        for (var i = 0; i < faceCount; ++i) {
            var (a, b, c) = Faces[i];
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
        prf.Leave();
        prf.Enter((int)FooNum.Rasterization);
        if (drawn > 0) {
            if (drawn > 1)
                Array.Sort(FaceZ, FacesAndDots, 0, drawn);

            for (var i = 0; i < drawn; ++i) {
                var (d, idx) = FacesAndDots[i];
                var intensity = IntClamp((int)DoubleFloor(d * 256), byte.MinValue, byte.MaxValue);
                var (a, b, c) = Faces[idx];
                softwareRenderSurface.TriangleU32(ScreenSpace[a], ScreenSpace[b], ScreenSpace[c], Color.FromRgb(intensity, intensity, intensity));
            }
        }
        softwareRenderSurface.DrawString(VersionString, Font, 0, textRow += Font.Height);
        softwareRenderSurface.DrawString(IsSupported("GL_ARB_clip_control").ToString(), Font, 0, textRow += Font.Height);
        softwareRenderSurface.DrawString(syncs[1 + State.SwapInterval], Font, 0, textRow += Font.Height);

        prf.Leave();
        prf.Enter((int)FooNum.TextureUpload);

        softwareRenderTexture.Upload(softwareRenderSurface);
        prf.Leave();
        State.Program = passThrough;
        State.VertexArray = quad;
        State.DepthTest = true;
        State.DepthFunc = DepthFunction.Always;
        State.CullFace = true;
        softwareRenderTexture.BindTo(1);
        passThrough.Tex(1);
        DrawArrays(Primitive.Triangles, 0, 6);

    }

    protected override void OnKeyDown (Keys k) {
        switch (k) {
            case Keys.Space:
                camera.Location = new();
                return;
                //case Keys.M:
                //    CursorGrabbed = !CursorGrabbed;
                //    return;
        }
    }

    protected override void OnMouseMove (in Vector2i e) {
        //if (CursorGrabbed) {
        //    switch (Buttons) {
        //        case Buttons.Left:
        //            theta = Extra.ModuloTwoPi(theta, 0.01 * e.X);
        //            phi = DoubleClamp(phi - 0.01 * e.Y, -dPi / 2, dPi / 2);
        //            break;
        //    }
        //} else {
        var d = e - lastCursorPosition;
        switch (Buttons) {
            case Buttons.Left:
                theta = Functions.ModuloTwoPi(theta, 0.01 * d.X);
                phi = DoubleClamp(phi + 0.01 * d.Y, -dPi / 2, dPi / 2);
                break;
        }
        //}
        lastCursorPosition = e;
    }
}
