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

    private Raster raster;
    private Sampler2D sampler;
    private VertexArray quad;
    private VertexBuffer<Vector4> quadBuffer;

    private int PointCount;
    private Vector3[] points3D;
    private Vector2i[] points2Di;

    private static readonly byte[] White = { 255, 255, 255, 255 };
    private static readonly byte[] Black = { 64, 0, 0, 255 };
    private float theta, phi;
    private const float TwoPi = (float)(Math.PI * 2);

    private Matrix4x4 viewProjection;// = Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)Width / Height, 1, 100);
    private readonly Matrix4x4 ModelTranslation = Matrix4x4.CreateTranslation(0, -1, -20);
    private bool leftDown;
    private int lastX = -1;
    private int lastY;

    private Framebuffer fb;
    private Sampler2D fbsampler;
    private Renderbuffer rb;

    protected override void Load () {
        viewProjection = Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY) * Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)Width / Height, 1, 100);
        const string TeapotFilepath = @"data\teapot.obj";

        rb = new(new(Width, Height), RenderbufferFormat.Depth24Stencil8);

        fbsampler = new(new(Width, Height), TextureFormat.Rgba8);
        fbsampler.Mag = MagFilter.Nearest;
        fbsampler.Min = MinFilter.Nearest;

        fb = new();

        fb.Attach(rb, FramebufferAttachment.DepthStencil);

        fb.Attach(fbsampler, FramebufferAttachment.Color0);
        NamedFramebufferDrawBuffer(fb, DrawBuffer.Color0);

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
        Disposables.Add(raster);
        Disposables.Add(sampler);
        Disposables.Add(quad);
        Disposables.Add(quadBuffer);
    }

    protected override void ButtonDown (int b) {
        if (b == 1)
            (leftDown, lastX) = (true, -1);
    }
    protected override void ButtonUp (int b) {
        if (b == 1)
            leftDown = false;
    }

    protected override void MouseMove (int x, int y) {
        if (!leftDown)
            return;

        if (lastX < 0) {
            (lastX, lastY) = (x, y);
        } else {
            var (dx, dy) = (x - lastX, y - lastY);
            theta += 0.01f * dx;
            phi += 0.01f * dy;
            if (theta > TwoPi)
                theta -= TwoPi;
            else if (theta < 0)
                theta += TwoPi;
            if (phi > TwoPi)
                phi -= TwoPi;
            else if (phi < 0)
                phi += TwoPi;
            (lastX, lastY) = (x, y);
        }
    }
    protected override void Render (float dt) {

        var mvp = Matrix4x4.CreateRotationY(theta) * Matrix4x4.CreateRotationX(phi) * ModelTranslation * viewProjection;

        PutPixels(raster, points2Di, Black);
        for (int i = 0; i < PointCount; i++) {
            var p = Vector4.Transform(points3D[i], mvp);
            Debug.Assert(p.W > 1e-10);
            var n = new Vector2(p.X / p.W, p.Y / p.W);
            points2Di[i] = new((int)(Width * (n.X + 0.5f)), (int)(Height * (0.5f - n.Y)));
        }
        PutPixels(raster, points2Di);
        sampler.Upload(raster);

        State.Framebuffer = fb;
        Viewport(0, 0, Width, Height);
        Clear(BufferBit.Color | BufferBit.Depth);
        State.Program = PassThrough.Id;
        State.VertexArray = quad;
        State.DepthTest = true;
        State.DepthFunc = DepthFunction.Always;
        State.CullFace = true;
        sampler.BindTo(1);
        PassThrough.Tex(1);
        DrawArrays(Primitive.Triangles, 0, 6);

        State.Framebuffer = 0;
        Viewport(0, 0, Width, Height);
        Clear(BufferBit.Color | BufferBit.Depth);

        fbsampler.BindTo(1);
        PassThrough.Tex(1);
        DrawArrays(Primitive.Triangles, 0, 6);
    }

    unsafe public static void PutPixels (Raster b, IEnumerable<Vector2i> points, byte[] color = null) {
        Debug.Assert(b.Stride == b.Width * 4);
        color ??= White;
        Debug.Assert(color.Length == 4);
        foreach (var p in points)
            if (0 <= p.X && 0 <= p.Y && p.X < b.Width && p.Y < b.Height)
                Array.Copy(color, 0, b.Pixels, b.Stride * p.Y + 4 * p.X, 4);
    }
    bool useFramebuffer;
    protected override void KeyUp (Keys k) {
        switch (k) {
            case Keys.Space:
                useFramebuffer = !useFramebuffer;
                break;
        }
    }

    protected override void KeyDown (Keys k) {
        base.KeyDown(k);
    }
}
