namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class Glow:Program {
#pragma warning disable CS0649

    protected override string VertexSource { get; } = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbjsgaW4gdmVjMiB2ZXJ0ZXhVVjsgdW5pZm9ybSBtYXQ0IG1vZGVsLCB2aWV3LCBwcm9qZWN0aW9uOyBvdXQgdmVjMiB1djsgdm9pZCBtYWluKCkgeyB1diA9IHZlcnRleFVWOyBnbF9Qb3NpdGlvbiA9IHByb2plY3Rpb24gKiB2aWV3ICogbW9kZWwgKiB2ZXJ0ZXhQb3NpdGlvbjsgfQ==";
    protected override string FragmentSource { get; } = "aW4gdmVjMiB1djsgb3V0IHZlYzQgY29sb3IwOyB2b2lkIG1haW4oKSB7IGNvbG9yMCA9IHZlYzQoMCwgdXYueCwgdXYueSwgMSk7IH0=";

    public FragOut Color0 { get; }

    public Attrib<Vector4> VertexPosition { get; }

    public Attrib<Vector2> VertexUV { get; }

    private readonly int model;
    public void Model (in Matrix4x4 v) => Uniform(model, v);

    private readonly int projection;
    public void Projection (in Matrix4x4 v) => Uniform(projection, v);

    private readonly int view;
    public void View (in Matrix4x4 v) => Uniform(view, v);

    public Glow () {
        view = GetUniformLocation(this, nameof(view));
        projection = GetUniformLocation(this, nameof(projection));
        model = GetUniformLocation(this, nameof(model));
        VertexUV = GetAttribLocation(this, "vertexUV");
        VertexPosition = GetAttribLocation(this, "vertexPosition");
        Color0 = GetFragDataLocation(this, "color0");
    }

#pragma warning restore CS0649
}