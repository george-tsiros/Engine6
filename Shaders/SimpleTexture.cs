namespace Shaders;
using Gl;
using static Gl.Opengl;
using System.Numerics;
public static class SimpleTexture {
#pragma warning disable CS0649

    //size 1, type 35676
    [GlAttrib("model")]
    public static int Model { get; }

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
    static SimpleTexture () => ParsedShader.Prepare(typeof(SimpleTexture));
#pragma warning restore CS0649
}
