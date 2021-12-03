namespace Engine;

using System;
using System.Numerics;
using Shaders;
using Gl;
using static Gl.Opengl;

class NeinCraft:GlWindow {

    public NeinCraft (Predicate<PixelFormatDescriptor> p, int width, int height) : base(p, width, height) { }

    private Camera Camera { get; } = new(new(0, 0, -5));
    private VertexArray skyboxVao, cubeVao;
    private Sampler2D skyboxTexture;
    private VertexBuffer<Vector4> skyboxVertices, cubeVertices;
    private VertexBuffer<Vector2> skyboxUV;

    protected override void Init () {
        skyboxVao = new();
        State.Program = SkyBox.Id;
        skyboxTexture = Sampler2D.FromFile("data/skybox.raw");
        SkyBox.Tex(0);
        skyboxVertices = new(Geometry.Dex(Geometry.Translate(Cube.Vertices, -.5f * Vector3.One), Geometry.FlipWinding(Cube.Indices)));
        skyboxVao.Assign(skyboxVertices, SkyBox.VertexPosition);
        skyboxUV = new(Geometry.Dex(Cube.UvVectors, Geometry.FlipWinding(Cube.UvIndices)));
        skyboxVao.Assign(skyboxUV, SkyBox.VertexUV);

        var projection = Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)Width / Height, 2f, 2000f);
        SkyBox.Projection(projection);

        State.Program = SolidColor.Id;
        cubeVao = new();
        cubeVertices = new(Geometry.Dex(Geometry.Translate(Cube.Vertices, -.5f * Vector3.One), Cube.Indices));
        cubeVao.Assign(cubeVertices, SolidColor.VertexPosition);
        SolidColor.Color(new(1f, 1f, 1f, 1f));
        SolidColor.Projection(projection);
    }

    double theta = 0.0;
    protected override void Render (float dt) {
        Viewport(0, 0, Width, Height);
        Clear(BufferBit.Color | BufferBit.Depth);
        theta += 1.8 * dt;
        if (theta > 2.0 * Math.PI)
            theta -= 2.0 * Math.PI;
        State.DepthTest = true;
        State.DepthFunc = DepthFunction.Less;
        State.Program = SolidColor.Id;
        State.VertexArray = cubeVao;
        SolidColor.View(Camera.LookAtMatrix);
        SolidColor.Model(Matrix4x4.CreateTranslation(new(2f * (float)Math.Sin(theta), 2f * (float)Math.Cos(theta), -3f)));
        DrawArrays(Primitive.Triangles, 0, 36);
        State.Program = SkyBox.Id;
        State.VertexArray = skyboxVao;
        State.DepthFunc = DepthFunction.LessEqual;
        skyboxTexture.BindTo(0);
        SkyBox.View(Camera.RotationOnly);
        DrawArrays(Primitive.Triangles, 0, 36);
        _ = Gdi.SwapBuffers(DeviceContext);
    }

    protected override void Closing () {
        skyboxTexture.Dispose();
        skyboxVao.Dispose();
        skyboxVertices.Dispose();
        skyboxUV.Dispose();
    }
}