namespace Engine;

using System;
using System.Numerics;
using Shaders;
using Gl;
using static Gl.Opengl;
using static Gl.Utilities;
using System.Diagnostics;
using Win32;

class BlitTest:GlWindow {
    public BlitTest (Predicate<PixelFormatDescriptor> p, int width, int height) : base(p, width, height) { }
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
    protected override void Render (float dt) {

        sampler.Upload(raster);
        Viewport(0, 0, Width, Height);
        Clear(BufferBit.Color | BufferBit.Depth);
        State.Program = SimpleTexture.Id;
        State.VertexArray = quad;
        //State.Blend = false;
        State.DepthTest = true;
        State.DepthFunc = DepthFunction.Less;
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

class TextureTest:GlWindow {

    public TextureTest (Predicate<PixelFormatDescriptor> p, int width, int height) : base(p, width, height) { }

    private Camera Camera { get; } = new(new(0, 0, 5));
    private VertexArray quad, skyboxVao;
    private Sampler2D tex, skyboxTexture;
    private VertexBuffer<Vector4> skyboxVertices;
    private VertexBuffer<Vector2> skyboxUV;

    protected unsafe override void Load () {
        quad = new();
        State.Program = SimpleTexture.Id;
        var quadBuffer = new VertexBuffer<Vector4>(Quad.Vertices);
        quad.Assign(quadBuffer, SimpleTexture.VertexPosition);
        quad.Assign(new VertexBuffer<Vector2>(Quad.Uv), SimpleTexture.VertexUV);
        var models = new Matrix4x4[] { Matrix4x4.Identity };
        quad.Assign(new VertexBuffer<Matrix4x4>(models), SimpleTexture.Model, 1);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)Width / Height, 1f, 100f);
        SimpleTexture.Projection(projection);

        tex = Sampler2D.FromFile("data/untitled.raw");
        tex.Mag = MagFilter.Linear;
        tex.Min = MinFilter.LinearMipMapLinear;
        tex.Wrap = Wrap.ClampToEdge;

        State.Program = SkyBox.Id;
        skyboxVao = new();
        skyboxTexture = Sampler2D.FromFile("data/skybox.raw");
        skyboxTexture.Mag = MagFilter.Linear;
        skyboxTexture.Min = MinFilter.Linear;
        skyboxTexture.Wrap = Wrap.ClampToEdge;
        skyboxVertices = new(Geometry.Dex(Geometry.Translate(Cube.Vertices, -.5f * Vector3.One), Geometry.FlipWinding(Cube.Indices)));
        skyboxVao.Assign(skyboxVertices, SkyBox.VertexPosition);
        skyboxUV = new(Geometry.Dex(Cube.UvVectors, Geometry.FlipWinding(Cube.UvIndices)));
        skyboxVao.Assign(skyboxUV, SkyBox.VertexUV);
        SkyBox.Projection(projection);
    }
    protected override void KeyDown (Keys k) {
        if (k == Keys.Space) {
            var values = Enum.GetValues<DepthFunction>();
            var i = Array.IndexOf(values, f);
            if (--i < 0)
                i = values.Length - 1;
            f = values[i];
            Debug.WriteLine(f);
        } else
            base.KeyDown(k);
    }
    private DepthFunction f = DepthFunction.LessEqual;
    protected override void Render (float dt) {
        //Camera.Mouse(new(dt, 0));
        Viewport(0, 0, Width, Height);
        Clear(BufferBit.Color | BufferBit.Depth);
        State.Program = SimpleTexture.Id;
        State.VertexArray = quad;
        //State.Blend = false;
        State.DepthTest = true;
        State.DepthFunc = DepthFunction.Less;
        State.CullFace = true;
        tex.BindTo(1);
        SimpleTexture.Tex(1);
        SimpleTexture.View(Camera.LookAtMatrix);
        DrawArraysInstanced(Primitive.Triangles, 0, 6, 1);

        State.Program = SkyBox.Id;
        State.VertexArray = skyboxVao;
        State.DepthFunc = f;
        skyboxTexture.BindTo(0);
        SkyBox.Tex(0);
        SkyBox.View(Camera.RotationOnly);
        DrawArrays(Primitive.Triangles, 0, 36);
    }
}