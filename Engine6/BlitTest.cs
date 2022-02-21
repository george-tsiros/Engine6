namespace Engine;

using System;
using System.Numerics;
using Shaders;
using Gl;
using static Gl.Opengl;
using static Gl.Utilities;
using Win32;
using System.Diagnostics;
using System.Collections.Generic;

class BlitTest:GlWindow {
    public BlitTest (Vector2i size) : base(size) { }
    private Camera Camera { get; } = new(new(0, 0, 5));
    private Raster raster;
    private Sampler2D sampler;
    private VertexArray quad;
    private VertexBuffer<Vector4> quadBuffer;

    int PointCount;
    Vector3[] points3D;
    Vector2i[] points2Di;

    static readonly byte[] White = { 255, 255, 255, 255 };
    float theta;
    const float TwoPi = (float)(Math.PI * 2);

    protected override void Load () {
        viewProjection = Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY) * Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)Width / Height, 1, 100);
        //projection = Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)Width / Height, 1, 100);
        const string TeapotFilepath = @"data\teapot.obj";
        var teapot = new Model(TeapotFilepath);
        PointCount = teapot.Vertices.Count;
        points3D = teapot.Vertices.ToArray();
        points2Di = new Vector2i[PointCount];

        quad = new();
        State.Program = PassThrough.Id;
        quadBuffer = new(Quad.Vertices);
        quad.Assign(quadBuffer, PassThrough.VertexPosition);
        raster = new(new(Width, Height), 4, 1);
        MemSet(raster.Pixels, 0xff000040u);

        sampler = new(raster.Size, TextureFormat.Rgba8);
        sampler.Mag = MagFilter.Nearest;
        sampler.Min = MinFilter.Nearest;
        sampler.Wrap = Wrap.ClampToEdge;
    }

    unsafe public static void PutPixels (Raster b, IEnumerable<Vector2i> points, byte[] color = null) {
        Debug.Assert(b.Stride == b.Width * 4);
        color ??= White;
        Debug.Assert(color.Length == 4);
        foreach (var p in points)
            if (0 <= p.X && 0 <= p.Y && p.X < b.Width && p.Y < b.Height)
                Array.Copy(color, 0, b.Pixels, b.Stride * p.Y + 4 * p.X, 4);
    }

    //Matrix4x4 view;// = Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY);
    //Matrix4x4 projection;// = Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)Width / Height, 1, 100);

    Matrix4x4 viewProjection;// = Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)Width / Height, 1, 100);
    static readonly byte[] Black = { 64, 0, 0, 255 };
    static readonly Matrix4x4 ModelTranslation = Matrix4x4.CreateTranslation(0, -1, -20);
    protected override void Render (float dt) {
        var t0 = Stopwatch.GetTimestamp();
        theta += 0.1f * dt;
        if (theta > TwoPi)
            theta -= TwoPi;

        var mvp = Matrix4x4.CreateRotationY(theta) * ModelTranslation * viewProjection;

        PutPixels(raster, points2Di, Black);
        for (int i = 0; i < PointCount; i++) {
            var p = Vector4.Transform(points3D[i], mvp);
            Debug.Assert(p.W > 1e-10);
            var n = new Vector2(p.X / p.W, p.Y / p.W);
            points2Di[i] = new((int)(Width * (n.X + 0.5f)), (int)(Height * (0.5f - n.Y)));
        }
        PutPixels(raster, points2Di);
        Debug.WriteLine(1000.0 * (Stopwatch.GetTimestamp() - t0) / Stopwatch.Frequency);
        sampler.Upload(raster);
        glViewport(0, 0, Width, Height);
        glClear(BufferBit.Color | BufferBit.Depth);
        State.Program = PassThrough.Id;
        State.VertexArray = quad;
        State.DepthTest = true;
        State.DepthFunc = DepthFunction.Always;
        State.CullFace = true;
        sampler.BindTo(1);
        PassThrough.Tex(1);
        glDrawArrays(Primitive.Triangles, 0, 6);
    }

    protected override void Closing () {
        raster.Dispose();
        sampler.Dispose();
        quad.Dispose();
        quadBuffer.Dispose();
    }
}
