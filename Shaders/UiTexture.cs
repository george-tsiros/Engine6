namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class UiTexture:Program {
#pragma warning disable CS0649

    protected override string VertexSource { get; } = "aW4gdmVjMiB2ZXJ0ZXhQb3NpdGlvbjsgb3V0IHZlYzIgdXY7IHZvaWQgbWFpbigpIHsgdXYgPSAodmVydGV4UG9zaXRpb24gKyB2ZWMyKDEpKSAvIDI7IGdsX1Bvc2l0aW9uID0gdmVjNCh2ZXJ0ZXhQb3NpdGlvbiwgMCwgMSk7IH0=";
    protected override string FragmentSource { get; } = "dW5pZm9ybSBzYW1wbGVyMkQgdGV4MDsgaW4gdmVjMiB1djsgb3V0IHZlYzQgY29sb3IwOyB2b2lkIG1haW4oKSB7IGNvbG9yMCA9IHZlYzQodGV4dHVyZSh0ZXgwLCB1dikucmdiLCAxKTsgfQ==";

    public FragOut Color0 { get; }

    public Attrib<Vector2> VertexPosition { get; }

    private readonly int tex0;
    public void Tex0 (int v) => Uniform(tex0, v);

    public UiTexture () {
        tex0 = GetUniformLocation(this, nameof(tex0));
        VertexPosition = GetAttribLocation(this, "vertexPosition");
        Color0 = GetFragDataLocation(this, "color0");
    }

#pragma warning restore CS0649
}