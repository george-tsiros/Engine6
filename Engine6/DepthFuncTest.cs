namespace Engine;

using System;
using System.Numerics;
using Shaders;
using Gl;
using static Gl.Opengl;
using Win32;

class DepthFuncTest:GlWindow {
    public DepthFuncTest (Vector2i size) : base(size) { }
    private VertexArray quad;
    private Sampler2D tex;
    private VertexBuffer<Vector4> quadBuffer;
    private VertexBuffer<Vector2> quadUvBuffer;
    private VertexBuffer<Matrix4x4> quadModelBuffer;

    protected override void Load () {
        quad = new();
        State.Program = SimpleTexture.Id;
        quadBuffer = new VertexBuffer<Vector4>(Quad.Vertices);
        quad.Assign(quadBuffer, SimpleTexture.VertexPosition);
        quadUvBuffer = new VertexBuffer<Vector2>(Quad.Uv);
        quad.Assign(quadUvBuffer, SimpleTexture.VertexUV);
        var models = new Matrix4x4[] { Matrix4x4.CreateTranslation(.5f, 0, -5), Matrix4x4.CreateTranslation(-.5f, 0, -6), };
        quadModelBuffer = new VertexBuffer<Matrix4x4>(models);
        quad.Assign(quadModelBuffer, SimpleTexture.Model, 1);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)Width / Height, 1f, 10f);
        SimpleTexture.Projection(projection);

        tex = Sampler2D.FromFile("data/untitled.raw");
        tex.Mag = MagFilter.Linear;
        tex.Min = MinFilter.LinearMipMapLinear;
        tex.Wrap = Wrap.ClampToEdge;
    }
    protected override void KeyDown (Keys k) {
        switch (k) {
            case Keys.F1:
                TextureTest.Cycle(ref selectedDepthFunction);
                return;
        }
        base.KeyDown(k);
    }

    protected override void Render (float dt) {
        if (FramesRendered % 20 == 0)
            TextureTest.Cycle(ref selectedDepthFunction);
        glViewport(0, 0, Width, Height);
        State.DepthTest = true;
        Opengl.glDepthMask(true);
        glClear(BufferBit.Color | BufferBit.Depth);
        State.Framebuffer = 0;
        State.Program = SimpleTexture.Id;
        State.VertexArray = quad;
        State.DepthFunc = selectedDepthFunction;
        State.CullFace = true;
        tex.BindTo(1);
        SimpleTexture.Tex(1);
        SimpleTexture.View(Matrix4x4.Identity);
        DrawArraysInstanced(Primitive.Triangles, 0, 6, 2);
    }
    private DepthFunction selectedDepthFunction = DepthFunction.LessEqual;
}
