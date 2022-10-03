
namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class Directional:Program {
#pragma warning disable CS0649
    protected override string VertexSource { get; } = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbjsgaW4gdmVjNCB2ZXJ0ZXhOb3JtYWw7IHVuaWZvcm0gbWF0NCBtb2RlbCwgdmlldywgcHJvamVjdGlvbjsgb3V0IHZlYzQgbjsgdm9pZCBtYWluKCkgeyBuID0gbW9kZWwgKiB2ZXJ0ZXhOb3JtYWw7IGdsX1Bvc2l0aW9uID0gcHJvamVjdGlvbiAqIHZpZXcgKiBtb2RlbCAqIHZlcnRleFBvc2l0aW9uOyB9";
    protected override string FragmentSource { get; } = "dW5pZm9ybSB2ZWM0IGNvbG9yOyB1bmlmb3JtIHZlYzQgbGlnaHREaXJlY3Rpb247IGluIHZlYzQgbjsgb3V0IHZlYzQgY29sb3IwOyB2b2lkIG1haW4oKSB7IHZlYzQgbm4gPSBub3JtYWxpemUobik7IHZlYzQgbmwgPSBub3JtYWxpemUobGlnaHREaXJlY3Rpb24pOyBjb2xvcjAgPSB2ZWM0KG1heCgwLjAsIGRvdChubiwgLW5sKSkgKiBjb2xvci5yZ2IsIDEuMCk7IH0=";
    [GlFragOut]
    public int Color0 { get; }
    //size 1, type Vector4
    [GlAttrib]
    public int VertexNormal { get; }    //size 1, type Vector4
    [GlAttrib]
    public int VertexPosition { get; }
    //size 1, type Vector4
    [GlUniform]
    private readonly int color;
    public void Color (in Vector4 v) => Uniform(color, v);

    //size 1, type Vector4
    [GlUniform]
    private readonly int lightDirection;
    public void LightDirection (in Vector4 v) => Uniform(lightDirection, v);

    //size 1, type Matrix4x4
    [GlUniform]
    private readonly int model;
    public void Model (in Matrix4x4 v) => Uniform(model, v);

    //size 1, type Matrix4x4
    [GlUniform]
    private readonly int projection;
    public void Projection (in Matrix4x4 v) => Uniform(projection, v);

    //size 1, type Matrix4x4
    [GlUniform]
    private readonly int view;
    public void View (in Matrix4x4 v) => Uniform(view, v);

#pragma warning restore CS0649
}