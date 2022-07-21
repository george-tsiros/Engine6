namespace Shaders;
using Gl;
using static Gl.Opengl;
using System.Numerics;
public static class VertexIndex {
#pragma warning disable CS0649

    //size 1, type 35666
    [GlAttrib("vertexPosition")]
    public static int VertexPosition { get; }

    //size 1, type Vector4
    [GlUniform("color0")]
    private readonly static int color0;
    public static void Color0 (Vector4 v) => Uniform(color0, v);

    //size 1, type Vector4
    [GlUniform("color1")]
    private readonly static int color1;
    public static void Color1 (Vector4 v) => Uniform(color1, v);

    //size 1, type Matrix4x4
    [GlUniform("model")]
    private readonly static int model;
    public static void Model (Matrix4x4 v) => Uniform(model, v);

    //size 1, type Matrix4x4
    [GlUniform("projection")]
    private readonly static int projection;
    public static void Projection (Matrix4x4 v) => Uniform(projection, v);

    //size 1, type Int
    [GlUniform("tri")]
    private readonly static int tri;
    public static void Tri (int v) => Uniform(tri, v);

    //size 1, type Matrix4x4
    [GlUniform("view")]
    private readonly static int view;
    public static void View (Matrix4x4 v) => Uniform(view, v);

    public static int Id { get; }
    static VertexIndex () => ParsedShader.Prepare(typeof(VertexIndex));
#pragma warning restore CS0649
}
