namespace Shaders;

using Gl;
using static Gl.RenderingContext;
using System.Numerics;
using Common;

public class Axes:Program {
#pragma warning disable CS0649
    protected override string VertexSource { get; } = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbiwgY29sb3I7IG91dCB2ZWM0IGNvbG9yMDsgdW5pZm9ybSBtYXQ0IG1vZGVsLCB2aWV3LCBwcm9qZWN0aW9uOyB2b2lkIG1haW4gKCkgeyBnbF9Qb3NpdGlvbiA9IHByb2plY3Rpb24gKiB2aWV3ICogbW9kZWwgKiB2ZXJ0ZXhQb3NpdGlvbjsgY29sb3IwID0gY29sb3I7IH0=";
    protected override string FragmentSource { get; } = "aW4gdmVjNCBjb2xvcjA7IG91dCB2ZWM0IG91dDA7IHZvaWQgbWFpbiAoKSB7IG91dDAgPSBjb2xvcjA7IH0=";

    //size 1, type Vector4
    [GlAttrib("color")]
    public int Color { get; }

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

#pragma warning restore CS0649
}