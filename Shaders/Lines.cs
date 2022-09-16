namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class Lines:Program {
#pragma warning disable CS0649
    protected override string VertexSource { get; } = "dW5pZm9ybSBpdmVjMiByZW5kZXJTaXplOyB1bmlmb3JtIGl2ZWMyIG9mZnNldDsgaW4gaXZlYzIgdmVydGV4UG9zaXRpb247IHZvaWQgbWFpbiAoKSB7IHZlYzIgYSA9IDIgLyAocmVuZGVyU2l6ZSAtIHZlYzIoMS4wKSk7IGdsX1Bvc2l0aW9uID0gdmVjNCgodmVydGV4UG9zaXRpb24gKyBvZmZzZXQpICogYSAtIHZlYzIoMS4wKSwgMC4wLCAxLjApOyB9";
    protected override string FragmentSource { get; } = "dW5pZm9ybSB2ZWM0IGNvbG9yOyBvdXQgdmVjNCBvdXQwOyB2b2lkIG1haW4gKCkgeyBvdXQwID0gY29sb3I7IH0=";

    //size 1, type 35667
    [GlAttrib("vertexPosition")]
    public int VertexPosition { get; }

    //size 1, type Vector4
    [GlUniform("color")]
    private readonly int color;
    public void Color (Vector4 v) => Uniform(color, v);

    //size 1, type Vector2i
    [GlUniform("offset")]
    private readonly int offset;
    public void Offset (Vector2i v) => Uniform(offset, v);

    //size 1, type Vector2i
    [GlUniform("renderSize")]
    private readonly int renderSize;
    public void RenderSize (Vector2i v) => Uniform(renderSize, v);

#pragma warning restore CS0649
}