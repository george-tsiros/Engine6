namespace Engine6;
using Common;
using Gl;
using static Gl.GlContext;
using System.Numerics;
using Shaders;

public class ExampleDrawArraysIndirect:ExampleBase {

    private FlatColor program;
    private VertexArray va;
    private BufferObject<Vector4> vertexBuffer;
    private BufferObject<DrawArraysIndirectCommand> commandBuffer;

    private static readonly DrawArraysIndirectCommand[] Commands = {
        new() { VertexCount = 3, InstanceCount = 1, First = 0, BaseInstance = 0,  },
        new() { VertexCount = 3, InstanceCount = 1, First = 3, BaseInstance = 0,  },
        new() { VertexCount = 3, InstanceCount = 1, First = 6, BaseInstance = 0,  },
        new() { VertexCount = 3, InstanceCount = 1, First = 9, BaseInstance = 0,  },
    };

    public ExampleDrawArraysIndirect () {
        Reusables.Add(va = new());
        Reusables.Add(program = new());
        Reusables.Add(vertexBuffer = new(ThreeFaces));
        va.Assign(vertexBuffer, program.VertexPosition);

        Reusables.Add(commandBuffer = new(Commands, BufferTarget.DrawIndirect));
        commandBuffer.Bind();
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
        Enable(Capability.DepthTest);
        Enable(Capability.CullFace);
        program.Color(Vector4.One);
        program.Model(Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, 0));
        program.View(Matrix4x4.CreateTranslation(0, 0, -15));
        program.Projection(Matrix4x4.CreatePerspectiveFieldOfView(Maths.fPi / 4, aspectRatio, 1, 100));
        DrawArraysIndirect(Primitive.Triangles, 0);
    }
}
