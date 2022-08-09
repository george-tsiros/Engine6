namespace Shaders;
using Gl;
using static Gl.Opengl;
using System.Numerics;
using Linear;
public static class PassThrough {
#pragma warning disable CS0649

    //size 1, type 35666
    [GlAttrib("vertexPosition")]
    public static int VertexPosition { get; }

    //size 1, type Sampler2D
    [GlUniform("tex")]
    private readonly static int tex;
    public static void Tex (int v) => Uniform(tex, v);

    public static int Id { get; }
    static PassThrough () => ParsedShader.Prepare(typeof(PassThrough));
#pragma warning restore CS0649
}
