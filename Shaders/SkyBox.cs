namespace Shaders;
using Gl;
using static Gl.Opengl;
using System.Numerics;
using Linear;
public static class SkyBox {
#pragma warning disable CS0649
    public const string VertexSource = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbjsgaW4gdmVjMiB2ZXJ0ZXhVVjsgdW5pZm9ybSBtYXQ0IHZpZXcsIHByb2plY3Rpb247IG91dCB2ZWMyIHV2OyB2b2lkIG1haW4gKCkgeyB1diA9IHZlcnRleFVWOyB2ZWM0IHAgPSBwcm9qZWN0aW9uICogdmlldyAqIHZlcnRleFBvc2l0aW9uOyBnbF9Qb3NpdGlvbiA9IHAueHl3dzsgfQ==";
    public const string FragmentSource = "aW4gdmVjMiB1djsgdW5pZm9ybSBzYW1wbGVyMkQgdGV4OyBvdXQgdmVjNCBvdXQwOyB2b2lkIG1haW4gKCkgeyBvdXQwID0gdGV4dHVyZSh0ZXgsIHV2KTsgfQ==";

    //size 1, type 35666
    [GlAttrib("vertexPosition")]
    public static int VertexPosition { get; }

    //size 1, type 35664
    [GlAttrib("vertexUV")]
    public static int VertexUV { get; }

    //size 1, type Matrix4x4
    [GlUniform("projection")]
    private readonly static int projection;
    public static void Projection (Matrix4x4 v) => Uniform(projection, v);

    //size 1, type Matrix4x4
    [GlUniform("view")]
    private readonly static int view;
    public static void View (Matrix4x4 v) => Uniform(view, v);

    //size 1, type Sampler2D
    [GlUniform("tex")]
    private readonly static int tex;
    public static void Tex (int v) => Uniform(tex, v);

    public static int Id { get; }
    static SkyBox () => ParsedShader.Prepare(typeof(SkyBox));
#pragma warning restore CS0649
}
