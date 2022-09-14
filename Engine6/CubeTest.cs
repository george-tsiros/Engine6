namespace Engine6;
using static Common.Maths;
using System;
using Win32;
using Gl;
using Shaders;
using System.Numerics;
using static Gl.Opengl;
using System.Diagnostics;

class CubeTest:GlWindow {
    const float MouseSensitivity = .0005f;
    const float KeyboardSensitivity = .002f;


    public CubeTest () : base() {
        Load += OnLoad;
        Input += OnInput;
        KeyUp += OnKeyUp;
    }

    private ICamera camera = new QCamera(new(0, 0, 10));
    private Axes axes;
    private VertexArray vertexArray;

    static readonly Vector4[] FaceColors = {
        new(0, 0, 1, 1),
        new(0, 0, .5f, 1),
        new(0, .5f, 0, 1),
        new(0, 1, 0, 1),
        new(1, 0, 0, 1),
        new(.5f, 0, 0, 1),
    };

    void OnKeyUp (object sender, KeyEventArgs args) {
        switch (args.Key) {
            case Key.Tab:
                var x = GetSwapInterval();
                var y = x == 1 ? -1 : x + 1;
                Debug.WriteLine($"swap {y}");
                SetSwapInterval(y);
                index = 0;
                return;
            case Key.Escape:
                User32.PostQuitMessage(0);
                return;
        }
    }

    void OnLoad (object sender, EventArgs _) {
        axes = new();
        UseProgram(axes);
        axes.Model(Matrix4x4.Identity);
        vertexArray = new();
        var m = Model.Cube(1f);
        var vertices = new Vector4[m.Faces.Count * 3];
        var vi = 0;
        foreach (var (i, j, k) in m.Faces) {
            vertices[vi++] = new(m.Vertices[i], 1);
            vertices[vi++] = new(m.Vertices[j], 1);
            vertices[vi++] = new(m.Vertices[k], 1);
        }
        var colors = new Vector4[vertices.Length];
        for (var (i, o) = (0, 0); i < FaceColors.Length; ++i) {
            var c = FaceColors[i];
            for (var j = 0; j < 6; ++j)
                colors[o++] = c;
        }
        vertexArray.Assign(new VertexBuffer<Vector4>(vertices), axes.VertexPosition);
        vertexArray.Assign(new VertexBuffer<Vector4>(colors), axes.Color);
        Enable(Capability.DepthTest);
        DepthFunc(DepthFunction.LessEqual);
        Enable(Capability.CullFace);
        ClearColor(0, 0, 0, 1);
        Disposables.Add(vertexArray);
        Disposables.Add(axes);
    }

    int Axis (Key plus, Key minus) {
        var x = IsKeyDown(plus) ? 1 : 0;
        var y = IsKeyDown(minus) ? -1 : 0;
        return x + y;
    }

    void OnInput (object sender, InputEventArgs args) {
        var (dx, dy) = (args.Dx, args.Dy);
        camera.Rotate(-MouseSensitivity * dy, 0, -MouseSensitivity * dx);
    }

    void Update () {
        var yaw = KeyboardSensitivity * Axis(Key.F, Key.S);
        camera.Rotate(0, yaw, 0);
    }

    protected override void Render () {
        Update();
        var size = ClientSize;
        Viewport(new(), size);
        Clear(BufferBit.ColorDepth);
        BindVertexArray(vertexArray);
        axes.Projection(Matrix4x4.CreatePerspectiveFieldOfView(fPi / 3, (float)size.X / size.Y, .1f, 100f));
        axes.View(camera.LookAtMatrix);
        DrawArrays(Primitive.Triangles, 0, 36);
    }
}
