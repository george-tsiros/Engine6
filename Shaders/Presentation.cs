
namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class Presentation:Program {
#pragma warning disable CS0649
    protected override string VertexSource { get; } = "aW4gdmVjMiB2ZXJ0ZXhQb3NpdGlvbjsgb3V0IHZlYzIgdXY7IHZvaWQgbWFpbigpIHsgdXYgPSAodmVydGV4UG9zaXRpb24gKyB2ZWMyKDEpKSAvIDI7IGdsX1Bvc2l0aW9uID0gdmVjNCh2ZXJ0ZXhQb3NpdGlvbiwgMCwgMSk7IH0=";
    protected override string FragmentSource { get; } = "dW5pZm9ybSBzYW1wbGVyMkQgdGV4MDsgaW4gdmVjMiB1djsgb3V0IHZlYzQgY29sb3IwOyB2b2lkIG1haW4oKSB7IGNvbG9yMCA9IHRleHR1cmUodGV4MCwgdXYpOyB9";
    [GlFragOut]
    public int Color0 { get; }
    //size 1, type 35664
    [GlAttrib]
    public int VertexPosition { get; }
    //size 1, type Sampler2D
    [GlUniform]
    private readonly int tex0;
    public void Tex0 (int v) => Uniform(tex0, v);

#pragma warning restore CS0649
}