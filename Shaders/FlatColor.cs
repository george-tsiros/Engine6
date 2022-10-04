namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class FlatColor:Program {
#pragma warning disable CS0649

    protected override string VertexSource { get; } = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbjsgdW5pZm9ybSBtYXQ0IG1vZGVsLCB2aWV3LCBwcm9qZWN0aW9uOyB2b2lkIG1haW4oKSB7IGdsX1Bvc2l0aW9uID0gcHJvamVjdGlvbiAqIHZpZXcgKiBtb2RlbCAqIHZlcnRleFBvc2l0aW9uOyB9";
    protected override string FragmentSource { get; } = "dW5pZm9ybSB2ZWM0IGNvbG9yOyBvdXQgdmVjNCBjb2xvcjA7IHZvaWQgbWFpbigpIHsgY29sb3IwID0gY29sb3I7IH0=";

    public FragOut Color0 { get; }

    public Attrib<Vector4> VertexPosition { get; }

    private readonly int color;
    public void Color (in Vector4 v) => Uniform(color, v);

    private readonly int model;
    public void Model (in Matrix4x4 v) => Uniform(model, v);

    private readonly int projection;
    public void Projection (in Matrix4x4 v) => Uniform(projection, v);

    private readonly int view;
    public void View (in Matrix4x4 v) => Uniform(view, v);

    public FlatColor () {
        view = GetUniformLocation(this, nameof(view));
        projection = GetUniformLocation(this, nameof(projection));
        model = GetUniformLocation(this, nameof(model));
        color = GetUniformLocation(this, nameof(color));
        VertexPosition = GetAttribLocation(this, "vertexPosition");
        Color0 = GetFragDataLocation(this, "color0");
    }

#pragma warning restore CS0649
}