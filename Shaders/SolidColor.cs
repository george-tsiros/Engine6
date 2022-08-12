namespace Shaders;
using Gl;
using static Gl.Opengl;
using System.Numerics;
using Linear;
public static class SolidColor {
#pragma warning disable CS0649
    public const string VertexSource = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbjsgdW5pZm9ybSBtYXQ0IG1vZGVsLCB2aWV3LCBwcm9qZWN0aW9uOyB2b2lkIG1haW4gKCkgeyBnbF9Qb3NpdGlvbiA9IHByb2plY3Rpb24gKiB2aWV3ICogbW9kZWwgKiB2ZXJ0ZXhQb3NpdGlvbjsgfQ==";
    public const string FragmentSource = "dW5pZm9ybSB2ZWM0IGNvbG9yOyBvdXQgdmVjNCBvdXQwOyB2b2lkIG1haW4gKCkgeyBvdXQwID0gY29sb3I7IH0=";

    //size 1, type 35666
    [GlAttrib("vertexPosition")]
    public static int VertexPosition { get; }

    //size 1, type Matrix4x4
    [GlUniform("model")]
    private readonly static int model;
    public static void Model (Matrix4x4 v) => Uniform(model, v);

    //size 1, type Matrix4x4
    [GlUniform("projection")]
    private readonly static int projection;
    public static void Projection (Matrix4x4 v) => Uniform(projection, v);

    //size 1, type Matrix4x4
    [GlUniform("view")]
    private readonly static int view;
    public static void View (Matrix4x4 v) => Uniform(view, v);

    //size 1, type Vector4
    [GlUniform("color")]
    private readonly static int color;
    public static void Color (Vector4 v) => Uniform(color, v);

    public static int Id { get; }
    static SolidColor () => ParsedShader.Prepare(typeof(SolidColor));
#pragma warning restore CS0649
}
