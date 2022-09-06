namespace Engine6;

using System;
using System.Numerics;
using Shaders;
using Gl;
using static Gl.Opengl;
using Win32;
using static Common.Maths;
using Common;

class HighlightTriangle:GlWindow {
    public HighlightTriangle (Model model) : base() {
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
    VertexIndex vertexIndex;
    PassThrough passThrough;
    protected override void OnLoad () {
        var size = Rect.Size;

        Pixels = new byte[size.X * size.Y * sizeof(int)];
        fb = new();
        depthStencil = new(size, RenderbufferFormat.Depth24Stencil8);
        fb.Attach(depthStencil, FramebufferAttachment.DepthStencil);
        color0 = new(size, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest };
        fb.Attach(color0, FramebufferAttachment.Color0);
        vertexId = new(size, TextureFormat.R32i) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest };
        fb.Attach(vertexId, FramebufferAttachment.Color1);

        NamedFramebufferDrawBuffers(fb, DrawBuffer.Color0, DrawBuffer.Color1);
        NamedFramebufferReadBuffer(fb, DrawBuffer.Color1);
        vertexIndex = new();
        UseProgram(vertexIndex);

        vao = new();
        var v = new Vector4[Model.Faces.Count * 3];
        for (var (i, j) = (0, 0); j < VertexCount; ++i, ++j) {
            var face = Model.Faces[i];
            v[j] = new((Vector3)Model.Vertices[face.X], 1);
            v[++j] = new((Vector3)Model.Vertices[face.Y], 1);
            v[++j] = new((Vector3)Model.Vertices[face.Z], 1);
        }

        vao.Assign(new VertexBuffer<Vector4>(v), vertexIndex.VertexPosition);
        vertexIndex.Color0(Vector4.One);
        vertexIndex.Color1(new(1, 0, 0, 1));
        vertexIndex.Model(Matrix4x4.CreateTranslation(0, 0, -10));
        vertexIndex.Projection(Matrix4x4.CreatePerspectiveFieldOfView(fPi / 4, (float)size.X / size.Y, 1, 100));
        vertexIndex.View(Matrix4x4.Identity);
        passThrough = new();
        UseProgram(passThrough);
        quad = new();

        quad.Assign(new VertexBuffer<Vector4>(QuadVertices), passThrough.VertexPosition);
        SetSwapInterval(1);
    }

    static readonly Vector4[] QuadVertices = {
        new(-1f, -1f, 0, 1),
        new(+1f, -1f, 0, 1),
        new(+1f, +1f, 0, 1),
        new(-1f, -1f, 0, 1),
        new(+1f, +1f, 0, 1),
        new(-1f, +1f, 0, 1),
    };

    protected override void OnKeyUp (Key k) {
        switch (k) {
            case Key.Up:
                fovRatio = IntMin(fovRatio + 1, 6);
                return;
            case Key.Down:
                fovRatio = IntMax(fovRatio - 1, 2);
                return;
        }
    }

    int fovRatio = 4;
    uint lastTriangle = 0;
    protected override void Render () {
        BindFramebuffer(fb);
        color0.BindTo(0);
        vertexId.BindTo(1);
        Viewport(new(), Rect.Size);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        UseProgram(vertexIndex);
        BindVertexArray(vao);
        Enable(Capability.DepthTest);
        DepthFunc(DepthFunction.LessEqual);
        Enable(Capability.CullFace);
        vertexIndex.Tri((int)lastTriangle);
        vertexIndex.Projection(Matrix4x4.CreatePerspectiveFieldOfView(fPi / fovRatio, (float)Rect.Width / Rect.Height, 1, 100));
        DrawArrays(Primitive.Triangles, 0, VertexCount);

        if (0 <= CursorLocation.X && CursorLocation.X < Rect.Width && 0 <= CursorLocation.Y && CursorLocation.Y < Rect.Height) {
            ReadOnePixel(CursorLocation.X, CursorLocation.Y, 1, 1, out var p);
            lastTriangle = p / 3;
        }

        BindDefaultFramebuffer();
        Viewport(Vector2i.Zero, Rect.Size);
        Clear(BufferBit.ColorDepth);
        UseProgram(passThrough);
        BindVertexArray(quad);
        Disable(Capability.DepthTest);
        Disable(Capability.CullFace);
        passThrough.Tex(0);
        DrawArrays(Primitive.Triangles, 0, 6);
    }
}
