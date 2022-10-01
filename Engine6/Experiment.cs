namespace Engine6;

using Win32;
using Common;
using Gl;
using static Gl.GlContext;
using System.Numerics;
using Shaders;
using System;
using System.Diagnostics;

public class Experiment:GlWindow {

    protected override Key[] AxisKeys { get; } = { Key.C, Key.X, Key.Z, Key.D, Key.Q, Key.A, Key.PageUp, Key.PageDown, Key.Home, Key.End, Key.Insert, Key.Delete, };

    private FlatColor flatColor;
    private VertexArray sa;
    private BufferObject<Vector4> sphereVertices;
    private Presentation presentation;
    private VertexArray pa;
    private BufferObject<Vector2> presentationVertices;
    private Framebuffer framebuffer;

    private Renderbuffer depthbuffer;
    private Sampler2D renderTexture;

    private Vector3 cameraLocation = new(TerraLunaDistance/2, 0, TerraLunaDistance);
    private Quaternion cameraOrientation = Quaternion.Identity;

    private static readonly Vector2i loPolySphereSubdivisions = new(10, 5);
    private static readonly Vector2i highPolySphereSubdivisions = new(50, 25);
    private static readonly int loPolySphereVertexCount = 3 * SphereTriangleCount(loPolySphereSubdivisions);
    private static readonly int highPolySphereVertexCount = 3 * SphereTriangleCount(highPolySphereSubdivisions);

    private readonly struct Body {
        public Vector3 Position { get; init; }
        public float Radius { get; init; }
        public Vector4 Color { get; init; }
        public float Mass { get; init; }
    };

    private static readonly Body Terra = new() { Position = new(), Radius = TerraRadius, Color = new(0, .6f, .7f, 1), Mass = TerraMass, };
    private static readonly Body Luna = new() { Position = new(TerraLunaDistance, 0, 0), Radius = LunaRadius, Color = new(.7f, .7f, .7f, 1), Mass = LunaMass };
    private static readonly Body What = new() { Position = new(0, 0, TerraLunaDistance - 10 * NearPlane), Radius = WhatRadius, Color = Vector4.One, Mass = 1 };
    private static readonly Body[] TerraLunaSystem = { Luna, Terra, What };
    private const float Scale = .001f;
    private const float TerraLunaDistance = 384.4f;// 3.844e8f * Scale;
    private const float TerraRadius = 6.371f;// 6.371e6f * Scale;
    private const float LunaRadius = 1.737f;// 1.737e6f * Scale;
    private const float WhatRadius = 1f;// 1e3f * Scale;
    private const float TerraMass = 1f;// 5.972e24f * Scale;
    private const float LunaMass = 1f;// 7.342e22f * Scale;
    private const float NearPlane = 1f;// 1e3f * Scale;
    private const float FarPlane = 1000f;// 1e9f * Scale;

    public Experiment () {

        var allVertices = new Vector4[loPolySphereVertexCount + highPolySphereVertexCount];
        Sphere(loPolySphereSubdivisions, 1, allVertices.AsSpan(0, loPolySphereVertexCount));
        Sphere(highPolySphereSubdivisions, 1, allVertices.AsSpan(loPolySphereVertexCount, highPolySphereVertexCount));

        Recyclables.Add(sphereVertices = new(allVertices));
        Recyclables.Add(presentationVertices = new(PresentationQuad));
        Recyclables.Add(framebuffer = new());
        Recyclables.Add(presentation = new());
        Recyclables.Add(pa = new());
        Recyclables.Add(sa = new());
        Recyclables.Add(flatColor = new());
        pa.Assign(presentationVertices, presentation.VertexPosition);
        sa.Assign(sphereVertices, flatColor.VertexPosition);
        SetSwapInterval(1);
    }

    protected override void OnLoad () {
        base.OnLoad();

        renderTexture = new(ClientSize, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Linear };
        depthbuffer = new(ClientSize, RenderbufferFormat.Depth24Stencil8);
        framebuffer.Attach(renderTexture, FramebufferAttachment.Color0);
        framebuffer.Attach(depthbuffer, FramebufferAttachment.DepthStencil);
        Debug.Assert(FramebufferStatus.Complete == framebuffer.CheckStatus());

        Disposables.Add(depthbuffer);
        Disposables.Add(renderTexture);
    }

    protected override void OnKeyDown (Key key, bool repeat) {
        switch (key) {
            case Key.Escape:
                User32.PostQuitMessage(0);
                return;
        }
        base.OnKeyDown(key, repeat);
    }

    static void Rotate (ref Quaternion q, float yaw, float pitch, float roll) { 
        var newX = Vector3.Transform(Vector3.UnitX, q);
        q = Quaternion.Concatenate(q, Quaternion.CreateFromAxisAngle(newX, pitch));
        var newY = Vector3.Transform(Vector3.UnitY, q);
        q = Quaternion.Concatenate(q, Quaternion.CreateFromAxisAngle(newY, yaw));
        var newZ = Vector3.Transform(Vector3.UnitZ, q);
        q = Quaternion.Concatenate(q, Quaternion.CreateFromAxisAngle(newZ, roll));
    
    }

    static void Translate (ref Vector4 p, ref Quaternion q, in Vector3 dr) {
        var dx = dr.X * Vector3.Transform(Vector3.UnitX, q);
        var dy = dr.Y * Vector3.Transform(Vector3.UnitY, q);
        var dz = dr.Z * Vector3.Transform(Vector3.UnitZ, q);
        p += new Vector4(dx + dy + dz, 0);
    }

    protected override void OnInput (int dx, int dy) {
        Rotate(ref cameraOrientation, 0, .001f * dy, .001f * dx);
    }

    protected override void Render () {
        var size = ClientSize;
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(Maths.fPi / 4, (float)size.X / size.Y, NearPlane, FarPlane);
        BindFramebuffer(framebuffer, FramebufferTarget.Draw);
        Viewport(in Vector2i.Zero, size);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        Enable(Capability.DepthTest);
        DepthFunc(DepthFunction.LessEqual);
        Enable(Capability.CullFace);
        BindVertexArray(sa);
        UseProgram(flatColor);
        flatColor.View(Matrix4x4.CreateTranslation(-cameraLocation) * Matrix4x4.CreateFromQuaternion(cameraOrientation));
        flatColor.Projection(projection);

        foreach (var body in TerraLunaSystem) {
            var translation = Matrix4x4.CreateTranslation(body.Position);
            var model = translation;
            flatColor.Color(body.Color);
            flatColor.Model(model);
            var cameraDistanceFromSphere = (body.Position - cameraLocation).Length();
            if (cameraDistanceFromSphere < 120f * body.Radius)
                DrawArrays(Primitive.Triangles, loPolySphereVertexCount, highPolySphereVertexCount);
            else
                DrawArrays(Primitive.Triangles, 0, loPolySphereVertexCount);
        }
        BindDefaultFramebuffer(FramebufferTarget.Draw);
        Disable(Capability.DepthTest);
        Viewport(in Vector2i.Zero, size);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        BindVertexArray(pa);
        UseProgram(presentation);
        presentation.Tex0(0);
        renderTexture.BindTo(0);
        DrawArrays(Primitive.Triangles, 0, 6);
    }

    private static void GetCoordinateSystem (Quaternion q, out Vector3 ux, out Vector3 uy, out Vector3 uz) {
        ux = Vector3.Transform(Vector3.UnitX, q);
        uy = Vector3.Transform(Vector3.UnitY, q);
        uz = Vector3.Transform(Vector3.UnitZ, q);
    }

    public static int SphereTriangleCount (Vector2i n) =>
        2 * n.X * (n.Y - 1);

    public static void Sphere (Vector2i n, float radius, Span<Vector4> vertices) {
        var (nTheta, nPhi) = n;
        var spherePointCount = 2 + nTheta * (nPhi - 1);
        var triangleCount = 2 * nTheta * (nPhi - 1);
        var vertexCount = 3 * triangleCount;
        if (vertices.Length != vertexCount)
            throw new ArgumentException($"expected size {vertexCount} exactly, got {vertices.Length} instead", nameof(vertices));
        var dTheta = 2 * Maths.dPi / nTheta;
        var dPhi = Maths.dPi / nPhi;
        var vectors = new Vector4[spherePointCount];
        vectors[0] = new(radius * Vector3.UnitY, 1f);
        vectors[spherePointCount - 1] = new(-radius * Vector3.UnitY, 1);

        var phi = dPhi;
        for (int vi = 1, i = 1; vi < nPhi; ++vi, phi += dPhi) {
            var (sp, cp) = Maths.DoubleSinCos(phi);
            var theta = 0.0;
            for (var hi = 0; hi < nTheta; ++hi, ++i, theta += dTheta) {
                var (st, ct) = Maths.DoubleSinCos(theta);
                vectors[i] = new((float)(radius * sp * ct), (float)(radius * cp), (float)(radius * sp * st), 1);
            }
        }
        var indices = new int[triangleCount * 3];

        var faceIndex = 0;

        // top 
        for (var i = 1; i <= nTheta; ++i, ++faceIndex) {
            indices[3 * faceIndex] = 0;
            indices[3 * faceIndex + 2] = i;
            indices[3 * faceIndex + 1] = i % nTheta + 1; // i = nTheta
        }

        for (var y = 1; y < nPhi - 1; ++y)
            for (var x = 1; x <= nTheta; ++x) {
                // a--d
                // |\ |
                // | \|
                // b--c
                var a = (y - 1) * nTheta + x;
                var b = y * nTheta + x;
                var c = y * nTheta + x % nTheta + 1;
                var d = (y - 1) * nTheta + x % nTheta + 1;

                indices[3 * faceIndex] = a;
                indices[3 * faceIndex + 2] = b;
                indices[3 * faceIndex + 1] = c;
                ++faceIndex;
                indices[3 * faceIndex] = a;
                indices[3 * faceIndex + 2] = c;
                indices[3 * faceIndex + 1] = d;
                ++faceIndex;
            }

        for (var i = 1; i <= nTheta; ++i) {
            // y = 0 => 1 vertex
            // y = 1 ntheta vertices, starting from '1' = (y-1) * nTheta + 1
            // y = nphi-2 , second to last, starting from (nphi-3) * ntheta +1
            // y = nphi - 1, last row, 1 vertex (the last one)

            var a = (nPhi - 2) * nTheta + i;
            var b = (nPhi - 2) * nTheta + i % nTheta + 1;

            Debug.Assert(a < vectors.Length);
            Debug.Assert(b < vectors.Length);
            indices[3 * faceIndex] = a;
            indices[3 * faceIndex + 2] = vectors.Length - 1;
            indices[3 * faceIndex + 1] = b;
            ++faceIndex;
        }
        for (var i = 0; i < indices.Length; ++i)
            vertices[i] = vectors[indices[i]];
    }
}
