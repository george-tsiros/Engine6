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
        new() { VertexCount = 6, InstanceCount = 1, First = 0, BaseInstance = 0,  },
        new() { VertexCount = 12, InstanceCount = 1, First = 0, BaseInstance = 0,  },
        new() { VertexCount = 15, InstanceCount = 1, First = 0, BaseInstance = 0,  },
        new() { VertexCount = 18, InstanceCount = 1, First = 0, BaseInstance = 0,  },
    };

    public ExampleDrawArraysIndirect () {
        Reusables.Add(va = new());
        Reusables.Add(program = new());
        Reusables.Add(vertexBuffer = new(ThreeFaces));
        va.Assign(vertexBuffer, program.VertexPosition);

        Reusables.Add(commandBuffer = new(Commands, BufferTarget.DrawIndirect));
        commandBuffer.Bind();
    }

    protected override void Render () {
        var xActual = MatrixTests.ApplyDeadzone(cursor.X, Deadzone);
        var yActual = MatrixTests.ApplyDeadzone(cursor.Y, Deadzone);
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
        DrawArraysIndirect(Primitive.Triangles, 3);
    }
}
