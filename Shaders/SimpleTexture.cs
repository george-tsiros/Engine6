namespace Shaders;

using Gl;
using static Gl.RenderingContext;
using System.Numerics;
using Common;

public class SimpleTexture:Program {
#pragma warning disable CS0649
    protected override string VertexSource { get; } = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbjsgaW4gdmVjMiB2ZXJ0ZXhVVjsgaW4gbWF0NCBtb2RlbDsgdW5pZm9ybSBtYXQ0IHZpZXcsIHByb2plY3Rpb247IG91dCB2ZWMyIHV2OyB2b2lkIG1haW4gKCkgeyB1diA9IHZlcnRleFVWOyBnbF9Qb3NpdGlvbiA9IHByb2plY3Rpb24gKiB2aWV3ICogbW9kZWwgKiB2ZXJ0ZXhQb3NpdGlvbjsgfQ==";
    protected override string FragmentSource { get; } = "aW4gdmVjMiB1djsgdW5pZm9ybSBzYW1wbGVyMkQgdGV4OyBvdXQgdmVjNCBvdXQwOyB2b2lkIG1haW4gKCkgeyBvdXQwID0gdGV4dHVyZSh0ZXgsIHV2KTsgfQ==";

    //size 1, type 35676
    [GlAttrib("model")]
    public int Model { get; }

    //size 1, type Vector4
    [GlAttrib("vertexPosition")]
    public int VertexPosition { get; }

    //size 1, type 35664
    [GlAttrib("vertexUV")]
    public int VertexUV { get; }

    //size 1, type Matrix4x4
    [GlUniform("projection")]
    private readonly int projection;
    public void Projection (Matrix4x4 v) => Uniform(projection, v);

    //size 1, type Matrix4x4
    [GlUniform("view")]
    private readonly int view;
    public void View (Matrix4x4 v) => Uniform(view, v);

    //size 1, type Sampler2D
    [GlUniform("tex")]
    private readonly int tex;
    public void Tex (int v) => Uniform(tex, v);

#pragma warning restore CS0649
}