namespace Engine;

using System;
using System.Numerics;
using Shaders;
using Gl;
using static Gl.Opengl;
using Win32;
using System.Diagnostics;

class Teapot:GlWindowArb {
    public Teapot (Vector2i size, Model model) : base(size) {
        Model = model;
        VertexCount = Model.Faces.Count * 3;
    }

    Model Model;
    VertexArray vao;
    readonly int VertexCount;
    Framebuffer fb;
    Renderbuffer depthStencil;
    Sampler2D vertexId, color0;
    VertexArray quad;
    byte[] Pixels;
    protected override void Load () {
        Pixels = new byte[Width * Height * sizeof(int)];
        fb = new();
        depthStencil = new(new(Width, Height), RenderbufferFormat.Depth24Stencil8);
        fb.Attach(depthStencil, FramebufferAttachment.DepthStencil);
        color0 = new(new(Width, Height), TextureFormat.Rgba8);
        color0.Mag = MagFilter.Nearest;
        color0.Min = MinFilter.Nearest;
        fb.Attach(color0, FramebufferAttachment.Color0);
        vertexId = new(new(Width, Height), TextureFormat.R32i);
        vertexId.Mag = MagFilter.Nearest;
        vertexId.Min = MinFilter.Nearest;
        fb.Attach(vertexId, FramebufferAttachment.Color1);

        NamedFramebufferDrawBuffers(fb, DrawBuffer.Color0, DrawBuffer.Color1);
        NamedFramebufferReadBuffer(fb, DrawBuffer.Color1);
        State.Program = VertexIndex.Id;

        var largestDimension = Math.Max(Math.Max(Model.Max.X - Model.Min.X, Model.Max.Y - Model.Min.Y), Model.Max.Z - Model.Min.Z);
        Geometry.ScaleInPlace(Model.Vertices, 1 / largestDimension);

        vao = new();
        var v = new Vector4[Model.Faces.Count * 3];
        for (var (i, j) = (0, 0); j < VertexCount; ++i, ++j) {
            var face = Model.Faces[i];
            v[j] = new(Model.Vertices[face.X], 1);
            v[++j] = new(Model.Vertices[face.Y], 1);
            v[++j] = new(Model.Vertices[face.Z], 1);
        }

        vao.Assign(new VertexBuffer<Vector4>(v), VertexIndex.VertexPosition);
        VertexIndex.Color0(Vector4.One);
        VertexIndex.Color1(new(1, 0, 0, 1));

        VertexIndex.Model(Matrix4x4.CreateTranslation(0, 0, -2));
        VertexIndex.Projection(Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)Width / Height, 1, 100));
        VertexIndex.View(Matrix4x4.Identity);

        State.Program = PassThrough.Id;
        quad = new();
        quad.Assign(new VertexBuffer<Vector4>(Quad.Vertices), PassThrough.VertexPosition);
        State.SwapInterval = 1;
    }

    protected override void KeyUp (Keys k) {
        switch (k) {
            case Keys.Up:
                fovRatio = Math.Min(fovRatio + 1, 6);
                break;
            case Keys.Down:
                fovRatio = Math.Max(fovRatio - 1, 2);
                break;
        }
    }


    protected override void MouseMove (int x, int y) => (lastX, lastY) = (x, y);

    int lastX, lastY;
    int fovRatio = 4;
    uint lastTriangle = 0;
    unsafe protected override void Render (float dt) {
        State.Framebuffer = fb;
        color0.BindTo(0);
        vertexId.BindTo(1);
        Viewport(0, 0, Width, Height);
        Clear(BufferBit.Color | BufferBit.Depth);
        State.Program = VertexIndex.Id;
        State.VertexArray = vao;
        State.DepthTest = true;
        State.DepthFunc = DepthFunction.LessEqual;
        State.CullFace = true;
        VertexIndex.Tri((int)lastTriangle);
        VertexIndex.Projection(Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / fovRatio), (float)Width / Height, 1, 100));
        DrawArrays(Primitive.Triangles, 0, VertexCount);

        if (lastX >= 0) {
            Span<uint> pixel = stackalloc uint[1];
            fixed (uint* p = pixel)
                ReadnPixels(lastX, Height - lastY, 1, 1, Const.RED_INTEGER, Const.INT, sizeof(uint), p);
            var tri = pixel[0] / 3;
            if (tri != lastTriangle) {
                Debug.WriteLine(tri);
                lastTriangle = tri;
            }
            lastX = -1;
        }

        State.Framebuffer = 0;
        Viewport(0, 0, Width, Height);
        Clear(BufferBit.Color | BufferBit.Depth);
        State.Program = PassThrough.Id;
        State.VertexArray = quad;
        State.DepthTest = false;
        State.CullFace = false;
        PassThrough.Tex(0);
        DrawArrays(Primitive.Triangles, 0, 6);
    }
}
