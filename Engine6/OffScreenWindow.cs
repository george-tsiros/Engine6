namespace Engine;

using Gl;
using static Gl.Opengl;
using Shaders;
using System.Numerics;
using System;
using System.Diagnostics;
using System.Collections.Generic;

public class OffScreenWindow:OffScreenWindowBase {
    private Camera Camera { get; } = new(new(0, 0, 0));
    private Framebuffer fb;
    private Renderbuffer depthBuffer, colorBuffer;
    private VertexArray quad, skyboxVao;
    private Sampler2D tex, skyboxTexture;//, renderTexture;
    private VertexBuffer<Vector4> skyboxBuffer, quadBuffer, cubeBuffer;
    private VertexBuffer<Vector2> skyboxUvBuffer, quadUvBuffer, cubeUvBuffer;
    private VertexBuffer<Matrix4x4> quadModelBuffer, cubeModelBuffer;
    private VertexArray cube;

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

    private static bool Far (Vector3 a, Vector3 b) => Math.Abs(a.X - b.X) > 1 || Math.Abs(a.Y - b.Y) > 1 || Math.Abs(a.Z - b.Z) > 1;
    private static Vector3 RandVector (double d = 100) => new((float)((2 * rand.NextDouble() - 1) * d), (float)((2 * rand.NextDouble() - 1) * d), (float)((2 * rand.NextDouble() - 1) * d));
    private static readonly Random rand = new();

    protected override void Load () {
        quad = new();
        State.Program = SimpleTexture.Id;
        quadBuffer = new(Quad.Vertices);
        quad.Assign(quadBuffer, SimpleTexture.VertexPosition);
        quadUvBuffer = new(Quad.Uv);
        quad.Assign(quadUvBuffer, SimpleTexture.VertexUV);

        var models = new Matrix4x4[] { Matrix4x4.CreateTranslation(.5f, 0, -5), Matrix4x4.CreateTranslation(-.5f, 0, -6), };
        quadModelBuffer = new(models);
        quad.Assign(quadModelBuffer, SimpleTexture.Model, 1);
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

        fb = new Framebuffer();
        depthBuffer = new(new(Width, Height), RenderbufferFormat.Depth32);
        fb.Attach(depthBuffer, FramebufferAttachment.Depth);
        colorBuffer = new(new(Width, Height), RenderbufferFormat.Rgba8);
        fb.Attach(colorBuffer, FramebufferAttachment.Color0);
        Debug.Assert(FramebufferStatus.Complete == fb.CheckStatus(), $"{nameof(fb)} status is {fb.CheckStatus()}");
    }

    protected override void Render () {
        State.Framebuffer = fb;

        glViewport(0, 0, Width, Height);
        glClearColor(0, 0, 0, 1);
        glClear(BufferBit.Color | BufferBit.Depth);
        State.Program = SimpleTexture.Id;
        State.VertexArray = quad;
        State.DepthTest = true;
        State.DepthFunc = DepthFunction.Less;
        State.CullFace = true;
        tex.BindTo(0);
        SimpleTexture.Tex(0);
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
        State.Framebuffer = 0;

        //State.Program = PassThrough.Id;
        //State.VertexArray = quad;
        //State.DepthTest = false;
        //State.CullFace = false;
        //renderTexture.BindTo(1);
        //PassThrough.Tex(1);
        //glDrawArrays(Primitive.Triangles, 0, 6);
    }
}
