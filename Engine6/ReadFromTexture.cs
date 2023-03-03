namespace Engine6;

using Win32;
using System.Diagnostics;
using System;
using Gl;
using static Gl.GlContext;
using System.Numerics;
using Shaders;
using System.Text;
using Common;

public class ReadFromTexture:GlWindow {

    public ReadFromTexture () : this(new(1280, 720)) { }

    private VertexArray va;
    private BufferObject<Vector4> vertexBuffer;
    private Framebuffer fb;
    private FlatColor program;
    private static readonly Vector4[] vertices = {
        new(0, 0, 0, 1),
        new(2, 0, -2, 1),
        new(0, 1, 0, 1),
    };

    public ReadFromTexture (Vector2i size) {
        ClientSize = size;
        Reusables.Add(va = new());
        Reusables.Add(program = new());
        Reusables.Add(vertexBuffer = new(vertices));
        va.Assign(vertexBuffer, program.VertexPosition);
    }

    protected override void OnLoad () {
        base.OnLoad();
        fb = new();

    }

    protected override void Render () {
        var size = ClientSize;
        var aspectRatio = (float)size.X / size.Y;
        Viewport(in Vector2i.Zero, in size);
        Enable(Capability.DEPTH_TEST);
        Enable(Capability.CULL_FACE);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        UseProgram(program);
        BindVertexArray(va);
        program.Color(Vector4.One);
        program.Model(Matrix4x4.CreateTranslation(0, -1, -2));
        program.View(Matrix4x4.Identity);
        program.Projection(Matrix4x4.CreatePerspectiveFieldOfView(Maths.fPi / 2, aspectRatio, 1f, 10f));
        DrawArrays(PrimitiveType.TRIANGLES, 0, 3);
    }
}
