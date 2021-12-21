namespace Engine;

using System;
using System.Numerics;
using Shaders;
using Gl;
using static Gl.Opengl;
using static Gl.Utilities;
using Win32;

class BlitTest:GlWindow {
    public BlitTest (Vector2i size) : base(size) { }
    private Camera Camera { get; } = new(new(0, 0, 5));
    private Raster raster;
    private Sampler2D sampler;
    private VertexArray quad;
    private VertexBuffer<Vector4> quadBuffer;
    protected override void Load () {
        quad = new();
        State.Program = PassThrough.Id;
        quadBuffer = new(Quad.Vertices);
        quad.Assign(quadBuffer, PassThrough.VertexPosition);
        raster = new(new(Width >> 2, Height >> 2), 4, 1);
        MemSet(raster.Pixels, 0xff00000fu);
        for (var i = 0; i < raster.Height; i++) {
            var p = i * raster.Stride + i * raster.Channels * raster.BytesPerChannel;
            raster.Pixels[p] = byte.MaxValue;
            raster.Pixels[++p] = byte.MaxValue;
            raster.Pixels[++p] = byte.MaxValue;
            raster.Pixels[++p] = byte.MaxValue;
        }
        sampler = new(raster.Size, TextureFormat.Rgba8);
        sampler.Mag = MagFilter.Nearest;
        sampler.Min = MinFilter.Nearest;
        sampler.Wrap = Wrap.ClampToEdge;
    }
    protected override void KeyDown (Keys k) {
        base.KeyDown(k);
    }

    protected override void KeyUp (Keys k) {
        base.KeyDown(k);
    }

    protected override void Render (float dt) {
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
