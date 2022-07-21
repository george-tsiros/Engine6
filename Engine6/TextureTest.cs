namespace Engine;

using System;
using System.IO;
using System.Numerics;
using Shaders;
using Gl;
using static Gl.Opengl;
using Win32;
using System.Diagnostics;
using System.Collections.Generic;

class TextureTest:GlWindow {
    [Flags]
    enum Dir {
        None = 0,
        Up = 1,
        Right = 2,
        Down = 4,
        Left = 8,
    }                                               // L D R U
    static readonly Dir[] dirs = {                  // 8 4 2 1
        Dir.None,                                   // 0 0 0 0
        Dir.Up,                                     // 0 0 0 1
        Dir.Right,                                  // 0 0 1 0
        Dir.Up | Dir.Right,                         // 0 0 1 1
        Dir.Down,                                   // 0 1 0 0
        Dir.None,                                   // 0 1 0 1
        Dir.Down | Dir.Right,                       // 0 1 1 0
        Dir.Right,                                  // 0 1 1 1
        Dir.Left,                                   // 1 0 0 0
        Dir.Up | Dir.Left,                          // 1 0 0 1
        Dir.None,                                   // 1 0 1 0
        Dir.Up,                                     // 1 0 1 1
        Dir.Down | Dir.Left,                        // 1 1 0 0
        Dir.Left,                                   // 1 1 0 1
        Dir.Down,                                   // 1 1 1 0
        Dir.None,                                   // 1 1 1 1
    };
    public TextureTest (Vector2i size) : base(size) { }

    private Camera Camera { get; } = new(new(0, 0, 0));
    private VertexArray quad, skyboxVao;
    private Sampler2D tex, skyboxTexture;
    private VertexBuffer<Vector4> skyboxBuffer, quadBuffer, cubeBuffer;
    private VertexBuffer<Vector2> skyboxUvBuffer, quadUvBuffer, cubeUvBuffer;
    private VertexBuffer<Matrix4x4> quadModelBuffer, cubeModelBuffer;
    private VertexArray cube;

    protected unsafe override void Load () {
        quad = new();
        State.Program = SimpleTexture.Id;
        quadBuffer = new(Quad.Vertices);
        quad.Assign(quadBuffer, SimpleTexture.VertexPosition);
        quadUvBuffer = new(Quad.Uv);
        quad.Assign(quadUvBuffer, SimpleTexture.VertexUV);

        var models = new Matrix4x4[] { Matrix4x4.CreateTranslation(.5f, 0, -5), Matrix4x4.CreateTranslation(-.5f, 0, -6), };
        quadModelBuffer = new(models);
        quad.Assign(quadModelBuffer, SimpleTexture.Model, 1);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(float.Pi / 4, (float)Width / Height, 0.1f, 1000f);
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
        skyboxBuffer = new(Geometry.Dex(Geometry.Translate(Cube.Vertices, -.5f * Vector3.One), Geometry.FlipWinding(Cube.Indices)));
        skyboxVao.Assign(skyboxBuffer, SkyBox.VertexPosition);
        skyboxUvBuffer = new(Geometry.Dex(Cube.UvVectors, Geometry.FlipWinding(Cube.UvIndices)));
        skyboxVao.Assign(skyboxUvBuffer, SkyBox.VertexUV);
        SkyBox.Projection(projection);

        cube = new();
        State.Program = SimpleTexture.Id;
        cubeBuffer = new(Geometry.Dex(Geometry.Translate(Cube.Vertices, -.5f * Vector3.One), Cube.Indices));
        cube.Assign(cubeBuffer, SimpleTexture.VertexPosition);
        cubeUvBuffer = new(Geometry.Dex(Cube.UvVectors, Cube.UvIndices));
        cube.Assign(cubeUvBuffer, SimpleTexture.VertexUV);

        var positions = new List<Vector3>(CubeCount);
        for (var i = 0; i < CubeCount; i++)
            EmptyRandomPosition(positions);
        var cubes = new Matrix4x4[CubeCount];
        for (var i = 0; i < CubeCount; i++)
            cubes[i] = Matrix4x4.CreateTranslation(positions[i]);
        cubeModelBuffer = new(cubes);
        cube.Assign(cubeModelBuffer, SimpleTexture.Model, 1);
    }
    const int CubeCount = 10000;
    private static readonly Random rand = new();
    private static Vector3 RandVector (double d = 100) => new((float)((2 * rand.NextDouble() - 1) * d), (float)((2 * rand.NextDouble() - 1) * d), (float)((2 * rand.NextDouble() - 1) * d));
    private static void EmptyRandomPosition (List<Vector3> positions) {
        for (; ; ) {
            var v = RandVector(100);
            if (positions.TrueForAll(e => Far(e, v))) {
                positions.Add(v);
                return;
            }
        }
    }
    private static bool Far (Vector3 a, Vector3 b) => float.Abs(a.X - b.X) > 1 || float.Abs(a.Y - b.Y) > 1 || float.Abs(a.Z - b.Z) > 1;

    private static readonly Keys[] keys = { Keys.D, Keys.C, Keys.X, Keys.Z };
    private Dir keyState = Dir.None;
    protected override void KeyUp (Keys k) {
        if (!KeyAction(k, false))
            base.KeyUp(k);
    }
    private bool KeyAction (Keys k, bool down) {
        var i = Array.IndexOf(keys, k);
        if (i < 0)
            return false;
        var d = (Dir)(1 << i);
        keyState = down ? keyState | d : keyState & ~d;
        return true;
    }
    protected override void KeyDown (Keys k) {
        if (!KeyAction(k, true))
            base.KeyDown(k);
    }
    //private int previousX, previousY;
    protected override void MouseMove (int x, int y) {
        Camera.Rotate(new(x * 0.01f, y * 0.01f));
    }

    protected override void Render (float dt) {
        Viewport(0, 0, Width, Height);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.Color | BufferBit.Depth);
        State.Framebuffer = 0;
        State.Program = SimpleTexture.Id;
        State.VertexArray = quad;
        State.DepthTest = true;
        State.DepthFunc = DepthFunction.Less;
        State.CullFace = true;
        tex.BindTo(1);
        SimpleTexture.Tex(1);
        SimpleTexture.View(Camera.LookAtMatrix);
        DrawArraysInstanced(Primitive.Triangles, 0, 6, 2);

        State.VertexArray = cube;
        DrawArraysInstanced(Primitive.Triangles, 0, 36, CubeCount);

        State.Program = SkyBox.Id;
        State.VertexArray = skyboxVao;
        State.DepthFunc = DepthFunction.LessEqual;
        skyboxTexture.BindTo(0);
        SkyBox.Tex(0);
        SkyBox.View(Camera.RotationOnly);
        DrawArrays(Primitive.Triangles, 0, 36);
    }
}
