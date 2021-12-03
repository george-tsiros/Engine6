namespace Shaders;
using Gl;
using static Gl.Opengl;
using System.Numerics;
public static class RasterFont {
#pragma warning disable CS0649

    //size 1, type 35664
    [GlAttrib("char")]
    public static int Char { get; }

    //size 1, type Int
    [GlAttrib("gl_InstanceID")]
    public static int Gl_InstanceID { get; }

    //size 1, type 35664
    [GlAttrib("offset")]
    public static int Offset { get; }

    //size 1, type 35664
    [GlAttrib("vertex")]
    public static int Vertex { get; }

    //size 1, type Vector2
    [GlUniform("fontSize")]
    private readonly static int fontSize;
    public static void FontSize (Vector2 v) => Uniform(fontSize, v);

    //size 1, type Vector2
    [GlUniform("screenSize")]
    private readonly static int screenSize;
    public static void ScreenSize (Vector2 v) => Uniform(screenSize, v);

    //size 1, type Sampler2D
    [GlUniform("font")]
    private readonly static int font;
    public static void Font (int v) => Uniform(font, v);

    public static int Id { get; }
    static RasterFont () => ParsedShader.Prepare(typeof(RasterFont));
#pragma warning restore CS0649
}