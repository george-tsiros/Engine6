namespace Engine;

using Gl;
using static Gl.Opengl;
using Shaders;
using System.Numerics;
using System;
using System.Diagnostics;

public class OffScreenWindow:OffScreenWindowBase {
    private Framebuffer fb;
    private Renderbuffer rb;
    private Sampler2D tex;
    private VertexArray cube, quad;
    private VertexBuffer<Vector4> quadBuffer, cubeBuffer;

    public OffScreenWindow (Vector2i size) : base(size) { }

    protected override void Load () {
        fb = new Framebuffer();
        Debug.WriteLine(fb.CheckStatus());
        rb = new(RenderbufferFormat.Depth32, new(Width, Height));
        fb.Attach(rb, Attachment.Depth);
        tex = new(new(Width, Height), TextureFormat.Rgb8);
        tex.Mag = MagFilter.Nearest;
        tex.Min = MinFilter.Nearest;
        tex.Wrap = Wrap.ClampToEdge;

        fb.Attach(tex, Attachment.Color0);
        Debug.WriteLine(fb.CheckStatus());

        State.Program = PassThrough.Id;
        PassThrough.Tex(tex);
        
        State.Program = SimpleTexture.Id;
        glViewport(0, 0, Width, Height);
    }

    protected override void Render () {
        State.Framebuffer = fb;
        State.Program = SimpleTexture.Id;

        glClearColor(0, 0, 0, 1);
        glClear(BufferBit.Color | BufferBit.Depth);

        State.Framebuffer = 0;
        State.Program = PassThrough.Id;

        glClearColor(0, 0, 0, 1);
        glClear(BufferBit.Color | BufferBit.Depth);
    }
}


public class ImageWindow:OffScreenWindowBase {
    private readonly Raster Image;
    private Sampler2D Sampler;
    private VertexArray quad;
    private VertexBuffer<Vector4> quadBuffer;
    public ImageWindow (Vector2i size) : base(size) { }
    public ImageWindow (Raster image) : this(image.Size) {
        Image = image;
    }
    protected override void Load () {
        quad = new();
        State.Program = PassThrough.Id;
        quadBuffer = new(Quad.Vertices);
        quad.Assign(quadBuffer, PassThrough.VertexPosition);
        Sampler = new(Image.Size, ImageTextureFormat(Image.Channels));
        Sampler.Mag = MagFilter.Nearest;
        Sampler.Min = MinFilter.Nearest;
        Sampler.Wrap = Wrap.ClampToEdge;
        Sampler.Upload(Image);
    }

    private static TextureFormat ImageTextureFormat (int channels) => channels switch {
        1 => TextureFormat.R8,
        2 => TextureFormat.Rg8,
        3 => TextureFormat.Rgb8,
        _ => throw new Exception($"{channels} invalid channel count")
    };

    protected override void Render () {
        glViewport(0, 0, Width, Height);
        glClear(BufferBit.Color | BufferBit.Depth);
        State.Program = PassThrough.Id;
        State.VertexArray = quad;
        State.DepthTest = true;
        State.DepthFunc = DepthFunction.Always;
        State.CullFace = true;
        Sampler.BindTo(1);
        PassThrough.Tex(1);
        glDrawArrays(Primitive.Triangles, 0, 6);
    }

    protected override void Closing () {
        Image.Dispose();
        Sampler.Dispose();
        quad.Dispose();
        quadBuffer.Dispose();
    }
}
