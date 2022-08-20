namespace Shaders;

using Gl;
using static Gl.Opengl;
using System.Numerics;
using Linear;

public class PassThrough:Program {
#pragma warning disable CS0649
    protected override string VertexSource { get; } = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbjsgb3V0IHZlYzIgbm90X0ZyYWdQb3M7IHZvaWQgbWFpbiAoKSB7IG5vdF9GcmFnUG9zID0gMC41ICogdmVydGV4UG9zaXRpb24ueHkgKyAwLjU7IGdsX1Bvc2l0aW9uID0gdmVjNCh2ZXJ0ZXhQb3NpdGlvbi54eSwgMC4wLCAxLjApOyB9";
    protected override string FragmentSource { get; } = "aW4gdmVjMiBub3RfRnJhZ1BvczsgdW5pZm9ybSBzYW1wbGVyMkQgdGV4OyBvdXQgdmVjNCBvdXQwOyB2b2lkIG1haW4gKCkgeyBvdXQwID0gdGV4dHVyZSh0ZXgsIG5vdF9GcmFnUG9zKTsgfQ==";

    //size 1, type 35666
    [GlAttrib("vertexPosition")]
    public int VertexPosition { get; }

    //size 1, type Sampler2D
    [GlUniform("tex")]
    private readonly int tex;
    public void Tex (int v) => Uniform(tex, v);

#pragma warning restore CS0649
}
