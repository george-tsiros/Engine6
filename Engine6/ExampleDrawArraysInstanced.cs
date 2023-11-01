namespace Engine6;
using Common;
using Gl;
using static Gl.GlContext;
using System.Numerics;
using Shaders;
using System;

public class ExampleDrawArraysInstanced:ExampleBase {

    private static readonly Matrix4x4[] Translations = {
        Matrix4x4.CreateTranslation(-4, -4, 0),
        Matrix4x4.CreateTranslation( 0, -4, 0),
        Matrix4x4.CreateTranslation( 4, -4, 0),
        Matrix4x4.CreateTranslation(-4, 0, 0),
        Matrix4x4.CreateTranslation( 0, 0, 0),
        Matrix4x4.CreateTranslation( 4, 0, 0),
        Matrix4x4.CreateTranslation(-4, 4, 0),
        Matrix4x4.CreateTranslation( 0, 4, 0),
        Matrix4x4.CreateTranslation( 4, 4, 0),
    };

    private DirectionalInstanced program;
    private VertexArray va;
    private BufferObject<Vector4> vertexBuffer;
    private BufferObject<Vector4> colorBuffer;
    private BufferObject<Vector3> normalBuffer;
    private BufferObject<Matrix4x4> modelBuffer;

    public ExampleDrawArraysInstanced () : this(new(1280, 720)) { }

    public ExampleDrawArraysInstanced (Vector2i size) {
        ClientSize = size;
        Reusables.Add(va = new());
        Reusables.Add(program = new());
        Reusables.Add(vertexBuffer = new(ThreeFaces));
        Reusables.Add(normalBuffer = new(ThreeFacesNormals));
        Reusables.Add(modelBuffer = new BufferObject<Matrix4x4>(Translations.Length));
        Reusables.Add(colorBuffer = new(CyberpunkColors));
        va.Assign(vertexBuffer, program.VertexPosition);
        va.Assign(normalBuffer, program.VertexNormal);
        va.Assign(modelBuffer, program.Model, 1);
        va.Assign(colorBuffer, program.Color, 1);
    }

    private readonly Matrix4x4[] modelMatrices = new Matrix4x4[9];

    private const int CursorCap = 1000;
    private const int Deadzone = 10;

    private Vector2i cursor;
    protected override void OnInput (int dx, int dy) {
        var x = int.Clamp(cursor.X + dx, -CursorCap, CursorCap);
        var y = int.Clamp(cursor.Y + dy, -CursorCap, CursorCap);
        cursor = new(x, y);
    }

    protected override void Render () {
        var xActual = Functions.ApplyDeadzone(cursor.X, Deadzone);
        var yActual = Functions.ApplyDeadzone(cursor.Y, Deadzone);
        var yaw = xActual * float.Pi / 3 / (CursorCap - Deadzone);
        var pitch = yActual * float.Pi / 3 / (CursorCap - Deadzone);
        var size = ClientSize;
        var aspectRatio = (float)size.X / size.Y;
        var rotation = Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, 0);
        for (var i = 0; i < 9; ++i)
            modelMatrices[i] = rotation * Translations[i];
        modelBuffer.BufferData(modelMatrices, Translations.Length, 0, 0);
        Viewport(in Vector2i.Zero, in size);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        BindVertexArray(va);
        UseProgram(program);
        Enable(Capability.DEPTH_TEST);
        Enable(Capability.CULL_FACE);
        program.View(Matrix4x4.CreateTranslation(0, 0, -15));
        program.Projection(Matrix4x4.CreatePerspectiveFieldOfView(float.Pi / 4, aspectRatio, 1, 100));
        program.LightDirection(-Vector4.UnitZ);
        DrawArraysInstanced(PrimitiveType.TRIANGLES, 0, ThreeFaces.Length, Translations.Length);
    }
}