namespace Shaders;
using Gl;
using static Gl.Opengl;
using System.Numerics;
using Linear;
public static class Lines {
#pragma warning disable CS0649
    public const string VertexSource = "dW5pZm9ybSBpdmVjMiByZW5kZXJTaXplOyB1bmlmb3JtIGl2ZWMyIG9mZnNldDsgaW4gaXZlYzIgdmVydGV4UG9zaXRpb247IHZvaWQgbWFpbiAoKSB7IHZlYzIgYSA9IDIgLyAocmVuZGVyU2l6ZSAtIHZlYzIoMS4wKSk7IGdsX1Bvc2l0aW9uID0gdmVjNCgodmVydGV4UG9zaXRpb24gKyBvZmZzZXQpICogYSAtIHZlYzIoMS4wKSwgMC4wLCAxLjApOyB9";
    public const string FragmentSource = "dW5pZm9ybSB2ZWM0IGNvbG9yOyBvdXQgdmVjNCBvdXQwOyB2b2lkIG1haW4gKCkgeyBvdXQwID0gY29sb3I7IH0=";

    //size 1, type 35667
    [GlAttrib("vertexPosition")]
    public static int VertexPosition { get; }

    //size 1, type Vector4
    [GlUniform("color")]
    private readonly static int color;
    public static void Color (Vector4 v) => Uniform(color, v);

    //size 1, type Vector2i
    [GlUniform("offset")]
    private readonly static int offset;
    public static void Offset (Vector2i v) => Uniform(offset, v);

    //size 1, type Vector2i
    [GlUniform("renderSize")]
    private readonly static int renderSize;
    public static void RenderSize (Vector2i v) => Uniform(renderSize, v);

    public static int Id { get; }
    static Lines () => ParsedShader.Prepare(typeof(Lines));
#pragma warning restore CS0649
}
