namespace Shaders;
using Gl;
using static Gl.Opengl;
using System.Numerics;
public static class Lines {
#pragma warning disable CS0649

    //size 1, type 35664
    [GlAttrib("coords")]
    public static int Coords { get; }

    //size 1, type Vector4
    [GlUniform("color")]
    private readonly static int color;
    public static void Color (Vector4 v) => Uniform(color, v);

    public static int Id { get; }
    static Lines () => ParsedShader.Prepare(typeof(Lines));
#pragma warning restore CS0649
}