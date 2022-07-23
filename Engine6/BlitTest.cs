namespace Engine;

using System;
using System.Numerics;
using Shaders;
using System.Diagnostics;
using System.Collections.Generic;
using Gl;
using Win32;
using static Gl.Opengl;
using static Gl.Utilities;

class BlitTest:GlWindowArb {


    public BlitTest (Vector2i size, Model m = null) : base(size) {
        const string TeapotFilepath = @"data\teapot.obj";
        model = m ?? new Model(TeapotFilepath);
        PointCount = model.Vertices.Count;
        points3D = model.Vertices.ToArray();
        points2Di = new Vector2i[PointCount];
    }

    Raster raster;
    Sampler2D sampler;
    VertexArray quad;
    VertexBuffer<Vector4> quadBuffer;

    int PointCount;
    Vector3[] points3D;
    Vector2i[] points2Di;

    static readonly byte[] White = { 255, 255, 255, 255 };
    static readonly byte[] Black = { 64, 0, 0, 255 };
    float theta, phi;

    Matrix4x4 viewProjection;
    readonly Matrix4x4 ModelTranslation = Matrix4x4.CreateTranslation(0, 0, -20);
    bool leftDown;
    int lastX = -1;
    int lastY;

    Framebuffer fb;
    Sampler2D fbsampler;
    Renderbuffer rb;

    Model model;

    protected override void Load () {
        viewProjection = Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY) * Matrix4x4.CreatePerspectiveFieldOfView(float.Pi / 4, (float)Width / Height, 1, 100);


        rb = new(new(Width, Height), RenderbufferFormat.Depth24Stencil8);

        fbsampler = new(new(Width, Height), TextureFormat.Rgba8);
        fbsampler.Mag = MagFilter.Nearest;
        fbsampler.Min = MinFilter.Nearest;

        fb = new();

        fb.Attach(rb, FramebufferAttachment.DepthStencil);

        fb.Attach(fbsampler, FramebufferAttachment.Color0);
        NamedFramebufferDrawBuffer(fb, DrawBuffer.Color0);

        quad = new();
        State.Program = PassThrough.Id;
        quadBuffer = new(Quad.Vertices);
        quad.Assign(quadBuffer, PassThrough.VertexPosition);
        raster = new(new(Width, Height), 4, 1);
        MemSet(raster.Pixels, 0xff000040u);

        sampler = new(raster.Size, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest };
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

        if (0 <= lastX) {
            var (dx, dy) = (x - lastX, y - lastY);
            theta += 0.01f * dx;
            phi += 0.01f * dy;
            if (theta > float.Tau)
                theta -= float.Tau;
            else if (theta < 0)
                theta += float.Tau;
            if (phi > float.Tau)
                phi -= float.Tau;
            else if (phi < 0)
                phi += float.Tau;
        }
        (lastX, lastY) = (x, y);

    }
    static readonly long MicroSecondTicks = Stopwatch.Frequency / 1_000_000;
    static readonly long MilliSecondTicks = Stopwatch.Frequency / 1_000;
    static readonly long SecondTicks = Stopwatch.Frequency;

    protected override void Render (float dt) {
        var mvp = Matrix4x4.CreateRotationY(theta) * Matrix4x4.CreateRotationX(phi) * ModelTranslation * viewProjection;

        PutPixels(raster, points2Di, Black);
        for (int i = 0; i < PointCount; i++) {
            var p = Vector4.Transform(points3D[i], mvp);
            Debug.Assert(p.W > 1e-10);
            var n = new Vector2(p.X / p.W, p.Y / p.W);
            points2Di[i] = new((int)(Width * (n.X + 0.5f)), (int)(Height * (n.Y + .5f)));
        }
        PutPixels(raster, points2Di);
        var color = 0u;
        var length = 0;
        if (RenderTicks > 0) {
            if (RenderTicks < MicroSecondTicks) {
                //ns
                length = (int)double.Round((double)Width * RenderTicks / MicroSecondTicks);
                color = 0xff00ff00;
            } else if (RenderTicks < MilliSecondTicks) {
                //us
                length = (int)double.Round((double)Width * RenderTicks / MilliSecondTicks);
                color = 0xffffff00;
            } else if (RenderTicks < SecondTicks) {
                //ms
                length = (int)double.Round((double)Width * RenderTicks / SecondTicks);
                color = 0xffff0000;
            } else
                Debug.WriteLine($"{FramesRendered}: {RenderTicks}");
        }
        if (color != 0 && length > 0) {
            for (var y = 0; y < 4; ++y) {
                Horizontal(raster, 0xff000040, 0, y, Width);
                Horizontal(raster, color, 0, y, length);
            }
        }
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
    unsafe static void Horizontal (Raster raster, uint color, int x, int y, int length) {
        Debug.Assert(raster.Stride == raster.Width * 4);
        if (length <= 0 || x < 0 || raster.Width < x + length || y < 0 || raster.Height <= y)
            return;
        fixed (byte* bytes = raster.Pixels) {
            uint* p = (uint*)bytes;
            var i = y * raster.Width + x;
            var end = i + length;
            while (i < end)
                p[i++] = color;
        }
    }

    public static void PutPixels (Raster b, Vector2i[] points, byte[] color = null) {
        Debug.Assert(b.Stride == b.Width * 4);
        color ??= White;
        Debug.Assert(color.Length == 4);
        foreach (var p in points)
            if (0 <= p.X && 0 <= p.Y && p.X < b.Width && p.Y < b.Height)
                Array.Copy(color, 0, b.Pixels, b.Stride * p.Y + 4 * p.X, 4);
    }

    public unsafe static void PutPixelsFast (Raster b, Vector2i[] points, uint color = ~0u) {
        Debug.Assert(b.Stride == b.Width * 4);
        fixed (byte* bytes = b.Pixels) {
            uint* raster = (uint*)bytes;
            foreach (var p in points)
                if (0 <= p.X && 0 <= p.Y && p.X < b.Width && p.Y < b.Height)
                    raster[b.Width * p.Y + p.X] = color;
        }
    }

    bool useFramebuffer;
    protected override void KeyUp (Keys k) {
        switch (k) {
            case Keys.Space:
                useFramebuffer = !useFramebuffer;
                break;
        }
    }

    bool useFast = false;

    protected override void KeyDown (Keys k) {
        base.KeyDown(k);
    }
}
