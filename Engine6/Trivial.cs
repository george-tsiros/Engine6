namespace Engine6;
using Common;
using Gl;
using static Gl.GlContext;
using System.Numerics;
using Shaders;
using System;

public class Trivial:GlWindow {

    private static readonly Vector4[] quad = Array.ConvertAll(PresentationQuad, v => new Vector4((v + Vector2.One) / 2, 0, 1));

    public Trivial () : this(new(1280, 720)) { }

    public Trivial (in Vector2i size) {
        ClientSize = size;
        Reusables.Add(va = new());
        Reusables.Add(program = new());
        Reusables.Add(vertices = new(quad));
        va.Assign(vertices, program.VertexPosition);
        UseProgram(program);
        program.Color(Vector4.One);
        program.Model(Matrix4x4.Identity);
        program.View(Matrix4x4.CreateTranslation(0, 0, -10));
    }

    private FlatColor program;
    private VertexArray va;
    private BufferObject<Vector4> vertices;

    protected override void Render () {
        var size = ClientSize;
        Viewport(in Vector2i.Zero, in size);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        BindVertexArray(va);
        UseProgram(program);
        program.Projection(Matrix4x4.CreatePerspectiveFieldOfView(Maths.fPi / 4, (float)size.X / size.Y, 1, 100));
        DrawArrays(Primitive.Triangles, 0, 6);
    }
}

