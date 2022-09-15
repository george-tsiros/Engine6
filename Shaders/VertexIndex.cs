namespace Shaders;

using Gl;
using static Gl.RenderingContext;
using System.Numerics;
using Common;

public class VertexIndex:Program {
#pragma warning disable CS0649
    protected override string VertexSource { get; } = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbjsgdW5pZm9ybSBtYXQ0IG1vZGVsLCB2aWV3LCBwcm9qZWN0aW9uOyBmbGF0IG91dCBpbnQgdmVydGV4SWQ7IHZvaWQgbWFpbiAoKSB7IGdsX1Bvc2l0aW9uID0gcHJvamVjdGlvbiAqIHZpZXcgKiBtb2RlbCAqIHZlcnRleFBvc2l0aW9uOyB2ZXJ0ZXhJZCA9IGdsX1ZlcnRleElEICsgMTsgfQ==";
    protected override string FragmentSource { get; } = "dW5pZm9ybSB2ZWM0IGNvbG9yMCwgY29sb3IxOyB1bmlmb3JtIGludCB0cmk7IG91dCB2ZWM0IG91dDA7IGZsYXQgaW4gaW50IHZlcnRleElkOyBvdXQgaW50IG91dDE7IHZvaWQgbWFpbiAoKSB7IG91dDAgPSAodHJpID09IHZlcnRleElkIC8gMykgPyBjb2xvcjEgOiBjb2xvcjA7IG91dDEgPSB2ZXJ0ZXhJZDsgfQ==";

    //size 1, type Vector4
    [GlAttrib("vertexPosition")]
    public int VertexPosition { get; }

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

    //size 1, type Vector4
    [GlUniform("color0")]
    private readonly int color0;
    public void Color0 (Vector4 v) => Uniform(color0, v);

    //size 1, type Vector4
    [GlUniform("color1")]
    private readonly int color1;
    public void Color1 (Vector4 v) => Uniform(color1, v);

    //size 1, type Int
    [GlUniform("tri")]
    private readonly int tri;
    public void Tri (int v) => Uniform(tri, v);

#pragma warning restore CS0649
}