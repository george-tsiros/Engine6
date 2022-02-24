namespace Engine;

using Gl;
using static Gl.Opengl;
using Shaders;
using System.Numerics;
using System;

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
