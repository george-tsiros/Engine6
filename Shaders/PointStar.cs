namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class PointStar:Program {
#pragma warning disable CS0649

    protected override string VertexSource { get; } = "dW5pZm9ybSBtYXQ0IHZpZXcsIHByb2plY3Rpb247IGluIHZlYzQgdmVydGV4UG9zaXRpb247IHZvaWQgbWFpbigpIHsgZ2xfUG9zaXRpb24gPSBwcm9qZWN0aW9uICogdmlldyAqIHZlcnRleFBvc2l0aW9uOyB9";
    protected override string FragmentSource { get; } = "b3V0IHZlYzQgY29sb3IwOyB2b2lkIG1haW4oKSB7IGNvbnN0IGZsb2F0IHBpID0gMy4xNDE1OTI2NTM1ODk3OTMxOyB2ZWMyIHBwID0gcGkgKiBnbF9Qb2ludENvb3JkLnh5OyBmbG9hdCBpbnRlbnNpdHkgPSAocG93KHNpbihwcC55KSwxMCkgKyBwb3coc2luKHBwLngpLDEwKSkgKiAwLjU7IGNvbG9yMCA9IHZlYzQodmVjMyhpbnRlbnNpdHkpLDEpOyB9";

    public FragOut Color0 { get; }

    public Attrib<Vector4> VertexPosition { get; }

    private readonly int projection;
    public void Projection (in Matrix4x4 v) => Uniform(projection, v);

    private readonly int view;
    public void View (in Matrix4x4 v) => Uniform(view, v);

    public PointStar () {
        view = GetUniformLocation(this, nameof(view));
        projection = GetUniformLocation(this, nameof(projection));
        VertexPosition = GetAttribLocation(this, "vertexPosition");
        Color0 = GetFragDataLocation(this, "color0");
    }

#pragma warning restore CS0649
}