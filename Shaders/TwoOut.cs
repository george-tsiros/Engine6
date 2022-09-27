
namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class TwoOut:Program {
#pragma warning disable CS0649
    protected override string VertexSource { get; } = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbjsgdW5pZm9ybSBtYXQ0IG1vZGVsLCB2aWV3LCBwcm9qZWN0aW9uOyB2b2lkIG1haW4oKSB7IGdsX1Bvc2l0aW9uID0gcHJvamVjdGlvbiAqIHZpZXcgKiBtb2RlbCAqIHZlcnRleFBvc2l0aW9uOyB9";
    protected override string FragmentSource { get; } = "dW5pZm9ybSB2ZWM0IGNvbG9yOyBvdXQgdmVjNCBjb2xvcjAsIGNvbG9yMTsgdm9pZCBtYWluKCkgeyBjb2xvcjAgPSBjb2xvcjsgY29sb3IxID0gdmVjNCh2ZWMzKDEpIC0gY29sb3IucmdiLDEpOyB9";
    //size 1, type Vector4
    [GlAttrib]
    public int VertexPosition { get; }
    //size 1, type Vector4
    [GlUniform]
    private readonly int color;
    public void Color (in Vector4 v) => Uniform(color, v);

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