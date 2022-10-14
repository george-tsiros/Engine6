namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class Glow:Program {
#pragma warning disable CS0649

    protected override string VertexSource { get; } = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbjsgaW4gdmVjMiB2ZXJ0ZXhVVjsgaW4gdmVjMyB2ZXJ0ZXhOb3JtYWw7IHVuaWZvcm0gbWF0NCBtb2RlbCwgdmlldywgcHJvamVjdGlvbjsgb3V0IHZlYzIgdXY7IGZsYXQgb3V0IHZlYzQgbjsgdm9pZCBtYWluKCkgeyB1diA9IHZlcnRleFVWOyBuID0gbm9ybWFsaXplKG1vZGVsICogdmVjNCh2ZXJ0ZXhOb3JtYWwsMCkpOyBnbF9Qb3NpdGlvbiA9IHByb2plY3Rpb24gKiB2aWV3ICogbW9kZWwgKiB2ZXJ0ZXhQb3NpdGlvbjsgfQ==";
    protected override string FragmentSource { get; } = "aW4gdmVjMiB1djsgZmxhdCBpbiB2ZWM0IG47IHVuaWZvcm0gdmVjNCBsaWdodERpcmVjdGlvbjsgb3V0IHZlYzQgY29sb3IwOyB2b2lkIG1haW4oKSB7IGZsb2F0IGludGVuc2l0eSA9IG1heChkb3QoLWxpZ2h0RGlyZWN0aW9uLnh5eiwgbi54eXopLCAwLjApOyBjb2xvcjAgPSB2ZWM0KGludGVuc2l0eSAqIHZlYzMoMSksIDEpOyB9";

    public FragOut Color0 { get; }

    public Attrib<Vector3> VertexNormal { get; }

    public Attrib<Vector4> VertexPosition { get; }

    public Attrib<Vector2> VertexUV { get; }

    private readonly int lightDirection;
    public void LightDirection (in Vector4 v) => Uniform(lightDirection, v);

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
        lightDirection = GetUniformLocation(this, nameof(lightDirection));
        VertexUV = GetAttribLocation(this, "vertexUV");
        VertexPosition = GetAttribLocation(this, "vertexPosition");
        VertexNormal = GetAttribLocation(this, "vertexNormal");
        Color0 = GetFragDataLocation(this, "color0");
    }

#pragma warning restore CS0649
}