namespace Shaders;
using Gl;
using static Gl.Opengl;
using System.Numerics;
using Linear;
public static class Gui {
#pragma warning disable CS0649

    //size 1, type 35667
    [GlAttrib("vertexPosition")]
    public static int VertexPosition { get; }

    //size 1, type Vector4
    [GlUniform("color")]
    private readonly static int color;
    public static void Color (Vector4 v) => Uniform(color, v);

    //size 1, type Vector2i
    [GlUniform("renderSize")]
    private readonly static int renderSize;
    public static void RenderSize (Vector2i v) => Uniform(renderSize, v);

    public static int Id { get; }
    static Gui () => ParsedShader.Prepare(typeof(Gui));
#pragma warning restore CS0649
}
