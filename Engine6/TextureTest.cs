namespace Engine;

using System;
using System.Numerics;
using Shaders;
using Gl;
using static Gl.Opengl;
using Win32;

class TextureTest:GlWindow {

    public TextureTest (Vector2i size) : base(size) { }

    private Camera Camera { get; } = new(new(0, 0, 5));
    private VertexArray quad, skyboxVao;
    private Sampler2D tex, skyboxTexture;
    private VertexBuffer<Vector4> skyboxBuffer, quadBuffer;
    private VertexBuffer<Vector2> skyboxUvBuffer, quadUvBuffer;
    private VertexBuffer<Matrix4x4> quadModelBuffer;

    protected override void Closing () {
        quad.Dispose();
        skyboxVao.Dispose();
        skyboxBuffer.Dispose();
        skyboxUvBuffer.Dispose();
        quadBuffer.Dispose();
        quadModelBuffer.Dispose();
        quadUvBuffer.Dispose();
    }

    protected unsafe override void Load () {
        quad = new();
        State.Program = SimpleTexture.Id;
        quadBuffer = new VertexBuffer<Vector4>(Quad.Vertices);
        quad.Assign(quadBuffer, SimpleTexture.VertexPosition);
        quadUvBuffer = new VertexBuffer<Vector2>(Quad.Uv);
        quad.Assign(quadUvBuffer, SimpleTexture.VertexUV);
        var models = new Matrix4x4[] { Matrix4x4.Identity };
        quadModelBuffer = new VertexBuffer<Matrix4x4>(models);
        quad.Assign(quadModelBuffer, SimpleTexture.Model, 1);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)Width / Height, 1f, 10f);
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
        skyboxBuffer = new(Geometry.Dex(Geometry.ScaleInPlace(Geometry.Translate(Cube.Vertices, -.5f * Vector3.One), 10f), Geometry.FlipWinding(Cube.Indices)));
        skyboxVao.Assign(skyboxBuffer, SkyBox.VertexPosition);
        skyboxUvBuffer = new(Geometry.Dex(Cube.UvVectors, Geometry.FlipWinding(Cube.UvIndices)));
        skyboxVao.Assign(skyboxUvBuffer, SkyBox.VertexUV);
        SkyBox.Projection(projection);
    }
    private DepthFunction[] DepthFunctionValues;
    protected override void KeyDown (Keys k) {
        switch (k) {
            case Keys.Space:
                DepthFunctionValues ??= Enum.GetValues<DepthFunction>();
                var i = Array.IndexOf(DepthFunctionValues, selectedDepthFunction);
                if (--i < 0)
                    i = DepthFunctionValues.Length - 1;
                selectedDepthFunction = DepthFunctionValues[i];
                SetText(selectedDepthFunction.ToString());
                return;
            case Keys.Up:
                State.SwapInterval = Math.Min(1, State.SwapInterval + 1);
                SetText(State.SwapInterval.ToString());
                return;
            case Keys.Down:
                State.SwapInterval = Math.Max(-1, State.SwapInterval - 1);
                SetText(State.SwapInterval.ToString());
                return;
        }
        base.KeyDown(k);
    }
    private DepthFunction selectedDepthFunction = DepthFunction.Equal;
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
        State.DepthFunc = selectedDepthFunction;
        skyboxTexture.BindTo(0);
        SkyBox.Tex(0);
        SkyBox.View(Camera.RotationOnly);
        DrawArrays(Primitive.Triangles, 0, 36);
    }
}