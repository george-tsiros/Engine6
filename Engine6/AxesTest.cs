namespace Engine6;
using static Common.Maths;
using Gl;
using System;
using Shaders;
using System.Numerics;
using static Gl.RenderingContext;
using Win32;

class AxesTest:GlWindow {
    private static readonly Vector4[] Axes = {
        new(Vector3.Zero, 1),
        new(Vector3.UnitX, 1),
        new(Vector3.Zero, 1),
        new(Vector3.UnitY, 1),
        new(Vector3.Zero, 1),
        new(Vector3.UnitZ, 1),
    };

    private static readonly Vector4[] Colors = {
        new(1, 0, 0, 1),
        new(1, 0, 0, 1),
        new(0, 1, 0, 1),
        new(0, 1, 0, 1),
        new(0, 0, 1, 1),
        new(0, 0, 1, 1),
    };

    public AxesTest () : base() {
        KeyUp += OnKeyUp;
        Input += OnInput;
        Load += OnLoad;
    }

    void OnKeyUp (object sender, KeyEventArgs args) {
        switch (args.Key) {
            case Key.Escape:
                User32.PostQuitMessage(0);
                return;
        }
    }

    void OnInput (object sender, InputEventArgs args) {
        var (dx, dy) = (args.Dx, args.Dy);

        // x != 0, y == 0 => rotation along Z axis (roll, like elite)
        // x == 0, y != 0 => rotation along X axis (pitch)
        // so if both != 0 , the axis of rotation is a linear combination of UnitX + UnitZ
        if (0 != dx && 0 == dy) {
            q *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -.001f * dx);
        } else if (0 == dx && 0 != dy) {
            q *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, .001f * dy);
        }
    }
    private Quaternion q = Quaternion.Identity;
    private Axes axes;
    private VertexArray vertexArray;
    private VertexBuffer<Vector4> vb, cb;

    void OnLoad (object sender, EventArgs _) {
        var size = ClientSize = new(1280, 720);
        axes = new();
        UseProgram(axes);
        axes.Projection(Matrix4x4.CreatePerspectiveFieldOfView(fPi / 3, (float)size.X / size.Y, .1f, 10f));
        axes.View(Matrix4x4.CreateTranslation(0, 0, -5));
        vertexArray = new();
        vb = new VertexBuffer<Vector4>(Axes);
        vertexArray.Assign(vb, axes.VertexPosition);
        cb = new VertexBuffer<Vector4>(Colors);
        vertexArray.Assign(cb, axes.Color);
        Disable(Capability.DepthTest);
        Disposables.Add(vertexArray);
        Disposables.Add(axes);
        Disposables.Add(vb);
        Disposables.Add(cb);
    }

    protected override void Render () {
        Viewport(new(), ClientSize);
        UseProgram(axes);
        BindVertexArray(vertexArray);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.Color);
        axes.Model(Matrix4x4.CreateFromQuaternion(q));
        DrawArrays(Primitive.Lines, 0, 6);
    }
}
