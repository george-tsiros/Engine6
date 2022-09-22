
namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class BitmapText:Program {
#pragma warning disable CS0649
    protected override string VertexSource { get; } = "dW5pZm9ybSBpbnQgZm9udFdpZHRoOyB1bmlmb3JtIGl2ZWMyIG9mZnNldDsgaW4gaXZlYzIgdmVydGV4UG9zaXRpb247IG91dCB2ZWMyIHV2OyB2b2lkIG1haW4gKCkgeyB1diA9IHZlYzIoMSk7IGdsX1Bvc2l0aW9uID0gdmVjNCh2ZXJ0ZXhQb3NpdGlvbi54eSwwLjAsIDEuMCk7IH0=";
    protected override string FragmentSource { get; } = "dW5pZm9ybSBzYW1wbGVyMkQgdGV4OyBpbiB2ZWMyIHV2OyBvdXQgdmVjNCBvdXQwOyB2b2lkIG1haW4gKCkgeyBvdXQwID0gdGV4dHVyZSh0ZXgsIHV2KTsgfQ==";
    //size 1, type 35667
    [GlAttrib]
    public int VertexPosition { get; }
    //size 1, type Sampler2D
    [GlUniform]
    private readonly int tex;
    public void Tex (int v) => Uniform(tex, v);

#pragma warning restore CS0649
}