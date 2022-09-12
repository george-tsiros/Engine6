namespace Engine6;

using System;
using System.Numerics;
using Shaders;
using Gl;
using static Gl.Opengl;
using Win32;
using static Common.Maths;
using Common;

class HighlightTriangle:GlWindowArb {
    public HighlightTriangle (Model model) : base() {
        Model = model;
        VertexCount = Model.Faces.Count * 3;
        Load += OnLoad;
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

    void OnLoad (object sender, EventArgs _) {
        var size = ClientSize;

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

    void OnKeyUp (object sender, KeyEventArgs args) {
        switch (args.Key) {
            case Key.Up:
                fovRatio = IntMin(fovRatio + 1, 6);
                return;
            case Key.Down:
                fovRatio = IntMax(fovRatio - 1, 2);
                return;
            case Key.Escape:
                User32.PostQuitMessage(0);
                return;
        }
    }

    int fovRatio = 4;
    uint lastTriangle = 0;
    protected override void Render () {
        BindFramebuffer(fb);
        color0.BindTo(0);
        vertexId.BindTo(1);
        Viewport(new(), ClientSize);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        UseProgram(vertexIndex);
        BindVertexArray(vao);
        Enable(Capability.DepthTest);
        DepthFunc(DepthFunction.LessEqual);
        Enable(Capability.CullFace);
        vertexIndex.Tri((int)lastTriangle);
        vertexIndex.Projection(Matrix4x4.CreatePerspectiveFieldOfView(fPi / fovRatio, (float)ClientSize.X / ClientSize.Y, 1, 100));
        DrawArrays(Primitive.Triangles, 0, VertexCount);

        if (0 <= CursorLocation.X && CursorLocation.X < ClientSize.X && 0 <= CursorLocation.Y && CursorLocation.Y < ClientSize.Y) {
            ReadOnePixel(CursorLocation.X, CursorLocation.Y, 1, 1, out var p);
            lastTriangle = p / 3;
        }

        BindDefaultFramebuffer();
        Viewport(Vector2i.Zero, ClientSize);
        Clear(BufferBit.ColorDepth);
        UseProgram(passThrough);
        BindVertexArray(quad);
        Disable(Capability.DepthTest);
        Disable(Capability.CullFace);
        passThrough.Tex(0);
        DrawArrays(Primitive.Triangles, 0, 6);
    }
}
