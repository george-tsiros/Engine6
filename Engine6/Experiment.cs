namespace Engine6;
using Win32;
using Common;
using Gl;
using static Gl.GlContext;
using System.Numerics;
using Shaders;
using System;
using System.Diagnostics;
using System.Collections.Generic;

public class Experiment:GlWindow {

    //private static readonly (int, int)[] C3_lines = { (0, 1), (0, 4), (1, 3), (3, 8), (4, 7), (6, 7), (6, 9), (5, 9), (5, 8), (2, 5), (2, 6), (3, 5), (4, 6), (1, 2), (0, 2), (8, 10), (10, 11), (7, 11), (1, 10), (0, 11), (1, 5), (0, 6), (20, 21), (12, 13), (18, 19), (14, 15), (16, 17), (15, 16), (14, 17), (13, 18), (12, 19), (2, 9), (22, 24), (23, 24), (22, 23), (25, 26), (26, 27), (25, 27), };
    //private static readonly Vector3[] C3_vertices = { new(32, 0, -76), new(-32, 0, -76), new(0, 26, -24), new(-120, -3, 8), new(120, -3, 8), new(-88, 16, 40), new(88, 16, 40), new(128, -8, 40), new(-128, -8, 40), new(0, 26, 40), new(-32, -24, 40), new(32, -24, 40), new(-36, 8, 40), new(-8, 12, 40), new(8, 12, 40), new(36, 8, 40), new(36, -12, 40), new(8, -16, 40), new(-8, -16, 40), new(-36, -12, 40), new(0, 0, -76), new(0, 0, -90), new(-80, -6, 40), new(-80, 6, 40), new(-88, 0, 40), new(80, 6, 40), new(88, 0, 40), new(80, -6, 40), };

    protected override Key[] AxisKeys { get; } = { Key.C, Key.X, Key.Z, Key.D, Key.Q, Key.A, Key.PageUp, Key.PageDown, Key.Home, Key.End, Key.Insert, Key.Delete, };

    private DirectionalFullbright directional;
    private VertexArray sa;
    private BufferObject<Vector4> sphereVertices;
    //private BufferObject<Vector4> sphereNormals;
    private Presentation presentation;
    private VertexArray pa;
    private BufferObject<Vector2> presentationVertices;
    private Framebuffer framebuffer;

    private Renderbuffer depthbuffer;
    private Sampler2D renderTexture;

    private Vector3 cameraLocation = new(0, 0, TerraLunaDistance);
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

    private const float TerraLunaDistance = 3.844e8f * .001f;
    private const float TerraRadius = 6.371e6f * .001f;
    private const float LunaRadius = 1.737e6f * .001f;
    private const float WhatRadius = 1e3f * .001f;
    private const float TerraMass = 5.972e24f * .001f;
    private const float LunaMass = 7.342e22f * .001f;
    private const float NearPlane = 1e3f * .001f;
    private const float FarPlane = 1e9f * .001f;

    public Experiment () {

        var allVertices = new Vector4[loPolySphereVertexCount + highPolySphereVertexCount];
        Sphere(loPolySphereSubdivisions, 1, allVertices.AsSpan(0, loPolySphereVertexCount));
        Sphere(highPolySphereSubdivisions, 1, allVertices.AsSpan(loPolySphereVertexCount, highPolySphereVertexCount));

        Recyclables.Add(sphereVertices = new(allVertices));
        //for (var i = 0; i < allVertices.Length; ++i)
        //    allVertices[i] = new(allVertices[i].Xyz(), 0);

        //Recyclables.Add(sphereNormals = new(allVertices));
        Recyclables.Add(presentationVertices = new(PresentationQuad));
        Recyclables.Add(framebuffer = new());
        Recyclables.Add(presentation = new());
        Recyclables.Add(pa = new());
        Recyclables.Add(sa = new());
        Recyclables.Add(directional = new());
        pa.Assign(presentationVertices, presentation.VertexPosition);
        sa.Assign(sphereVertices, directional.VertexPosition);
        //sa.Assign(sphereNormals, directional.VertexNormal);
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

    private void Update () {
        //var roll = IsKeyDown(Key.Z) ? 1 : 0;
        //if (IsKeyDown(Key.C))
        //    roll -= 1;
        //var pitch = IsKeyDown(Key.D) ? 1 : 0;
        //if (IsKeyDown(Key.X))
        //    pitch -= 1;

        //if (roll != 0) 
        //    cameraOrientation = Quaternion.Concatenate(cameraOrientation, Quaternion.CreateFromAxisAngle(-Vector3.UnitZ, .01f * roll));

        //if (pitch != 0) 
        //    cameraOrientation = Quaternion.Concatenate(cameraOrientation, Quaternion.CreateFromAxisAngle(Vector3.UnitX, .01f * pitch));

        var dx = Axis(Key.C, Key.Z);
        var dz = Axis(Key.D, Key.X);
        cameraLocation += new Vector3(dx, 0, -dz);

    }
    float theta = 0.0f;
    protected override void Render () {
        Update();
        var size = ClientSize;
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(Maths.fPi / 4, (float)size.X / size.Y, NearPlane, FarPlane);
        BindFramebuffer(framebuffer, FramebufferTarget.Draw);
        Viewport(in Vector2i.Zero, size);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        theta += .0001f;
        if (Maths.fTau < theta)
            theta -= Maths.fTau;
        //cameraOrientation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, theta);
        //cameraOrientation = Quaternion.Concatenate(cameraOrientation, Quaternion.CreateFromAxisAngle(Vector3.UnitZ, .001f));
        Enable(Capability.DepthTest);
        DepthFunc(DepthFunction.LessEqual);
        Enable(Capability.CullFace);
        BindVertexArray(sa);
        UseProgram(directional);
        //directional.LightDirection(-Vector4.UnitZ);
        directional.View(Matrix4x4.CreateRotationY(theta)* Matrix4x4.CreateTranslation(-cameraLocation));
        directional.Projection(projection);

        foreach (var body in TerraLunaSystem) {
            var translation = Matrix4x4.CreateTranslation(body.Position);
            //var scale = Matrix4x4.CreateScale(body.Radius);
            var model = translation;
            directional.Color(body.Color);
            directional.Model(model);
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
