namespace Shaders;

using Gl;
using static Gl.Opengl;
using System.Numerics;
using Common;

public class DirectionalFlat:Program {
#pragma warning disable CS0649
    protected override string VertexSource { get; } = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbjsgaW4gdmVjNCBmYWNlTm9ybWFsOyB1bmlmb3JtIHZlYzQgbGlnaHREaXJlY3Rpb247IHVuaWZvcm0gbWF0NCBtb2RlbCwgdmlldywgcHJvamVjdGlvbjsgb3V0IHZlYzQgY29sb3I7IHZvaWQgbWFpbiAoKSB7IGZsb2F0IGxpZ2h0SW50ZW5zaXR5ID0gZG90KG5vcm1hbGl6ZShtb2RlbCAqIGZhY2VOb3JtYWwpLnh5eiwgLWxpZ2h0RGlyZWN0aW9uLnh5eik7IGNvbG9yID0gdmVjNChsaWdodEludGVuc2l0eSwgbGlnaHRJbnRlbnNpdHksIGxpZ2h0SW50ZW5zaXR5LCAxKTsgZ2xfUG9zaXRpb24gPSBwcm9qZWN0aW9uICogdmlldyAqIG1vZGVsICogdmVydGV4UG9zaXRpb247IH0=";
    protected override string FragmentSource { get; } = "aW4gdmVjNCBjb2xvcjsgb3V0IHZlYzQgb3V0MDsgdm9pZCBtYWluICgpIHsgb3V0MCA9IGNvbG9yOyB9";

    //size 1, type Vector4
    [GlAttrib("faceNormal")]
    public int FaceNormal { get; }

    //size 1, type Vector4
    [GlAttrib("vertexPosition")]
    public int VertexPosition { get; }

    //size 1, type Vector4
    [GlUniform("lightDirection")]
    private readonly int lightDirection;
    public void LightDirection (Vector4 v) => Uniform(lightDirection, v);

    //size 1, type Matrix4x4
    [GlUniform("model")]
    private readonly int model;
    public void Model (Matrix4x4 v) => Uniform(model, v);

    //size 1, type Matrix4x4
    [GlUniform("projection")]
    private readonly int projection;
    public void Projection (Matrix4x4 v) => Uniform(projection, v);

    //size 1, type Matrix4x4
    [GlUniform("view")]
    private readonly int view;
    public void View (Matrix4x4 v) => Uniform(view, v);

#pragma warning restore CS0649
}
