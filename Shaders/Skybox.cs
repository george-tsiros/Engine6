
namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class Skybox:Program {
#pragma warning disable CS0649
    protected override string VertexSource { get; } = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbjsgaW4gdmVjMiB0ZXhDb29yZHM7IHVuaWZvcm0gbWF0NCBvcmllbnRhdGlvbiwgcHJvamVjdGlvbjsgb3V0IHZlYzIgdXY7IHZvaWQgbWFpbigpIHsgdXYgPSB0ZXhDb29yZHM7IGdsX1Bvc2l0aW9uID0gcHJvamVjdGlvbiAqIG9yaWVudGF0aW9uICogdmVydGV4UG9zaXRpb247IH0=";
    protected override string FragmentSource { get; } = "dW5pZm9ybSBzYW1wbGVyMkQgdGV4MDsgaW4gdmVjMiB1djsgb3V0IHZlYzQgY29sb3IwOyB2b2lkIG1haW4oKSB7IGNvbG9yMCA9IHRleHR1cmUodGV4MCwgdXYpOyB9";
    //size 1, type 35664
    [GlAttrib]
    public int TexCoords { get; }    //size 1, type Vector4
    [GlAttrib]
    public int VertexPosition { get; }
    //size 1, type Matrix4x4
    [GlUniform]
    private readonly int orientation;
    public void Orientation (in Matrix4x4 v) => Uniform(orientation, v);

    //size 1, type Matrix4x4
    [GlUniform]
    private readonly int projection;
    public void Projection (in Matrix4x4 v) => Uniform(projection, v);

    //size 1, type Sampler2D
    [GlUniform]
    private readonly int tex0;
    public void Tex0 (int v) => Uniform(tex0, v);

#pragma warning restore CS0649
}