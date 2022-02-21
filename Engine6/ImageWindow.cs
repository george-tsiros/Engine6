namespace Engine;

using Gl;
using static Gl.Opengl;
using Shaders;
using System.Numerics;

public class ImageWindow:GlWindow {
    private Raster Image { get; }
    private Sampler2D sampler;
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

        sampler = new(Image.Size, TextureFormat.R8);
        sampler.Mag = MagFilter.Nearest;
        sampler.Min = MinFilter.Nearest;
        sampler.Wrap = Wrap.ClampToEdge;
    }
    protected override void Render (float dt) {
        sampler.Upload(Image);
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
        Image.Dispose();
        sampler.Dispose();
        quad.Dispose();
        quadBuffer.Dispose();
    }
}
