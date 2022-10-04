namespace Engine6;
using Win32;
using Common;
using Gl;
using static Gl.GlContext;
using System.Numerics;
using Shaders;

public class MatrixTests:GlWindow {
    private static readonly (int, int)[] C3_lines = { (0, 1), (0, 4), (1, 3), (3, 8), (4, 7), (6, 7), (6, 9), (5, 9), (5, 8), (2, 5), (2, 6), (3, 5), (4, 6), (1, 2), (0, 2), (8, 10), (10, 11), (7, 11), (1, 10), (0, 11), (1, 5), (0, 6), (20, 21), (12, 13), (18, 19), (14, 15), (16, 17), (15, 16), (14, 17), (13, 18), (12, 19), (2, 9), (22, 24), (23, 24), (22, 23), (25, 26), (26, 27), (25, 27), };
    private static readonly Vector3[] C3_vertices = { new(32, 0, -76), new(-32, 0, -76), new(0, 26, -24), new(-120, -3, 8), new(120, -3, 8), new(-88, 16, 40), new(88, 16, 40), new(128, -8, 40), new(-128, -8, 40), new(0, 26, 40), new(-32, -24, 40), new(32, -24, 40), new(-36, 8, 40), new(-8, 12, 40), new(8, 12, 40), new(36, 8, 40), new(36, -12, 40), new(8, -16, 40), new(-8, -16, 40), new(-36, -12, 40), new(0, 0, -76), new(0, 0, -90), new(-80, -6, 40), new(-80, 6, 40), new(-88, 0, 40), new(80, 6, 40), new(88, 0, 40), new(80, -6, 40), };

    public MatrixTests () {
        ClientSize = new(1280, 720);
    }
    protected override void OnLoad () {
        base.OnLoad();
        Reusables.Add(va = new());
        Reusables.Add(flatColor = new());
        Reusables.Add(vertices = new(lineSegments));
        va.Assign(vertices, flatColor.VertexPosition);
    }

    private const float AngularVelocity = Maths.fTau / 16;
    private static readonly Key[] _AxisKeys= { Key.Z, Key.X, Key.C, Key.D, Key.Q, Key.A, Key.Insert, Key.Home, Key.PageUp, Key.Delete, Key.End, Key.PageDown, Key.Up, Key.Down, Key.Left, Key.Right, Key.N, Key.M };
    private static readonly Vector4[] lineSegments = { new(Vector3.Zero, 1), new(Vector3.UnitX, 1), new(Vector3.Zero, 1), new(Vector3.UnitY, 1), new(Vector3.Zero, 1), new(Vector3.UnitZ, 1), };
    private static readonly Vector4[] Colors = { new(1, 0, 0, 1), new(0, 1, 0, 1), new(0, 0, 1, 1), };
    protected override Key[] AxisKeys => _AxisKeys;

    private FlatColor flatColor;
    private VertexArray va;
    private BufferObject<Vector4> vertices;
    private Pose camera = new() { Position = 2 * Vector3.UnitZ };
    private Pose model = new();

    protected override void Render () {
        var size = ClientSize;

        Vector3 modelTranslation = new((float)Axis(Key.C, Key.Z), (float)Axis(Key.Q, Key.A), (float)Axis(Key.X, Key.D));
        var modelRotation = AngularVelocity * new Vector3((float)Axis(Key.Insert, Key.Delete), (float)Axis(Key.Home, Key.End), (float)Axis(Key.PageUp, Key.PageDown));

        Experiment.RotateQuaternion(ref model.Orientation, modelRotation);
        Experiment.CameraRotate(ref camera.Orientation, new((float)Axis(Key.Up, Key.Down), (float)Axis(Key.Left, Key.Right), (float)Axis(Key.N, Key.M)));

        model.Position += Vector3.Transform(modelTranslation, model.Orientation);

        var modelMatrix = Matrix4x4.CreateFromQuaternion(model.Orientation) * Matrix4x4.CreateTranslation((Vector3)model.Position);
        var viewMatrix = Matrix4x4.CreateTranslation(-(Vector3)camera.Position) * Matrix4x4.CreateFromQuaternion(camera.Orientation);
        var projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(Maths.fPi / 4, (float)size.X / size.Y, 1f, 100f);

        Viewport(in Vector2i.Zero, in size);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);

        UseProgram(flatColor);
        BindVertexArray(va);

        flatColor.Model(modelMatrix);
        flatColor.View(viewMatrix);
        flatColor.Projection(projectionMatrix);
        for (var i = 0; i < 3; ++i) {
            flatColor.Color(Colors[i]);
            DrawArrays(Primitive.Lines, 2 * i, 2);
        }

    }
}

