namespace Shaders;
using Gl;
using static Gl.Opengl;
using System.Numerics;
using Linear;
public static class DirectionalFlat {
#pragma warning disable CS0649

    //size 1, type 35666
    [GlAttrib("faceNormal")]
    public static int FaceNormal { get; }

    //size 1, type 35666
    [GlAttrib("vertexPosition")]
    public static int VertexPosition { get; }

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
    static DirectionalFlat () => ParsedShader.Prepare(typeof(DirectionalFlat));
#pragma warning restore CS0649
}
