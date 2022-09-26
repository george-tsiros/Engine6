namespace Engine6;
using Win32;
using Common;
using Gl;
using static Gl.GlContext;
using System.Numerics;
using Shaders;
using System;
using System.Diagnostics;

public class Experiment:GlWindow {

    private static readonly (int, int)[] C3_lines = { (0, 1), (0, 4), (1, 3), (3, 8), (4, 7), (6, 7), (6, 9), (5, 9), (5, 8), (2, 5), (2, 6), (3, 5), (4, 6), (1, 2), (0, 2), (8, 10), (10, 11), (7, 11), (1, 10), (0, 11), (1, 5), (0, 6), (20, 21), (12, 13), (18, 19), (14, 15), (16, 17), (15, 16), (14, 17), (13, 18), (12, 19), (2, 9), (22, 24), (23, 24), (22, 23), (25, 26), (26, 27), (25, 27), };
    private static readonly Vector3[] C3_vertices = { new(32, 0, -76), new(-32, 0, -76), new(0, 26, -24), new(-120, -3, 8), new(120, -3, 8), new(-88, 16, 40), new(88, 16, 40), new(128, -8, 40), new(-128, -8, 40), new(0, 26, 40), new(-32, -24, 40), new(32, -24, 40), new(-36, 8, 40), new(-8, 12, 40), new(8, 12, 40), new(36, 8, 40), new(36, -12, 40), new(8, -16, 40), new(-8, -16, 40), new(-36, -12, 40), new(0, 0, -76), new(0, 0, -90), new(-80, -6, 40), new(-80, 6, 40), new(-88, 0, 40), new(80, 6, 40), new(88, 0, 40), new(80, -6, 40), };
    private bool guiActive = false;
    private Vector2i cursorPosition;

    private Line lines;
    private VertexArray va;
    private BufferObject<Vector4> points;
    private Vector3 modelPosition = new();
    private Quaternion modelOrientation = Quaternion.Identity;
    private Vector3 cameraLocation = 2 * Vector3.UnitZ;

    private Tex tex;
    private VertexArray quadArray;
    private BufferObject<Vector2> quadBuffer;
    private Sampler2D guiSampler;
    private Raster guiRaster;

    protected override void OnLoad () {
        SetSwapInterval(0);
        User32.MoveWindow(this, 0, 0, 800, 600, false);
        Debug.Assert(new Vector2i(800, 600) == ClientSize);
        var pts = new Vector4[C3_lines.Length * 2];
        var vertices = Array.ConvertAll(C3_vertices, v => new Vector4(v / 128, 1));
        for (var i = 0; i < C3_lines.Length; ++i) {
            var (j, k) = C3_lines[i];
            pts[2 * i] = vertices[j];
            pts[2 * i + 1] = vertices[k];
        }
        lines = new();
        BindVertexArray(va = new());
        va.Assign(points = new(pts), lines.VertexPosition);
        tex = new();
        BindVertexArray(quadArray = new());
        var quad = new Vector2[] { new(-1, -1), new(1, -1), new(1, 1), new(-1, -1), new(1, 1), new(-1, 1) };
        quadArray.Assign(quadBuffer = new(quad), tex.VertexPosition);
        guiSampler = new(ClientSize, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest, Wrap = Wrap.ClampToEdge };
        guiSampler.BindTo(0);
        tex.Tex0(0);
        guiRaster = new(guiSampler.Size, 4, 1);
        guiRaster.ClearU32(Color.Transparent);
        guiRaster.FillRectU32(new(new(), new(100, 100)), ~0u);
        Disposables.Add(va);
        Disposables.Add(quadArray);
        Disposables.Add(quadBuffer);
        Disposables.Add(lines);
        Disposables.Add(tex);
        Disposables.Add(points);
        Disposables.Add(guiSampler);
        Disposables.Add(guiRaster);
    }
    protected override void OnKeyDown (Key key, bool repeat) {
        switch (key) {
            case Key.Escape:
                User32.PostQuitMessage(0);
                return;
        }
        base.OnKeyDown(key, repeat);
    }

    protected override void OnKeyUp (Key key) {
        switch (key) {
            case Key.Tab:
                guiActive = !guiActive;
                return;
        }
        base.OnKeyUp(key);
    }

    private int Axis (Key positive, Key negative) {
        var d = IsKeyDown(positive) ? 1 : 0;
        return IsKeyDown(negative) ? d - 1 : d;
    }

    protected override void OnInput (int dx, int dy) {
        if (guiActive)
            return;
    }

    private void Update (double dt) {
        // if less time than 10 us has passed, it is assumed that we've stopped
        if (dt < 10e-6)
            return;
        const float Velocity = 1; // /s, implied length unit is whatever opengl considers it to be
        const float AngularVelocity = Maths.fPi / 2;
        var timeScale = (float)(dt * dt / TframeSeconds); //
        Vector3 cameraMovement = new(Axis(Key.C, Key.Z), Axis(Key.Q, Key.A), Axis(Key.X, Key.D));
        cameraLocation += timeScale * Velocity * cameraMovement;
        var yaw = Axis(Key.Left, Key.Right) * timeScale * AngularVelocity;
        var pitch = Axis(Key.Up, Key.Down) * timeScale * AngularVelocity;
        modelOrientation = Quaternion.Concatenate(modelOrientation, Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0));
        GetCoordinateSystem(modelOrientation, out var xLocal, out var yLocal, out var zLocal);
        modelPosition += timeScale * xLocal * Axis(Key.Insert, Key.Delete);
        modelPosition += timeScale * yLocal * Axis(Key.Home, Key.End);
        modelPosition += timeScale * zLocal * Axis(Key.PageUp, Key.PageDown);
    }

    protected override void Render (double dt) {
        if (!guiActive)
            Update(dt);
        var size = ClientSize;
        Viewport(new(), size);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        Enable(Capability.DepthTest);
        BindVertexArray(va);
        UseProgram(lines);
        lines.Color(Vector4.One);
        lines.Model(Matrix4x4.CreateFromQuaternion(modelOrientation) * Matrix4x4.CreateTranslation(modelPosition));
        lines.View(Matrix4x4.CreateTranslation(-cameraLocation));
        lines.Projection(Matrix4x4.CreatePerspectiveFieldOfView(Maths.fPi / 2, (float)size.X / size.Y, 1f, 100f));
        DrawArrays(Primitive.Lines, 0, 2 * C3_lines.Length);
        if (guiActive) {
            Viewport(new(), size);
            BindVertexArray(quadArray);
            UseProgram(tex);
            guiSampler.Upload(guiRaster);
            Disable(Capability.DepthTest);
            DrawArrays(Primitive.Triangles, 0, 6);
        }
    }

    static void GetCoordinateSystem (Quaternion q, out Vector3 ux, out Vector3 uy, out Vector3 uz) {
        ux = Vector3.Transform(Vector3.UnitX, q);
        uy = Vector3.Transform(Vector3.UnitY, q);
        uz = Vector3.Transform(Vector3.UnitZ, q);
    }
}
