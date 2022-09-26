
namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class TwoOutputs:Program {
#pragma warning disable CS0649
    protected override string VertexSource { get; } = "aW4gdmVjMiB2ZXJ0ZXhQb3NpdGlvbjsgdm9pZCBtYWluKCkgeyBnbF9Qb3NpdGlvbiA9IHZlYzQodmVydGV4UG9zaXRpb24sMCwxKTsgfQ==";
    protected override string FragmentSource { get; } = "dW5pZm9ybSB2ZWM0IGNvbG9yOyBvdXQgdmVjNCBjb2xvcjAsY29sb3IxOyB2b2lkIG1haW4oKSB7IGNvbG9yMCA9IGNvbG9yOyBjb2xvcjEgPSB2ZWM0KHZlYzMoMSktY29sb3IucmdiLCAxKTsgfQ==";
    //size 1, type 35664
    [GlAttrib]
    public int VertexPosition { get; }
    //size 1, type Vector4
    [GlUniform]
    private readonly int color;
    public void Color (in Vector4 v) => Uniform(color, v);

#pragma warning restore CS0649
}