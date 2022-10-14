namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class DirectionalInstanced:Program {
#pragma warning disable CS0649

    protected override string VertexSource { get; } = "aW4gdmVjNCB2ZXJ0ZXhQb3NpdGlvbjsgaW4gdmVjMyB2ZXJ0ZXhOb3JtYWw7IGluIG1hdDQgbW9kZWw7IHVuaWZvcm0gbWF0NCB2aWV3LCBwcm9qZWN0aW9uOyBvdXQgdmVjNCBuOyBpbiB2ZWM0IGNvbG9yOyBmbGF0IG91dCB2ZWM0IGM7IHZvaWQgbWFpbigpIHsgYyA9IGNvbG9yOyBuID0gbW9kZWwgKiB2ZWM0KHZlcnRleE5vcm1hbCwwKTsgZ2xfUG9zaXRpb24gPSBwcm9qZWN0aW9uICogdmlldyAqIG1vZGVsICogdmVydGV4UG9zaXRpb247IH0=";
    protected override string FragmentSource { get; } = "dW5pZm9ybSB2ZWM0IGxpZ2h0RGlyZWN0aW9uOyBpbiB2ZWM0IG47IGluIHZlYzQgYzsgb3V0IHZlYzQgY29sb3IwOyB2b2lkIG1haW4oKSB7IHZlYzQgbm4gPSBub3JtYWxpemUobik7IHZlYzQgbmwgPSBub3JtYWxpemUobGlnaHREaXJlY3Rpb24pOyBmbG9hdCBkID0gbWF4KGRvdChubiwtbmwpLCAwLjApOyBjb2xvcjAgPSB2ZWM0KGMucmdiICogKDAuOSAqIGQgKyAwLjEpLCAxLjApOyB9";

    public FragOut Color0 { get; }

    public Attrib<Vector4> Color { get; }

    public Attrib<Matrix4x4> Model { get; }

    public Attrib<Vector3> VertexNormal { get; }

    public Attrib<Vector4> VertexPosition { get; }

    private readonly int projection;
    public void Projection (in Matrix4x4 v) => Uniform(projection, v);

    private readonly int view;
    public void View (in Matrix4x4 v) => Uniform(view, v);

    private readonly int lightDirection;
    public void LightDirection (in Vector4 v) => Uniform(lightDirection, v);

    public DirectionalInstanced () {
        lightDirection = GetUniformLocation(this, nameof(lightDirection));
        view = GetUniformLocation(this, nameof(view));
        projection = GetUniformLocation(this, nameof(projection));
        VertexPosition = GetAttribLocation(this, "vertexPosition");
        VertexNormal = GetAttribLocation(this, "vertexNormal");
        Model = GetAttribLocation(this, "model");
        Color = GetAttribLocation(this, "color");
        Color0 = GetFragDataLocation(this, "color0");
    }

#pragma warning restore CS0649
}