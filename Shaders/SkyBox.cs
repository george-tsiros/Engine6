namespace Shaders;

using Gl;
using static Gl.RenderingContext;
using System.Numerics;
using Common;

public class SkyBox:Program {
#pragma warning disable CS0649
    protected override string VertexSource { get; } = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbjsgaW4gdmVjMiB2ZXJ0ZXhVVjsgdW5pZm9ybSBtYXQ0IHZpZXcsIHByb2plY3Rpb247IG91dCB2ZWMyIHV2OyB2b2lkIG1haW4gKCkgeyB1diA9IHZlcnRleFVWOyB2ZWM0IHAgPSBwcm9qZWN0aW9uICogdmlldyAqIHZlcnRleFBvc2l0aW9uOyBnbF9Qb3NpdGlvbiA9IHAueHl3dzsgfQ==";
    protected override string FragmentSource { get; } = "aW4gdmVjMiB1djsgdW5pZm9ybSBzYW1wbGVyMkQgdGV4OyBvdXQgdmVjNCBvdXQwOyB2b2lkIG1haW4gKCkgeyBvdXQwID0gdGV4dHVyZSh0ZXgsIHV2KTsgfQ==";

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