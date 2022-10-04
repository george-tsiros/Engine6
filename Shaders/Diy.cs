namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class Diy:Program {
#pragma warning disable CS0649

    protected override string VertexSource { get; } = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbjsgdW5pZm9ybSBtYXQ0IG1hdHJpeDsgdm9pZCBtYWluKCkgeyBnbF9Qb3NpdGlvbiA9IG1hdHJpeCAqIHZlcnRleFBvc2l0aW9uOyB9";
    protected override string FragmentSource { get; } = "dW5pZm9ybSB2ZWM0IGNvbG9yOyBvdXQgdmVjNCBjb2xvcjA7IHZvaWQgbWFpbigpIHsgY29sb3IwID0gY29sb3I7IH0=";

    public FragOut Color0 { get; }

    public Attrib<Vector4> VertexPosition { get; }

    private readonly int color;
    public void Color (in Vector4 v) => Uniform(color, v);

    private readonly int matrix;
    public void Matrix (in Matrix4x4 v) => Uniform(matrix, v);

    public Diy () {
        matrix = GetUniformLocation(this, nameof(matrix));
        color = GetUniformLocation(this, nameof(color));
        VertexPosition = GetAttribLocation(this, "vertexPosition");
        Color0 = GetFragDataLocation(this, "color0");
    }

#pragma warning restore CS0649
}