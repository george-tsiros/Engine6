namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class Tex:Program {
#pragma warning disable CS0649

    protected override string VertexSource { get; } = "aW4gdmVjMiB2ZXJ0ZXhQb3NpdGlvbjsgb3V0IHZlYzIgdXY7IHZvaWQgbWFpbigpIHsgdXYgPSAodmVydGV4UG9zaXRpb24gKyB2ZWMyKDEpKSAvIDI7IGdsX1Bvc2l0aW9uID0gdmVjNCh2ZXJ0ZXhQb3NpdGlvbiwgMCwgMSk7IH0=";
    protected override string FragmentSource { get; } = "dW5pZm9ybSBzYW1wbGVyMkQgdGV4MDsgaW4gdmVjMiB1djsgb3V0IHZlYzQgY29sb3IwLCBjb2xvcjE7IHZvaWQgbWFpbigpIHsgdmVjNCB2ID0gdGV4dHVyZSh0ZXgwLCB1dik7IGlmICgwID09IHYuYSkgZGlzY2FyZDsgY29sb3IxID0gdmVjNCh2LmJnciwgMSk7IGNvbG9yMCA9IHY7IH0=";

    public FragOut Color0 { get; }

    public FragOut Color1 { get; }

    public Attrib<Vector2> VertexPosition { get; }

    private readonly int tex0;
    public void Tex0 (int v) => Uniform(tex0, v);

    public Tex () {
        tex0 = GetUniformLocation(this, nameof(tex0));
        VertexPosition = GetAttribLocation(this, "vertexPosition");
        Color1 = GetFragDataLocation(this, "color1");
        Color0 = GetFragDataLocation(this, "color0");
    }

#pragma warning restore CS0649
}