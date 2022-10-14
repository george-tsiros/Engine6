namespace Engine6;
using Common;
using Gl;
using static Gl.GlContext;
using System.Numerics;
using Shaders;
using Win32;
using System.Diagnostics;

public class ExampleDrawArrays:ExampleBase {

    private Directional program;
    private VertexArray va;
    private BufferObject<Vector4> vertexBuffer;
    private BufferObject<Vector3> normalBuffer;

    private const int VerticesPerTriangle = 3;
    private const int TrianglesPerQuad = 2;
    private const int VertexCount = 10 * 10 * TrianglesPerQuad * VerticesPerTriangle;
    private static readonly Vector2[] quad = { new(0, 0), new(1, 0), new(1, 1), new(0, 0), new(1, 1), new(0, 1) };

    public ExampleDrawArrays () : this(new(1280, 720)) { }
    public ExampleDrawArrays (Vector2i size) {
        ClientSize = size;
        var faces = new Vector4[VertexCount];
        for (var (y, i) = (-5, 0); y < 5; ++y)
            for (var x = -5; x < 5; ++x) {
                Vector2 xy = new(x, y);
                foreach (var f in quad)
                    faces[i++] = new(Vector3.Normalize(new((f + xy) / 5f, 1)), 1);
            }

        Reusables.Add(va = new());
        Reusables.Add(program = new());
        Reusables.Add(vertexBuffer = new(faces));
        var normal = new Vector3[faces.Length];
        for (var i = 0; i < normal.Length; ++i)
            normal[i] = Vector3.Normalize(faces[i].Xyz());
        Reusables.Add(normalBuffer = new(normal));

        va.Assign(vertexBuffer, program.VertexPosition);
        va.Assign(normalBuffer, program.VertexNormal);
        UseProgram(program);
    }

    protected override void Render () {
        var xActual = MatrixTests.ApplyDeadzone(cursor.X, Deadzone);
        var yActual = MatrixTests.ApplyDeadzone(cursor.Y, Deadzone);
        var yaw = xActual * Maths.fPi / 3 / (CursorCap - Deadzone);
        var pitch = yActual * Maths.fPi / 3 / (CursorCap - Deadzone);
        var size = ClientSize;
        var aspectRatio = (float)size.X / size.Y;

        Viewport(in Vector2i.Zero, in size);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        BindVertexArray(va);
        UseProgram(program);
        Enable(Capability.DepthTest);
        Enable(Capability.CullFace);
        program.View(Matrix4x4.CreateTranslation(0, 0, -4));
        program.Projection(Matrix4x4.CreatePerspectiveFieldOfView(Maths.fPi / 4, aspectRatio, 1, 100));
        program.LightDirection(-Vector4.UnitZ);
        program.Model(Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, 0));
        program.Color(new(0, .8f, .2f, 1));
        DrawArrays(Primitive.Triangles, 0, VertexCount / 2);
        program.Color(new(0, .2f, .8f, 1));
        DrawArrays(Primitive.Triangles, VertexCount / 2, VertexCount / 2);
    }
}

