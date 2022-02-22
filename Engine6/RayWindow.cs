namespace Engine;

using Gl;
using static Gl.Opengl;
using Shaders;
using System;
using System.Numerics;
using System.Threading;

public class RayWindow:GlWindow {
    public RayWindow (Model model, Vector2i imageSize, int parallelism = 1) : base(imageSize) {
        Parallelism = parallelism > 0 ? parallelism : throw new ArgumentOutOfRangeException(nameof(parallelism));
        Model = model ?? throw new ArgumentNullException(nameof(model));
        const float yFov = (float)(Math.PI / 4);
        Fov = new(yFov * Width / Height, yFov);
    }

    private readonly Vector2 Fov;
    private readonly Model Model;
    private readonly int Parallelism;
    private Raster Image { get; }
    private Sampler2D sampler;
    private VertexArray quad;
    private VertexBuffer<Vector4> quadBuffer;

    protected override void Load () {
        quad = new();
        State.Program = PassThrough.Id;
        quadBuffer = new(Quad.Vertices);
        quad.Assign(quadBuffer, PassThrough.VertexPosition);

        sampler = new(Image.Size, TextureFormat.R8);
        sampler.Mag = MagFilter.Nearest;
        sampler.Min = MinFilter.Nearest;
        sampler.Wrap = Wrap.ClampToEdge;
        var faceCount = Model.Faces.Count;
        var vectors = Model.Vertices.ToArray();
        Geometry.Transform(vectors, Matrix4x4.CreateTranslation(0, 0, -5));
        var triangles = new (Vector3, Vector3, Vector3)[faceCount];

        for (int i = 0; i < faceCount; i++) {
            var f = Model.Faces[i];
            triangles[i] = (vectors[f.i], vectors[f.j], vectors[f.k]);
        }

    }

    protected override void Render (float dt) {

        sampler.Upload(Image);
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
        Image.Dispose();
        sampler.Dispose();
        quad.Dispose();
        quadBuffer.Dispose();
    }

}
