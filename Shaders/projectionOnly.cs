
namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class ProjectionOnly:Program {
#pragma warning disable CS0649
    protected override string VertexSource { get; } = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbjsgdW5pZm9ybSBtYXQ0IHByb2plY3Rpb247IHZvaWQgbWFpbiAoKSB7IGdsX1Bvc2l0aW9uID0gcHJvamVjdGlvbiAqIHZlcnRleFBvc2l0aW9uOyB9";
    protected override string FragmentSource { get; } = "b3V0IHZlYzQgb3V0MDsgdm9pZCBtYWluICgpIHsgb3V0MCA9IHZlYzQoMSwwLDAsMSk7IH0=";

    //size 1, type Vector4
    [GlAttrib("vertexPosition")]
    public int VertexPosition { get; }

    //size 1, type Matrix4x4
    [GlUniform("projection")]
    private readonly int projection;
    public void Projection (in Matrix4x4 v) => Uniform(projection, v);

#pragma warning restore CS0649
}