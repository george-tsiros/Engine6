namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class FlatColorPlanet:Program {
#pragma warning disable CS0649

    protected override string VertexSource { get; } = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbjsgdW5pZm9ybSBmbG9hdCByYWRpdXM7IHVuaWZvcm0gbWF0NCBtb2RlbCwgdmlldywgcHJvamVjdGlvbjsgdm9pZCBtYWluKCkgeyBnbF9Qb3NpdGlvbiA9IHByb2plY3Rpb24gKiB2aWV3ICogbW9kZWwgKiB2ZWM0KHZlYzMocmFkaXVzKSwgMSkgKiB2ZXJ0ZXhQb3NpdGlvbjsgfQ==";
    protected override string FragmentSource { get; } = "dW5pZm9ybSB2ZWM0IGNvbG9yOyBvdXQgdmVjNCBjb2xvcjA7IHZvaWQgbWFpbigpIHsgY29sb3IwID0gY29sb3I7IH0=";

    public FragOut Color0 { get; }

    public Attrib<Vector4> VertexPosition { get; }

    private readonly int model;
    public void Model (in Matrix4x4 v) => Uniform(model, v);

    private readonly int projection;
    public void Projection (in Matrix4x4 v) => Uniform(projection, v);

    private readonly int radius;
    public void Radius (float v) => Uniform(radius, v);

    private readonly int view;
    public void View (in Matrix4x4 v) => Uniform(view, v);

    private readonly int color;
    public void Color (in Vector4 v) => Uniform(color, v);

    public FlatColorPlanet () {
        color = GetUniformLocation(this, nameof(color));
        view = GetUniformLocation(this, nameof(view));
        radius = GetUniformLocation(this, nameof(radius));
        projection = GetUniformLocation(this, nameof(projection));
        model = GetUniformLocation(this, nameof(model));
        VertexPosition = GetAttribLocation(this, "vertexPosition");
        Color0 = GetFragDataLocation(this, "color0");
    }

#pragma warning restore CS0649
}