
namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class ViewProjection:Program {
#pragma warning disable CS0649
    protected override string VertexSource { get; } = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbjsgdW5pZm9ybSBtYXQ0IHZpZXcsIHByb2plY3Rpb247IHZvaWQgbWFpbiAoKSB7IGdsX1Bvc2l0aW9uID0gdmlldyAqIHByb2plY3Rpb24gKiB2ZXJ0ZXhQb3NpdGlvbjsgfQ==";
    protected override string FragmentSource { get; } = "b3V0IHZlYzQgb3V0MDsgdm9pZCBtYWluICgpIHsgb3V0MCA9IHZlYzQoMCwxLDAsMSk7IH0=";

    //size 1, type Vector4
    [GlAttrib("vertexPosition")]
    public int VertexPosition { get; }

    //size 1, type Matrix4x4
    [GlUniform("projection")]
    private readonly int projection;
    public void Projection (Matrix4x4 v) => Uniform(projection, v);

    //size 1, type Matrix4x4
    [GlUniform("view")]
    private readonly int view;
    public void View (Matrix4x4 v) => Uniform(view, v);

#pragma warning restore CS0649
}