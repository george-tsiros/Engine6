namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class Directional:Program {
#pragma warning disable CS0649

    protected override string VertexSource { get; } = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbjsgaW4gdmVjMyB2ZXJ0ZXhOb3JtYWw7IHVuaWZvcm0gbWF0NCBtb2RlbCwgdmlldywgcHJvamVjdGlvbjsgb3V0IHZlYzQgbjsgdm9pZCBtYWluKCkgeyBuID0gbW9kZWwgKiB2ZWM0KHZlcnRleE5vcm1hbCwwKTsgZ2xfUG9zaXRpb24gPSBwcm9qZWN0aW9uICogdmlldyAqIG1vZGVsICogdmVydGV4UG9zaXRpb247IH0=";
    protected override string FragmentSource { get; } = "dW5pZm9ybSB2ZWM0IGNvbG9yOyB1bmlmb3JtIHZlYzQgbGlnaHREaXJlY3Rpb247IGluIHZlYzQgbjsgb3V0IHZlYzQgY29sb3IwOyB2b2lkIG1haW4oKSB7IGZsb2F0IGQgPSBtYXgoZG90KG5vcm1hbGl6ZShuKSwtbm9ybWFsaXplKGxpZ2h0RGlyZWN0aW9uKSksIDAuMCk7IGNvbG9yMCA9IHZlYzQoY29sb3IucmdiICogKDAuOSAqIGQgKyAwLjEpLCAxLjApOyB9";

    public FragOut Color0 { get; }

    public Attrib<Vector3> VertexNormal { get; }

    public Attrib<Vector4> VertexPosition { get; }

    private readonly int color;
    public void Color (in Vector4 v) => Uniform(color, v);

    private readonly int lightDirection;
    public void LightDirection (in Vector4 v) => Uniform(lightDirection, v);

    private readonly int model;
    public void Model (in Matrix4x4 v) => Uniform(model, v);

    private readonly int projection;
    public void Projection (in Matrix4x4 v) => Uniform(projection, v);

    private readonly int view;
    public void View (in Matrix4x4 v) => Uniform(view, v);

    public Directional () {
        view = GetUniformLocation(this, nameof(view));
        projection = GetUniformLocation(this, nameof(projection));
        model = GetUniformLocation(this, nameof(model));
        lightDirection = GetUniformLocation(this, nameof(lightDirection));
        color = GetUniformLocation(this, nameof(color));
        VertexPosition = GetAttribLocation(this, "vertexPosition");
        VertexNormal = GetAttribLocation(this, "vertexNormal");
        Color0 = GetFragDataLocation(this, "color0");
    }

#pragma warning restore CS0649
}