
namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class DirectionalFlat:Program {
#pragma warning disable CS0649
    protected override string VertexSource { get; } = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbjsgaW4gdmVjNCB2ZXJ0ZXhOb3JtYWw7IHVuaWZvcm0gdmVjNCBsaWdodERpcmVjdGlvbjsgdW5pZm9ybSBtYXQ0IG1vZGVsLCB2aWV3LCBwcm9qZWN0aW9uOyBvdXQgdmVjNCBjb2xvcjsgdm9pZCBtYWluICgpIHsgZmxvYXQgbGlnaHRJbnRlbnNpdHkgPSBkb3Qobm9ybWFsaXplKG1vZGVsICogdmVydGV4Tm9ybWFsKS54eXosIC1saWdodERpcmVjdGlvbi54eXopOyBjb2xvciA9IHZlYzQobGlnaHRJbnRlbnNpdHksIGxpZ2h0SW50ZW5zaXR5LCBsaWdodEludGVuc2l0eSwgMSk7IGdsX1Bvc2l0aW9uID0gcHJvamVjdGlvbiAqIHZpZXcgKiBtb2RlbCAqIHZlcnRleFBvc2l0aW9uOyB9";
    protected override string FragmentSource { get; } = "aW4gdmVjNCBjb2xvcjsgb3V0IHZlYzQgb3V0MDsgdm9pZCBtYWluICgpIHsgb3V0MCA9IGNvbG9yOyB9";

    //size 1, type Vector4
    [GlAttrib("vertexNormal")]
    public int VertexNormal { get; }

    //size 1, type Vector4
    [GlAttrib("vertexPosition")]
    public int VertexPosition { get; }

    //size 1, type Vector4
    [GlUniform("lightDirection")]
    private readonly int lightDirection;
    public void LightDirection (in Vector4 v) => Uniform(lightDirection, v);

    //size 1, type Matrix4x4
    [GlUniform("model")]
    private readonly int model;
    public void Model (in Matrix4x4 v) => Uniform(model, v);

    //size 1, type Matrix4x4
    [GlUniform("projection")]
    private readonly int projection;
    public void Projection (in Matrix4x4 v) => Uniform(projection, v);

    //size 1, type Matrix4x4
    [GlUniform("view")]
    private readonly int view;
    public void View (in Matrix4x4 v) => Uniform(view, v);

#pragma warning restore CS0649
}