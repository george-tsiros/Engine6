namespace Engine6;
using Win32;
using Common;
using Gl;
using static Gl.GlContext;
using System.Numerics;
using Shaders;
using System;
public class Experiment:GlWindow {
    static readonly (int, int)[] C3_lines = { (0, 1), (0, 4), (1, 3), (3, 8), (4, 7), (6, 7), (6, 9), (5, 9), (5, 8), (2, 5), (2, 6), (3, 5), (4, 6), (1, 2), (0, 2), (8, 10), (10, 11), (7, 11), (1, 10), (0, 11), (1, 5), (0, 6), (20, 21), (12, 13), (18, 19), (14, 15), (16, 17), (15, 16), (14, 17), (13, 18), (12, 19), (2, 9), (22, 24), (23, 24), (22, 23), (25, 26), (26, 27), (25, 27), };
    static readonly Vector3[] C3_vertices = { new(32, 0, 76), new(-32, 0, 76), new(0, 26, 24), new(-120, -3, -8), new(120, -3, -8), new(-88, 16, -40), new(88, 16, -40), new(128, -8, -40), new(-128, -8, -40), new(0, 26, -40), new(-32, -24, -40), new(32, -24, -40), new(-36, 8, -40), new(-8, 12, -40), new(8, 12, -40), new(36, 8, -40), new(36, -12, -40), new(8, -16, -40), new(-8, -16, -40), new(-36, -12, -40), new(0, 0, 76), new(0, 0, 90), new(-80, -6, -40), new(-80, 6, -40), new(-88, 0, -40), new(80, 6, -40), new(88, 0, -40), new(80, -6, -40), };
    Line lines;
    VertexArray va;
    VertexBuffer<Vector4> points;
    static readonly ContextConfiguration config = new() { ColorBits = 32, DepthBits = 24, DoubleBuffer = true, Profile = ProfileMask.Core, Flags = ContextFlag.Debug | ContextFlag.ForwardCompatible };
    public Experiment () : base(config, WindowStyle.OverlappedWindow, WindowStyleEx.None) { }
    protected override void OnLoad () {
        var pts = new Vector4[C3_lines.Length * 2];
        var vertices = Array.ConvertAll(C3_vertices, v => new Vector4(v/ 128,1) );
        for (var i = 0; i < C3_lines.Length; ++i) {
            var (j, k) = C3_lines[i];
            pts[2 * i] = vertices[j];
            pts[2 * i + 1] = vertices[k];
        }
        BindVertexArray(va = new());
        UseProgram(lines = new());
        lines.Model(Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, -Maths.fPi/2));
        lines.View(Matrix4x4.CreateTranslation(0, 0, -2));
        va.Assign(points = new(pts), lines.VertexPosition);
        Disposables.Add(va);
        Disposables.Add(lines);
        Disposables.Add(points);
    }
    protected override void OnKeyDown (in KeyArgs args) {
        switch (args.Key) {
            case Key.Escape:
                User32.PostQuitMessage(0);
                return;
        }
        base.OnKeyDown(args);
    }
    protected override void Render (double dt_seconds) {
        var size = ClientSize;
        Viewport(new(), size);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        lines.Projection(Matrix4x4.CreatePerspectiveFieldOfView(Maths.fPi / 2, (float)size.X / size.Y, 1f, 100f));
        Foo_DrawArrays(Primitive.Lines, 0, 2 * C3_lines.Length);
    }
}
