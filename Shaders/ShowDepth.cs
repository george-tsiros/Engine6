namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class ShowDepth:Program {
#pragma warning disable CS0649

    protected override string VertexSource { get; } = "aW4gdmVjMiB2ZXJ0ZXhQb3NpdGlvbjsgb3V0IHZlYzIgdXY7IHZvaWQgbWFpbigpIHsgdXYgPSAodmVydGV4UG9zaXRpb24gKyB2ZWMyKDEpKSAvIDI7IGdsX1Bvc2l0aW9uID0gdmVjNCh2ZXJ0ZXhQb3NpdGlvbiwgMCwgMSk7IH0=";
    protected override string FragmentSource { get; } = "dW5pZm9ybSBzYW1wbGVyMkQgZGVwdGg7IHVuaWZvcm0gZmxvYXQgZmFyLCBuZWFyOyBpbiB2ZWMyIHV2OyBvdXQgdmVjNCBjb2xvcjA7IHZvaWQgbWFpbigpIHsgZmxvYXQgYyA9ICgyLjAgKiBuZWFyKSAvIChmYXIgKyBuZWFyIC0gdGV4dHVyZTJEKGRlcHRoLCB1dikueCAqIChmYXIgLSBuZWFyKSk7IGNvbG9yMCA9IHZlYzQoYywgYywgYywgMSk7IH0=";

    public FragOut Color0 { get; }

    public Attrib<Vector2> VertexPosition { get; }

    private readonly int near;
    public void Near (in float v) => Uniform(near, v);

    private readonly int far;
    public void Far (in float v) => Uniform(far, v);

    private readonly int depth;
    public void Depth (int v) => Uniform(depth, v);

    public ShowDepth () {
        depth = GetUniformLocation(this, nameof(depth));
        far = GetUniformLocation(this, nameof(far));
        near = GetUniformLocation(this, nameof(near));
        VertexPosition = GetAttribLocation(this, "vertexPosition");
        Color0 = GetFragDataLocation(this, "color0");
    }

#pragma warning restore CS0649
}