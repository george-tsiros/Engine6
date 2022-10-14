namespace Engine6;
using Common;
using Gl;
using static Gl.GlContext;
using System.Numerics;
using Shaders;
using Win32;

public class ExampleDrawArrays:ExampleBase {

    private Directional program;
    private VertexArray va;
    private BufferObject<Vector4> vertexBuffer;
    private BufferObject<Vector3> normalBuffer;

    public ExampleDrawArrays () {
        Reusables.Add(va = new());
        Reusables.Add(program = new());
        Reusables.Add(vertexBuffer = new(ThreeFaces));
        Reusables.Add(normalBuffer = new(ThreeFacesNormals));
        va.Assign(vertexBuffer, program.VertexPosition);
        va.Assign(normalBuffer, program.VertexNormal);
        UseProgram(program);
    }

    int startAt = 0;

    protected override void OnKeyUp (Key key) {
        switch (key) {
            case Key.D1:
            case Key.D2:
            case Key.D3:
                startAt = (key - Key.D1) * 6;
                return;
        }
        base.OnKeyUp(key);
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
        program.Color(Vector4.One);
        program.Model(Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, 0));
        DrawArrays(Primitive.Triangles, startAt, 6);
    }
}

