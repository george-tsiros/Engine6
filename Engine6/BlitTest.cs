namespace Engine;

using System;
using System.Numerics;
using Shaders;
using Gl;
using static Gl.Opengl;
using static Gl.Utilities;
using Win32;

class DesktopTest:GlWindow {
    public DesktopTest (Vector2i size) : base(size) { }
}

class BlitTest:GlWindow {
    public BlitTest (Vector2i size) : base(size) { }
    private Camera Camera { get; } = new(new(0, 0, 5));
    private Raster raster;
    private Sampler2D sampler;
    private VertexArray quad;
    private VertexBuffer<Vector4> quadBuffer;
    private VertexBuffer<Vector2> quadUvBuffer;
    private VertexBuffer<Matrix4x4> modelBuffer;
    protected override void Load () {
        quad = new();
        State.Program = SimpleTexture.Id;
        quadBuffer = new(Quad.Vertices);
        quadUvBuffer = new(Quad.Uv);
        modelBuffer = new(new Matrix4x4[] { Matrix4x4.Identity });
        quad.Assign(quadBuffer, SimpleTexture.VertexPosition);
        quad.Assign(quadUvBuffer, SimpleTexture.VertexUV);
        quad.Assign(modelBuffer, SimpleTexture.Model, 1);
        raster = new(new(Width, Height), 4, 1);
        MemSet(raster.Pixels, 0xff0000ffu);
        sampler = new(new(Width, Height), TextureFormat.Rgba8);
        sampler.Mag = MagFilter.Linear;
        sampler.Min = MinFilter.Linear;
        sampler.Wrap = Wrap.ClampToEdge;
        SimpleTexture.View(Camera.LookAtMatrix);
        SimpleTexture.Projection(Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)Width / Height, 1f, 100f));
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
        State.Program = SimpleTexture.Id;
        State.VertexArray = quad;
        State.DepthTest = true;
        State.DepthFunc = DepthFunction.Always;
        State.CullFace = true;
        sampler.BindTo(1);
        SimpleTexture.Tex(1);
        SimpleTexture.View(Camera.LookAtMatrix);
        DrawArraysInstanced(Primitive.Triangles, 0, 6, 1);
    }
    protected override void Closing () {
        raster.Dispose();
        sampler.Dispose();
        quad.Dispose();
        quadBuffer.Dispose();
        quadUvBuffer.Dispose();
        modelBuffer.Dispose();
    }
}
