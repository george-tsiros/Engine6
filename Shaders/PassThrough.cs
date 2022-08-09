namespace Shaders;
using Gl;
using static Gl.Opengl;
using System.Numerics;
using Linear;
public static class PassThrough {
#pragma warning disable CS0649
    public const string VertexSource = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbjsgb3V0IHZlYzIgbm90X0ZyYWdQb3M7IHZvaWQgbWFpbiAoKSB7IG5vdF9GcmFnUG9zID0gMC41ZiAqIHZlcnRleFBvc2l0aW9uLnh5ICsgMC41OyBub3RfRnJhZ1BvcyA9IHZlYzIobm90X0ZyYWdQb3MueCwgbm90X0ZyYWdQb3MueSk7IGdsX1Bvc2l0aW9uID0gdmVjNCh2ZXJ0ZXhQb3NpdGlvbi54eSwgMC4wLCAxLjApOyB9";
    public const string FragmentSource = "aW4gdmVjMiBub3RfRnJhZ1BvczsgdW5pZm9ybSBzYW1wbGVyMkQgdGV4OyBvdXQgdmVjNCBvdXQwOyB2b2lkIG1haW4gKCkgeyBvdXQwID0gdGV4dHVyZSh0ZXgsIG5vdF9GcmFnUG9zKTsgfQ==";

    //size 1, type 35666
    [GlAttrib("vertexPosition")]
    public static int VertexPosition { get; }

    //size 1, type Sampler2D
    [GlUniform("tex")]
    private readonly static int tex;
    public static void Tex (int v) => Uniform(tex, v);

    public static int Id { get; }
    static PassThrough () => ParsedShader.Prepare(typeof(PassThrough));
#pragma warning restore CS0649
}
