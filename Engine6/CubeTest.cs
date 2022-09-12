namespace Engine6;
using static Common.Maths;
using Win32;
using Gl;
using Shaders;
using System.Numerics;
using static Gl.Opengl;
using Common;

class CubeTest:GlWindowArb {

    private ICamera camera = new QCamera(new(0, 0, 10));
    private float speed = 0f;
    private Axes axes;
    private VertexArray vertexArray;

    static readonly Vector4[] FaceColors = {
        new(0, 0, 1, 1),
        new(0, 0, .5f, 1),
        new(0, .5f, 0, 1),
        new(0, 1, 0, 1),
        new(1, 0, 0, 1),
        new(.5f, 0, 0, 1),
    };

    protected override void OnLoad () {
        ClientSize = new(1280, 720);
        axes = new();
        UseProgram(axes);
        //axes.View(Matrix4x4.CreateTranslation(0, 0, -5));
        axes.Model(Matrix4x4.Identity);
        vertexArray = new();
        var m = Model.Cube(.5f);
        var vertices = new Vector4[m.Faces.Count * 3];
        var vi = 0;
        foreach (var (i, j, k) in m.Faces) {
            vertices[vi++] = new(m.Vertices[i], 1);
            vertices[vi++] = new(m.Vertices[j], 1);
            vertices[vi++] = new(m.Vertices[k], 1);
        }
        var colors = new Vector4[vertices.Length];
        for (var (i, o) = (0, 0); i < FaceColors.Length; ++i) {
            var c = FaceColors[i];
            for (var j = 0; j < 6; ++j)
                colors[o++] = c;
        }
        vertexArray.Assign(new VertexBuffer<Vector4>(vertices), axes.VertexPosition);
        vertexArray.Assign(new VertexBuffer<Vector4>(colors), axes.Color);
        Enable(Capability.DepthTest);
        DepthFunc(DepthFunction.LessEqual);
        Enable(Capability.CullFace);
        ClearColor(0, 0, 0, 1);
        Disposables.Add(vertexArray);
        Disposables.Add(axes);
    }

    int Axis (Key plus, Key minus) {
        var x = IsKeyDown(plus) ? 1 : 0;
        var y = IsKeyDown(minus) ? -1 : 0;
        return x + y;
    }

    void Move () {
        var pitch = Axis(Key.A, Key.Z);
        var yaw = Axis(Key.S, Key.X);
        var roll = Axis(Key.D, Key.C);
        camera.Rotate(.1f * pitch, 0, 0);
        camera.Rotate(0, .1f * yaw, 0);
        camera.Rotate(0, 0, .1f * roll);
    }

    protected override void Render () {
        Move();
        var size = ClientSize;
        Viewport(new(), size);
        Clear(BufferBit.ColorDepth);
        BindVertexArray(vertexArray);
        axes.Projection(Matrix4x4.CreatePerspectiveFieldOfView(fPi / 2, (float)size.X / size.Y, .1f, 100f));
        var q = Quaternion.CreateFromAxisAngle(Vector3.UnitX, fPi / 12);
        //axes.View(Matrix4x4.CreateTranslation(0, 0, -10) * Matrix4x4.CreateFromQuaternion(q ));
        //axes.View(Matrix4x4.CreateTranslation(0, 0, -10) * Matrix4x4.CreateRotationX(fPi / 6));
        //axes.View(Matrix4x4.CreateLookAt(new(0, 0, 10), Vector3.Zero, Vector3.UnitY));
        //axes.Model(Matrix4x4.CreateFromQuaternion(q) * Matrix4x4.CreateTranslation(location));
        axes.View(camera.LookAtMatrix);
        DrawArrays(Primitive.Triangles, 0, 36);
    }
}
