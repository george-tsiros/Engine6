namespace Shaders;
using Gl;
using static Gl.Opengl;
using System.Numerics;
using Linear;
public static class BitmapText {
#pragma warning disable CS0649
    public const string VertexSource = "dW5pZm9ybSBpbnQgZm9udFdpZHRoOyB1bmlmb3JtIGl2ZWMyIG9mZnNldDsgaW4gaXZlYzIgdmVydGV4UG9zaXRpb247IHZvaWQgbWFpbiAoKSB7IGdsX1Bvc2l0aW9uID0gdmVjNCh2ZXJ0ZXhQb3NpdGlvbi54eSwwLjAsIDEuMCk7IH0=";
    public const string FragmentSource = "dW5pZm9ybSBzYW1wbGVyMkQgdGV4OyBpbiB2ZWMyIHV2OyBvdXQgdmVjNCBvdXQwOyB2b2lkIG1haW4gKCkgeyBvdXQwID0gdGV4dHVyZSh0ZXgsIHV2KTsgfQ==";

    //size 1, type 35667
    [GlAttrib("vertexPosition")]
    public static int VertexPosition { get; }

    //size 1, type Sampler2D
    [GlUniform("tex")]
    private readonly static int tex;
    public static void Tex (int v) => Uniform(tex, v);

    public static int Id { get; }
    static BitmapText () => ParsedShader.Prepare(typeof(BitmapText));
#pragma warning restore CS0649
}
