namespace Engine;

using Gl;
using static Gl.Opengl;
using Shaders;
using System.Numerics;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Win32;

public class OffScreenWindow:OffScreenWindowBase {
    private Camera Camera { get; } = new(new(0, 0, 0));
    private VertexArray cube, quad, skyboxVao;
    private Sampler2D tex, skyboxTexture;

    public OffScreenWindow (Vector2i size) : base(size) { }
    const int CubeCount = 1000;
    private static void EmptyRandomPosition (List<Vector3> positions) {
        for (; ; ) {
            var v = RandVector(100);
            if (positions.TrueForAll(e => Far(e, v))) {
                positions.Add(v);
                return;
            }
        }
    }
    bool useFB;
    protected override void KeyUp (Keys k) {
        if (k == Keys.Space)
            useFB = !useFB;
    }

    private static bool Far (Vector3 a, Vector3 b) => Math.Abs(a.X - b.X) > 1 || Math.Abs(a.Y - b.Y) > 1 || Math.Abs(a.Z - b.Z) > 1;
    private static Vector3 RandVector (double d = 100) => new(rand.NextFloat(-d, d), rand.NextFloat(-d, d), rand.NextFloat(-d, d));
    private static readonly Random rand = new();

    protected override void Load () {
        quad = new();
        State.Program = SimpleTexture.Id;
        quad.Assign(new VertexBuffer<Vector4>(Quad.Vertices), SimpleTexture.VertexPosition);
        quad.Assign(new VertexBuffer<Vector2>(Quad.Uv), SimpleTexture.VertexUV);

        var models = new Matrix4x4[] { Matrix4x4.CreateTranslation(.5f, 0, -5), Matrix4x4.CreateTranslation(-.5f, 0, -6), };
        quad.Assign(new VertexBuffer<Matrix4x4>(models), SimpleTexture.Model, 1);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)Width / Height, 0.1f, 1000f);
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
        var centeredCube = Geometry.Translate(Cube.Vertices, -.5f * Vector3.One);
        skyboxVao.Assign(new VertexBuffer<Vector4>(Geometry.Dex(centeredCube, Geometry.FlipWinding(Cube.Indices))), SkyBox.VertexPosition);
        skyboxVao.Assign(new VertexBuffer<Vector2>(Geometry.Dex(Cube.UvVectors, Geometry.FlipWinding(Cube.UvIndices))), SkyBox.VertexUV);
        SkyBox.Projection(projection);

        cube = new();
        State.Program = SimpleTexture.Id;
        cube.Assign(new VertexBuffer<Vector4>(Geometry.Dex(centeredCube, Cube.Indices)), SimpleTexture.VertexPosition);
        cube.Assign(new VertexBuffer<Vector2>(Geometry.Dex(Cube.UvVectors, Cube.UvIndices)), SimpleTexture.VertexUV);

        var positions = new List<Vector3>(CubeCount);
        for (var i = 0; i < CubeCount; i++)
            EmptyRandomPosition(positions);
        var cubes = new Matrix4x4[CubeCount];
        for (var i = 0; i < CubeCount; i++)
            cubes[i] = Matrix4x4.CreateTranslation(positions[i]);
        cube.Assign(new VertexBuffer<Matrix4x4>(cubes), SimpleTexture.Model, 1);
        Disposables.Add(cube);
        Disposables.Add(quad);
        Disposables.Add(skyboxVao);
        Disposables.Add(tex);
        Disposables.Add(skyboxTexture);
    }

    protected override void Render () {
        glViewport(0, 0, Width, Height);
        glClearColor(0, 0, 0, 1);
        glClear(BufferBit.Color | BufferBit.Depth);
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
        glDrawArrays(Primitive.Triangles, 0, 36);
    }
}
