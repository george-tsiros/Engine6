namespace Engine6;
using static Common.Maths;
using Win32;
using Gl;
using Shaders;
using System.Numerics;
using static Gl.Opengl;

class CubeTest:GlWindowArb {

    protected override void OnInput (int dx, int dy) {
        if (0 != dx)
            q *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -.001f * dx);

        if (0 != dy)
            q *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, .001f * dy);
    }

    private float speed = 0f;
    private Vector3 location = Vector3.Zero;
    private Quaternion q = Quaternion.Identity;
    private Axes axes;
    private VertexArray vertexArray;
    private VertexBuffer<Vector4> vb, cb;
    protected override void OnIdle () =>
        Invalidate();

    static readonly Vector4[] FaceColors = {
        new(0, 0, 1, 1), // near
        new(0, 0, .5f, 1), // far
        new(0, .5f, 0, 1), // bottom
        new(0, 1, 0, 1), // top
        new(1, 0, 0, 1),
        new(.5f, 0, 0, 1),
    };

    protected override void OnLoad () {
        var size = ClientSize = new(1280, 720);
        axes = new();
        UseProgram(axes);
        axes.Projection(Matrix4x4.CreatePerspectiveFieldOfView(fPi / 3, (float)size.X / size.Y, .1f, 100f));
        axes.View(Matrix4x4.CreateTranslation(0, 0, -5));
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
        vb = new VertexBuffer<Vector4>(vertices);
        vertexArray.Assign(vb, axes.VertexPosition);
        cb = new VertexBuffer<Vector4>(colors);
        vertexArray.Assign(cb, axes.Color);
        Enable(Capability.DepthTest);
        DepthFunc(DepthFunction.LessEqual);
        Enable(Capability.CullFace);
        Disposables.Add(vertexArray);
        Disposables.Add(axes);
        Disposables.Add(vb);
        Disposables.Add(cb);
    }

    int Axis (Key plus, Key minus) {
        var x = IsKeyDown(plus) ? 1 : 0;
        var y = IsKeyDown(minus) ? -1 : 0;
        return x + y;
    }

    void Move () {
        var yaw = Axis(Key.S, Key.F);
        if (0 != yaw)
            q *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, .01f * yaw);
        var dv = Axis(Key.D, Key.X);
        if (0 != dv)
            speed = FloatClamp(speed + .001f * dv, -.01f, .01f);
        if (0 != speed)
            location += speed * Vector3.Transform(-Vector3.UnitZ, q);
    }

    protected override void Render () {
        Move();
        Viewport(new(), ClientSize);
        UseProgram(axes);
        BindVertexArray(vertexArray);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        axes.Model(Matrix4x4.CreateFromQuaternion(q) * Matrix4x4.CreateTranslation(location));
        DrawArrays(Primitive.Triangles, 0, 36);
    }
}
