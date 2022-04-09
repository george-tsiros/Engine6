namespace Engine;

using System;
using System.Numerics;
using Shaders;
using Gl;
using static Gl.Opengl;
using Win32;
using System.Diagnostics;

class Teapot:GlWindow {
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

        //NamedFramebufferDrawBuffer(fb, DrawBuffer.Color1);
        //NamedFramebufferDrawBuffer(fb, DrawBuffer.Color0);
        NamedFramebufferDrawBuffers(fb, DrawBuffer.Color0, DrawBuffer.Color1);
        NamedFramebufferReadBuffer(fb, DrawBuffer.Color1);
        State.Program = VertexIndex.Id;

        vao = new();
        var v = new Vector4[Model.Faces.Count * 3];
        for (var (i, j) = (0, 0); j < VertexCount; ++i, ++j) {
            var (a, b, c) = Model.Faces[i];
            v[j] = new(Model.Vertices[a], 1);
            v[++j] = new(Model.Vertices[b], 1);
            v[++j] = new(Model.Vertices[c], 1);
        }

        vao.Assign(new VertexBuffer<Vector4>(v), VertexIndex.VertexPosition);
        VertexIndex.Color(Vector4.One);

        VertexIndex.Model(Matrix4x4.CreateTranslation(0, 0, -10));
        VertexIndex.Projection(Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)Width / Height, 1, 100));
        VertexIndex.View(Matrix4x4.Identity);

        State.Program = PassThrough.Id;
        quad = new();
        quad.Assign(new VertexBuffer<Vector4>(Quad.Vertices), PassThrough.VertexPosition);
        Disposables.Add(quad);
        Disposables.Add(vao);
        Disposables.Add(fb);
        Disposables.Add(depthStencil);
        Disposables.Add(vertexId);
        Disposables.Add(color0);
        Disposables.Add(quad);
    }

    //static void WriteBmpHeader (System.IO.Stream stream, int width, int height) {
    //    stream.WriteByte((byte)'B');
    //    stream.WriteByte((byte)'M'); // 1
    //    var length = 0x36 + 4 * width * height;
    //    var zero = BitConverter.GetBytes(0);
    //    stream.Write(BitConverter.GetBytes(length), 0, sizeof(int)); // 5
    //    stream.Write(zero, 0, sizeof(int)); // 9
    //    stream.Write(BitConverter.GetBytes(0x36), 0, sizeof(int)); // 13
    //    stream.Write(BitConverter.GetBytes(0x28), 0, sizeof(int)); // 17
    //    stream.Write(BitConverter.GetBytes(width), 0, sizeof(int)); // 21
    //    stream.Write(BitConverter.GetBytes(height), 0, sizeof(int)); // 25
    //    stream.Write(BitConverter.GetBytes(0x200001), 0, sizeof(int)); // 29
    //    stream.Write(zero, 0, sizeof(int)); // 33
    //    stream.Write(zero, 0, sizeof(int)); // 37
    //    var ec4 = BitConverter.GetBytes(0xec4);
    //    stream.Write(ec4, 0, sizeof(int)); // 
    //    stream.Write(ec4, 0, sizeof(int)); // 
    //    stream.Write(zero, 0, sizeof(int));
    //    stream.Write(zero, 0, sizeof(int));
    //    //stream.Write(zero, 0, 3);
    //}

    //static void WriteBmp ((byte r, byte g, byte b, byte a)[] colors, int width, int height, byte[] pixels) {
    //    using var f = System.IO.File.Create("bmp.bmp");
    //    using var w = new System.IO.BinaryWriter(f);
    //    w.Write((byte)0x42);
    //    w.Write((byte)0x4d);
    //    var filesize = 0x36 + 256 * 4 + width * height;
    //    w.Write(filesize);
    //    w.Write(0);
    //    w.Write(0x436);
    //    w.Write(0x28);
    //    w.Write(width);
    //    w.Write(height);
    //    w.Write((byte)1);
    //    w.Write((byte)0);
    //    w.Write((byte)8);
    //    w.Write((byte)0);
    //    w.Write(0);
    //    w.Write(0);
    //    w.Write(0xec4);
    //    w.Write(0xec4);
    //    w.Write(0x100);
    //    w.Write(0x100);
    //    foreach (var (r, g, b, a) in colors) {
    //        w.Write(r);
    //        w.Write(g);
    //        w.Write(b);
    //        w.Write(a);
    //    }
    //    w.Write(pixels);
    //}

    protected override void MouseMove (int x, int y) => (lastX, lastY) = (x, y);

    int lastX, lastY;

    unsafe protected override void Render (float dt) {
        State.Framebuffer = fb;
        color0.BindTo(0);
        vertexId.BindTo(1);
        glViewport(0, 0, Width, Height);
        glClear(BufferBit.Color | BufferBit.Depth);
        State.Program = VertexIndex.Id;
        State.VertexArray = vao;
        State.DepthTest = true;
        State.DepthFunc = DepthFunction.LessEqual;
        State.CullFace = true;
        glDrawArrays(Primitive.Triangles, 0, VertexCount);
        
        if (lastX >= 0) {
            Span<uint> pixel = stackalloc uint[1];
            fixed (uint* p = pixel)
                ReadnPixels(lastX, lastY, 1, 1, Const.RED_INTEGER, Const.INT, sizeof(uint), p);
            Debug.WriteLine(pixel[0]);
            lastX = -1;
        }

        State.Framebuffer = 0;
        glViewport(0, 0, Width, Height);
        glClear(BufferBit.Color | BufferBit.Depth);
        State.Program = PassThrough.Id;
        State.VertexArray = quad;
        State.DepthTest = false;
        State.CullFace = false;
        PassThrough.Tex(0);
        glDrawArrays(Primitive.Triangles, 0, 6);
    }
}
