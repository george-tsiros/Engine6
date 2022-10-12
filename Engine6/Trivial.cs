namespace Engine6;
using Common;
using Gl;
using static Gl.GlContext;
using System.Numerics;
using Shaders;
using System;

public class Trivial:GlWindow {

    private static readonly Vector2[] uv = { };
    private static readonly Vector4[] quad = { new(0, -.5f, 0, 1), new(0, .5f, 0, 1), new(-.5f, 0, -.5f, 1), new(0, -.5f, 0, 1), new(.5f, 0, -.5f, 1), new(0, .5f, 0, 1), };
    private Glow program;
    private VertexArray va;
    private BufferObject<Vector4> vertices;
    private BufferObject<Vector2> uvCoords;
    private Vector2i cursor;
    private const int CursorCap = 1000;
    private const int Deadzone = 100;

    public Trivial () : this(new(1280, 720)) { }

    public Trivial (in Vector2i size) {
        ClientSize = size;
        Reusables.Add(va = new());
        Reusables.Add(program = new());
        Reusables.Add(vertices = new(quad));
        Reusables.Add(uvCoords = new(uv));
        va.Assign(vertices, program.VertexPosition);
        va.Assign(uvCoords, program.VertexUV);
        UseProgram(program);
    }

    protected override void OnInput (int dx, int dy) {
        var x = Maths.Int32Clamp(cursor.X + dx, -CursorCap, CursorCap);
        var y = Maths.Int32Clamp(cursor.Y + dy, -CursorCap, CursorCap);
        cursor = new(x, y);
    }

    protected override void Render () {
        var xActual = MatrixTests.ApplyDeadzone(cursor.X, Deadzone) / (float)(CursorCap - Deadzone);
        var yActual = MatrixTests.ApplyDeadzone(cursor.Y, Deadzone) / (float)(CursorCap - Deadzone);

        var size = ClientSize;
        Viewport(in Vector2i.Zero, in size);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        BindVertexArray(va);
        UseProgram(program);
        program.View(Matrix4x4.CreateTranslation(0, 0, -5));
        program.Model(Matrix4x4.CreateFromYawPitchRoll(xActual, yActual, 0) * Matrix4x4.CreateTranslation(0, 0, -5));
        program.Projection(Matrix4x4.CreatePerspectiveFieldOfView(Maths.fPi / 4, (float)size.X / size.Y, 1, 100));
        DrawArrays(Primitive.Triangles, 0, 6);
    }
}

