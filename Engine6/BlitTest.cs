namespace Engine6;

using System;
using System.Numerics;
using System.Diagnostics;
using Gl;
using Shaders;
using Win32;
using static Gl.RenderingContext;
using static Common.Maths;
using Common;

public class BlitTest:GlWindow {
    private static readonly string[] syncs = "free sink,no sync at all,vsync".Split(',');
    private static readonly Vector4[] QuadVertices = {
        new(-1f, -1f, 0, 1),
        new(+1f, -1f, 0, 1),
        new(+1f, +1f, 0, 1),
        new(-1f, -1f, 0, 1),
        new(+1f, +1f, 0, 1),
        new(-1f, +1f, 0, 1),
    };

    private static Vector2i NormalizedToScreen (in Vector3d n, in Vector2i size) =>
        new((int)((n.X + 1) * 0.5 * size.X), (int)((n.Y + 1) * 0.5 * size.Y));

    private static bool IsInside (in Vector3d v) =>
        -1 < v.Z && v.Z < 1;

    private ICamera camera = new QCamera(new(0, 0, 20));
    private readonly Vector3i[] Faces;
    private readonly Vector4d[] Vertices;
    //private Vector2i lastCursorPosition = new(-1, -1);
    private double phi = 0, theta = 0;
    private readonly Vector3d lightDirection = Vector3d.Normalize(-Vector3d.One);
    private Raster softwareRenderSurface;
    private Sampler2D softwareRenderTexture;
    private VertexArray quad;
    private Framebuffer offscreenFramebuffer;
    private Sampler2D offscreenRenderingSurface;
    private Renderbuffer offscreenDepthbuffer;
    private Vector3d[] ClipSpace;
    private Vector2i[] ScreenSpace;
    private Vector3d[] ModelSpace;
    private double[] FaceZ;
    private (double depth, int index)[] FacesAndDots;
    private VertexArray someLines;
    private VertexBuffer<Vector4> quadBuffer;
    private PassThrough passThrough;
    private DirectionalFlat directionalFlat;
    private Lines lines;

    public BlitTest (Model m = null) : base() {
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
        Load += OnLoad;
        KeyUp += OnKeyUp;
    }

    void OnKeyUp (object sender, KeyEventArgs args) {
        switch (args.Key) {
            case Key.Escape:
                User32.PostQuitMessage(0);
                return;
        }
    }

    void OnLoad (object sender, EventArgs _) {
        var size = ClientSize;
        offscreenDepthbuffer = new(size, RenderbufferFormat.Depth24Stencil8);
        offscreenRenderingSurface = new(size, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest };
        offscreenFramebuffer = new();
        offscreenFramebuffer.Attach(offscreenDepthbuffer, FramebufferAttachment.DepthStencil);
        offscreenFramebuffer.Attach(offscreenRenderingSurface, FramebufferAttachment.Color0);
        NamedFramebufferDrawBuffer(offscreenFramebuffer, DrawBuffer.Color0);
        quad = new();
        passThrough = new();
        UseProgram(passThrough);
        quadBuffer = new(QuadVertices);
        quad.Assign(quadBuffer, passThrough.VertexPosition);
        softwareRenderSurface = new(size, 4, 1);
        softwareRenderTexture = new(size, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest };
        offscreenRenderingSurface.BindTo(1);
        passThrough.Tex(1);
        directionalFlat = new();
        UseProgram(directionalFlat);
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
    }

    protected override void Render () {
        var dx = IsKeyDown(Key.C) ? .1f : 0f;
        if (IsKeyDown(Key.Z))
            dx -= .1f;
        var dy = IsKeyDown(Key.ShiftKey) ? .1f : 0f;
        if (IsKeyDown(Key.ControlKey))
            dy -= .1f;
        var dz = IsKeyDown(Key.X) ? .1f : 0f;
        if (IsKeyDown(Key.D))
            dz -= .1f;
        camera.Walk(dx, dy, dz);
        BindFramebuffer(offscreenFramebuffer);
        var size = ClientSize;
        Viewport(new(), size);
        Clear(BufferBit.ColorDepth);

        RenderSoftware();
        BindDefaultFramebuffer();
        BindVertexArray(quad);
        UseProgram(passThrough);
        Viewport(new(), size);
        Clear(BufferBit.ColorDepth);

        DrawArrays(Primitive.Triangles, 0, 6);

        BindVertexArray(someLines);
        UseProgram(lines);
        Disable(Capability.DepthTest);
        Disable(Capability.CullFace);
        lines.Color(new(0, 1, 0, 1));
        lines.RenderSize(size);
        lines.Offset(cursorLocation);
        DrawArrays(Primitive.LineStrip, 0, 3);

    }

    private Vector2i cursorLocation = new(-1, -1);

    private void RenderSoftware () {
        var textRow = -Font.Height;
        softwareRenderSurface.ClearU32(Color.Black);
        var faceCount = Faces.Length;
        var vertexCount = Vertices.Length;
        var model = Matrix4d.RotationY(theta) * Matrix4d.RotationX(-phi);
        var translation = Matrix4d.Translate(-camera.Location);
        var r = ClientSize;
        var projection = Matrix4d.Project(dPi / 4, (double)r.X / r.Y, 1, 100);
        for (var i = 0; i < vertexCount; ++i) {
            var modelSpace = Vertices[i] * model;
            ModelSpace[i] = modelSpace.Xyz();
            var projected = modelSpace * translation * projection;
            var n = projected.Xyz() / projected.W;
            ClipSpace[i] = n;
            ScreenSpace[i] = NormalizedToScreen(n, r);
        }

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

        if (drawn > 0) {
            if (drawn > 1)
                Array.Sort(FaceZ, FacesAndDots, 0, drawn);

            for (var i = 0; i < drawn; ++i) {
                var (d, idx) = FacesAndDots[i];
                var intensity = IntClamp((int)DoubleFloor(d * 256), byte.MinValue, byte.MaxValue);
                var color = Color.FromRgb(intensity, intensity, intensity);
                var (a, b, c) = Faces[idx];
                softwareRenderSurface.TriangleU32(ScreenSpace[a], ScreenSpace[b], ScreenSpace[c], color);
            }
        }
        softwareRenderSurface.DrawString(VersionString, Font, 0, textRow += Font.Height);

        softwareRenderTexture.Upload(softwareRenderSurface);

        UseProgram(passThrough);
        BindVertexArray(quad);
        Enable(Capability.DepthTest);
        DepthFunc(DepthFunction.Always);
        Enable(Capability.CullFace);
        softwareRenderTexture.BindTo(1);
        passThrough.Tex(1);
        DrawArrays(Primitive.Triangles, 0, 6);
    }
}
