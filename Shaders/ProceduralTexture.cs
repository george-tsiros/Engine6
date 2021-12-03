namespace Shaders;
using Gl;
using static Gl.Opengl;
using System.Numerics;
public static class ProceduralTexture {
#pragma warning disable CS0649

    //size 1, type 35666
    [GlAttrib("vertexPosition")]
    public static int VertexPosition { get; }

    //size 1, type 35664
    [GlAttrib("vertexUV")]
    public static int VertexUV { get; }

    //size 1, type Matrix4x4
    [GlUniform("model")]
    private readonly static int model;
    public static void Model (Matrix4x4 v) => Uniform(model, v);

    //size 1, type Matrix4x4
    [GlUniform("projection")]
    private readonly static int projection;
    public static void Projection (Matrix4x4 v) => Uniform(projection, v);

    //size 1, type Float
    [GlUniform("theta")]
    private readonly static int theta;
    public static void Theta (float v) => Uniform(theta, v);

    //size 1, type Matrix4x4
    [GlUniform("view")]
    private readonly static int view;
    public static void View (Matrix4x4 v) => Uniform(view, v);

    public static int Id { get; }
    static ProceduralTexture () => ParsedShader.Prepare(typeof(ProceduralTexture));
#pragma warning restore CS0649
}