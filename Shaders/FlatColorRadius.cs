
namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class FlatColorRadius:Program {
#pragma warning disable CS0649
    protected override string VertexSource { get; } = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbjsgdW5pZm9ybSBmbG9hdCBzY2FsZTsgdW5pZm9ybSBtYXQ0IG1vZGVsLCB2aWV3LCBwcm9qZWN0aW9uOyB2b2lkIG1haW4oKSB7IGdsX1Bvc2l0aW9uID0gcHJvamVjdGlvbiAqIHZpZXcgKiBtb2RlbCAqIHNjYWxlICogdmVydGV4UG9zaXRpb247IH0=";
    protected override string FragmentSource { get; } = "dW5pZm9ybSB2ZWM0IGNvbG9yOyBvdXQgdmVjNCBjb2xvcjA7IHZvaWQgbWFpbigpIHsgY29sb3IwID0gY29sb3I7IH0=";
    [GlFragOut]
    public int Color0 { get; }
    //size 1, type Vector4
    [GlAttrib]
    public int VertexPosition { get; }
    //size 1, type Vector4
    [GlUniform]
    private readonly int color;
    public void Color (in Vector4 v) => Uniform(color, v);

    public FragOut Col0 { get; }
    public Attrib<Vector4> VPos { get; }
    //size 1, type Matrix4x4
    [GlUniform]
    private readonly int model;
    public void Model (in Matrix4x4 v) => Uniform(model, v);

    //size 1, type Matrix4x4
    [GlUniform]
    private readonly int projection;
    public void Projection (in Matrix4x4 v) => Uniform(projection, v);

    //size 1, type Float
    [GlUniform]
    private readonly int scale;
    public void Scale (float v) => Uniform(scale, v);

    //size 1, type Matrix4x4
    [GlUniform]
    private readonly int view;
    public void View (in Matrix4x4 v) => Uniform(view, v);

#pragma warning restore CS0649
}