namespace Engine6;
using Common;
using Gl;
using static Gl.GlContext;
using System.Numerics;
using Shaders;

public class ExampleMultiDrawArrays:GlWindow {
    /*

       7-----6
      /     /
     /     / |
    3-----2  |
    |  4  |  |5
    |     | /
    |     |/
    0-----1

        new(-1, -1, +1, 1), // 0
        new(+1, -1, +1, 1), // 1
        new(+1, +1, +1, 1), // 2
        new(-1, +1, +1, 1), // 3
        new(-1, -1, -1, 1), // 4
        new(+1, -1, -1, 1), // 5
        new(+1, +1, -1, 1), // 6
        new(-1, +1, -1, 1), // 7

*/
    private static readonly Vector4[] vertex = {
        new(+1, -1, +1, 1), // 1
        new(+1, -1, -1, 1), // 5
        new(+1, +1, -1, 1), // 6
        new(+1, -1, +1, 1), // 1
        new(+1, +1, -1, 1), // 6
        new(+1, +1, +1, 1), // 2

        new(-1, +1, +1, 1), // 3
        new(+1, +1, +1, 1), // 2
        new(+1, +1, -1, 1), // 6
        new(-1, +1, +1, 1), // 3
        new(+1, +1, -1, 1), // 6
        new(-1, +1, -1, 1), // 7

        new(-1, -1, +1, 1), // 0
        new(+1, -1, +1, 1), // 1
        new(+1, +1, +1, 1), // 2
        new(-1, -1, +1, 1), // 0
        new(+1, +1, +1, 1), // 2
        new(-1, +1, +1, 1), // 3

    };

    private static readonly Vector3[] normal = {
        Vector3.UnitX,
        Vector3.UnitX,
        Vector3.UnitX,
        Vector3.UnitX,
        Vector3.UnitX,
        Vector3.UnitX,

        Vector3.UnitY,
        Vector3.UnitY,
        Vector3.UnitY,
        Vector3.UnitY,
        Vector3.UnitY,
        Vector3.UnitY,

        Vector3.UnitZ,
        Vector3.UnitZ,
        Vector3.UnitZ,
        Vector3.UnitZ,
        Vector3.UnitZ,
        Vector3.UnitZ,
    };

    private static readonly (Vector3 position, Vector3 color)[] Instances = new (Vector3, Vector3)[] {
        (new(-4, -4, 0), new(0x30/255f, 0x05/255f, 0xAE/255f)),
        (new( 0, -4, 0), new(0x40/255f, 0x04/255f, 0xFC/255f)),
        (new( 4, -4, 0), new(0x6F/255f, 0x00/255f, 0xFB/255f)),
        (new(-4, 0, 0), new(0x9D/255f, 0x00/255f, 0xFD/255f)),
        (new( 0, 0, 0), new(0xC2/255f, 0xDD/255f, 0xFB/255f)),
        (new( 4, 0, 0), new(0xCD/255f, 0x00/255f, 0xEE/255f)),
        (new(-4, 4, 0), new(0xFF/255f, 0x00/255f, 0x49/255f)),
        (new( 0, 4, 0), new(0x97/255f, 0x9D/255f, 0xA4/255f)),
        (new( 4, 4, 0), new(0xFF/255f, 0xBC/255f, 0xD1/255f)),
    };

    private Directional program;
    private VertexArray va;
    private BufferObject<Vector4> vertexBuffer;
    private BufferObject<Vector3> normalBuffer;
    private Vector2i cursor;
    private const int CursorCap = 1000;
    private const int Deadzone = 10;

    public ExampleMultiDrawArrays () {
        Reusables.Add(va = new());
        Reusables.Add(program = new());
        Reusables.Add(vertexBuffer = new(vertex));
        Reusables.Add(normalBuffer = new(normal));
        va.Assign(vertexBuffer, program.VertexPosition);
        va.Assign(normalBuffer, program.VertexNormal);
        UseProgram(program);
    }

    protected override void OnInput (int dx, int dy) {
        var x = Maths.Int32Clamp(cursor.X + dx, -CursorCap, CursorCap);
        var y = Maths.Int32Clamp(cursor.Y + dy, -CursorCap, CursorCap);
        cursor = new(x, y);
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
        program.View(Matrix4x4.CreateTranslation(0, 0, -15));
        program.Projection(Matrix4x4.CreatePerspectiveFieldOfView(Maths.fPi / 4, aspectRatio, 1, 100));
        program.LightDirection(-Vector4.UnitZ);
        MultiDrawArrays(Primitive.Triangles, 
        foreach (var (position, color) in Instances) {
            program.Color(new(color, 1));
            program.Model(Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, 0) * Matrix4x4.CreateTranslation(position));
            DrawArrays(Primitive.Triangles, 0, vertex.Length);
        }
    }

}

