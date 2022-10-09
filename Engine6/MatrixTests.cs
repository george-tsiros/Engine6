namespace Engine6;
using Win32;
using Common;
using Gl;
using static Gl.GlContext;
using System;
using System.Numerics;
using Shaders;

public class MatrixTests:GlWindow {

    private static readonly (int, int)[] C3_lines = { (0, 1), (0, 4), (1, 3), (3, 8), (4, 7), (6, 7), (6, 9), (5, 9), (5, 8), (2, 5), (2, 6), (3, 5), (4, 6), (1, 2), (0, 2), (8, 10), (10, 11), (7, 11), (1, 10), (0, 11), (1, 5), (0, 6), (20, 21), (12, 13), (18, 19), (14, 15), (16, 17), (15, 16), (14, 17), (13, 18), (12, 19), (2, 9), (22, 24), (23, 24), (22, 23), (25, 26), (26, 27), (25, 27), };
    private static readonly Vector3[] C3_vertices = { new(32, 0, -76), new(-32, 0, -76), new(0, 26, -24), new(-120, -3, 8), new(120, -3, 8), new(-88, 16, 40), new(88, 16, 40), new(128, -8, 40), new(-128, -8, 40), new(0, 26, 40), new(-32, -24, 40), new(32, -24, 40), new(-36, 8, 40), new(-8, 12, 40), new(8, 12, 40), new(36, 8, 40), new(36, -12, 40), new(8, -16, 40), new(-8, -16, 40), new(-36, -12, 40), new(0, 0, -76), new(0, 0, -90), new(-80, -6, 40), new(-80, 6, 40), new(-88, 0, 40), new(80, 6, 40), new(88, 0, 40), new(80, -6, 40), };

    public MatrixTests () {
        ClientSize = new(1280, 720);
        Span<Vector4> sphereVertices = new Vector4[highPolySphereVertexCount];
        Span<Vector4> sphereNormals = new Vector4[highPolySphereVertexCount];
        Experiment.Sphere(in highPolySphereSubdivisions, TerraRadius, sphereVertices);
        for (var i = 0; i < highPolySphereVertexCount; ++i)
            sphereNormals[i] = new(Vector3.Normalize(sphereVertices[i].Xyz()), 0);
        Reusables.Add(va = new());
        Reusables.Add(program = new());
        Reusables.Add(vertices = new(sphereVertices));
        Reusables.Add(normals = new(sphereNormals));
        va.Assign(vertices, program.VertexPosition);
        va.Assign(normals, program.VertexNormal);
    }

    protected override Key[] AxisKeys => _AxisKeys;

    private const float TerraRadius = 6.371e6f;
    private const int CursorCap = 1000;
    private const int Deadzone = 50;
    private static readonly Key[] _AxisKeys = { Key.Z, Key.C, };// Key.C, Key.D, Key.Q, Key.A, Key.Insert, Key.Home, Key.PageUp, Key.Delete, Key.End, Key.PageDown, Key.Up, Key.Down, Key.Left, Key.Right, Key.N, Key.M };
    private static readonly Vector2i highPolySphereSubdivisions = new(200, 100);
    private static readonly int highPolySphereVertexCount = 3 * Experiment.SphereTriangleCount(highPolySphereSubdivisions);
    private Directional program;
    private VertexArray va;
    private BufferObject<Vector4> vertices;
    private BufferObject<Vector4> normals;
    private Pose camera = new() { Position = new(0, TerraRadius + 100f, 0) };
    private Pose model = new();
    private Vector2i cursor;

    protected override void OnInput (int dx, int dy) {
        var x = Maths.Int32Clamp(cursor.X + dx, -CursorCap, CursorCap);
        var y = Maths.Int32Clamp(cursor.Y + dy, -CursorCap, CursorCap);
        cursor = new(x, y);
    }

    private static int ApplyDeadzone (int value, int deadzone) {
        if (deadzone < 0)
            throw new ArgumentOutOfRangeException(nameof(deadzone), "may not be negative");
        return value < 0 ? Maths.Int32Min(value + deadzone, 0) : Maths.Int32Max(value - deadzone, 0);
    }

    protected override void Render () {
        var size = ClientSize;
        var roll = .00001f * ApplyDeadzone(cursor.X, Deadzone);
        var pitch = -.000005f * ApplyDeadzone(cursor.Y, Deadzone);

        Experiment.CameraRotate(ref camera.Orientation, new(pitch, (float)Axis(Key.C, Key.Z), roll));

        var modelMatrix = Matrix4x4.CreateFromQuaternion(model.Orientation) * Matrix4x4.CreateTranslation((Vector3)model.Position);
        var viewMatrix = Matrix4x4.CreateTranslation(-(Vector3)camera.Position) * Matrix4x4.CreateFromQuaternion(camera.Orientation);
        var projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(Maths.fPi / 4, (float)size.X / size.Y, 10f, 100000f);

        Viewport(in Vector2i.Zero, in size);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);

        UseProgram(program);
        BindVertexArray(va);

        program.Model(modelMatrix);
        program.View(viewMatrix);
        program.Projection(projectionMatrix);
        program.Color(Vector4.One);
        program.LightDirection(Vector4.UnitX);
        DrawArrays(Primitive.Triangles, 0, highPolySphereVertexCount);
    }
}

