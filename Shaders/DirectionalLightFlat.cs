namespace Shaders;
using Gl;
using static Gl.Opengl;
using System.Numerics;
public static class DirectionalLightFlat {
#pragma warning disable CS0649

    //size 1, type 35666
    [GlAttrib("normal")]
    public static int Normal { get; }

    //size 1, type 35666
    [GlAttrib("vertex")]
    public static int Vertex { get; }

    //size 1, type Vector4
    [GlUniform("color")]
    private readonly static int color;
    public static void Color (Vector4 v) => Uniform(color, v);

    //size 1, type Vector4
    [GlUniform("lightDirection")]
    private readonly static int lightDirection;
    public static void LightDirection (Vector4 v) => Uniform(lightDirection, v);

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

    public static int Id { get; }
    static DirectionalLightFlat () => ParsedShader.Prepare(typeof(DirectionalLightFlat));
#pragma warning restore CS0649
}