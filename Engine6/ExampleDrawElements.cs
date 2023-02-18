namespace Engine6;
using Common;
using Gl;
using static Gl.GlContext;
using System.Numerics;
using Shaders;

public class ExampleDrawElements:ExampleBase {

    private FlatColor program;
    private VertexArray va;
    private BufferObject<Vector4> vertexBuffer;
    private BufferObject<uint> elementBuffer;

    private static readonly Vector4[] CubeVertices = {
        new(-1, -1, +1, 1), // 0
        new(+1, -1, +1, 1), // 1
        new(+1, +1, +1, 1), // 2
        new(-1, +1, +1, 1), // 3
        new(-1, -1, -1, 1), // 4
        new(+1, -1, -1, 1), // 5
        new(+1, +1, -1, 1), // 6
        new(-1, +1, -1, 1), // 7
    };

    private static readonly uint[] Elements = { 0, 1, 2, 0, 2, 3, 1, 5, 6, 1, 6, 2, 3, 2, 6, 3, 6, 7, };

    public ExampleDrawElements () : this(new(1280, 720)) { }
    public ExampleDrawElements (Vector2i size) {
        ClientSize = size;
        Reusables.Add(va = new());
        Reusables.Add(program = new());
        Reusables.Add(vertexBuffer = new(CubeVertices));
        va.Assign(vertexBuffer, program.VertexPosition);

        Reusables.Add(elementBuffer = new(Elements, BufferTarget.ELEMENT_ARRAY_BUFFER));
        elementBuffer.BufferData(Elements, Elements.Length, 0, 0);
        elementBuffer.Bind();
    }

    private const int CursorCap = 1000;
    private const int Deadzone = 10;

    private Vector2i cursor;
    protected override void OnInput (int dx, int dy) {
        var x = Maths.Int32Clamp(cursor.X + dx, -CursorCap, CursorCap);
        var y = Maths.Int32Clamp(cursor.Y + dy, -CursorCap, CursorCap);
        cursor = new(x, y);
    }

    protected override void Render () {
        var xActual = Functions.ApplyDeadzone(cursor.X, Deadzone);
        var yActual = Functions.ApplyDeadzone(cursor.Y, Deadzone);
        var yaw = xActual * Maths.fPi / 3 / (CursorCap - Deadzone);
        var pitch = yActual * Maths.fPi / 3 / (CursorCap - Deadzone);
        var size = ClientSize;
        var aspectRatio = (float)size.X / size.Y;
        var rotation = Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, 0);
        Viewport(in Vector2i.Zero, in size);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        BindVertexArray(va);
        UseProgram(program);
        Enable(Capability.DEPTH_TEST);
        Enable(Capability.CULL_FACE);
        program.Color(Vector4.One);
        program.Model(Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, 0));
        program.View(Matrix4x4.CreateTranslation(0, 0, -15));
        program.Projection(Matrix4x4.CreatePerspectiveFieldOfView(Maths.fPi / 4, aspectRatio, 1, 100));
        DrawElements(PrimitiveType.TRIANGLES, 18);
    }
}
