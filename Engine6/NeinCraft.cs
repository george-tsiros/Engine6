namespace Engine;

using System;
using System.Numerics;
using Shaders;
using Gl;
using Win32;
using static Gl.Opengl;

class NeinCraft:GlWindow {

    public NeinCraft (Vector2i size) : base(size) { }

    private Camera Camera { get; } = new(new(0, 0, -5));
    private VertexArray skyboxVao, cubeVao;
    private Sampler2D skyboxTexture;
    private VertexBuffer<Vector4> skyboxVertices, cubeVertices;
    private VertexBuffer<Vector2> skyboxUV;

    protected override void Load () {
        skyboxVao = new();
        State.Program = SkyBox.Id;
        skyboxTexture = Sampler2D.FromFile("data/skybox.raw");
        SkyBox.Tex(0);
        skyboxVertices = new(Geometry.Dex(Geometry.Translate(Cube.Vertices, -.5f * Vector3.One), Geometry.FlipWinding(Cube.Indices)));
        skyboxVao.Assign(skyboxVertices, SkyBox.VertexPosition);
        skyboxUV = new(Geometry.Dex(Cube.UvVectors, Geometry.FlipWinding(Cube.UvIndices)));
        skyboxVao.Assign(skyboxUV, SkyBox.VertexUV);

        var projection = Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)Width / Height, 0.1f, 100f);
        SkyBox.Projection(projection);

        State.Program = SolidColor.Id;
        cubeVao = new();
        cubeVertices = new(Geometry.Dex(Geometry.Translate(Cube.Vertices, -.5f * Vector3.One), Cube.Indices));
        cubeVao.Assign(cubeVertices, SolidColor.VertexPosition);
        SolidColor.Color(new(1f, 1f, 1f, 1f));
        SolidColor.Projection(projection);
        SolidColor.Model(Matrix4x4.CreateTranslation(0,0,0));

        Disposables.Add(skyboxTexture);
        Disposables.Add(skyboxVao);
        Disposables.Add(skyboxVertices);
        Disposables.Add(skyboxUV);
    }
    protected override void Render (float dt) {
        Viewport(0, 0, Width, Height);
        Clear(BufferBit.Color | BufferBit.Depth);
        State.DepthTest = true;
        State.DepthFunc = DepthFunction.Less;
        State.Program = SolidColor.Id;
        State.VertexArray = cubeVao;
        SolidColor.View(Camera.LookAtMatrix);
        DrawArrays(Primitive.Triangles, 0, 36);
        State.Program = SkyBox.Id;
        State.VertexArray = skyboxVao;
        State.DepthFunc = DepthFunction.LessEqual;
        skyboxTexture.BindTo(0);
        SkyBox.View(Camera.RotationOnly);
        DrawArrays(Primitive.Triangles, 0, 36);
    }
}
