namespace Engine6;
using Common;
using Gl;
using static Gl.GlContext;
using System.Numerics;
using Shaders;
using System;
using System.Diagnostics;
using System.Security.Principal;
using Win32;

public class MeasureBuffering:GlWindow {
    const int triangleCount = 100 * 1000;
    const int vertexCount = 3 * triangleCount;
    private Vector4[] vectors = new Vector4[vertexCount + 256];
    public MeasureBuffering () {
        Random r = new();
        for (var i = 0; i < vectors.Length; ++i)
            vectors[i] = new(r.NextFloat() * 20, r.NextFloat() * 20, r.NextFloat() * 20, 1);
        ClientSize = new(1920, 1080);
        Reusables.Add(va = new());
        Reusables.Add(flatColor = new());
        Reusables.Add(vertices = new(vertexCount));
        vertices.BufferData(vectors, vertexCount, 0, 0);
        va.Assign(vertices, flatColor.VertexPosition);
        UseProgram(flatColor);
        flatColor.Color(Vector4.One);
        flatColor.Model(Matrix4x4.CreateTranslation(-10, -10, -50));
        flatColor.View(Matrix4x4.Identity);
    }

    private FlatColor flatColor;
    private VertexArray va;
    private BufferObject<Vector4> vertices;
    private long someNumber;
    protected override void OnLoad () {
        base.OnLoad();
    }

    protected override void OnIdle () {
        base.OnIdle();
        var x = FramesRendered & 0xf;
        if (0 == x) {
            var num = FramesRendered >> 4;
            if (num != someNumber) {
                someNumber = num;
                var offset = (int)(num & 0xff);
                var t0 = Stopwatch.GetTimestamp();
                vertices.BufferData(vectors, vertexCount, offset, 0);
                var t1 = Stopwatch.GetTimestamp();
                var size = vertexCount * 4 * sizeof(float);
                var seconds = TimeSpan.FromTicks(t1 - t0).TotalSeconds;
                //User32.SetWindowText(this, "?");
                Debug.WriteLine($"{size} B, {seconds} s, {size/seconds} B/s");
            }
        }
    }

    protected override void Render () {
        var size = ClientSize;
        Viewport(in Vector2i.Zero, in size);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        BindVertexArray(va);
        UseProgram(flatColor);
        flatColor.Projection(Matrix4x4.CreatePerspectiveFieldOfView(float.Pi / 4, (float)size.X / size.Y, 10, 1000));
        DrawArrays(PrimitiveType.TRIANGLES, 0, vertexCount);
    }
}

