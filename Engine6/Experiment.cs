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

    protected override Key[] AxisKeys { get; } = { Key.C, Key.X, Key.Z, Key.D, Key.Q, Key.A, Key.Left, Key.Right, Key.Up, Key.Down, Key.PageUp, Key.PageDown, Key.Home, Key.End, Key.Insert, Key.Delete, };

    private BufferObject<Vector4> modelVertices;
    private BufferObject<Vector2> presentationVertices;
    private Framebuffer framebuffer;
    private Presentation presentation;
    private Line lines;
    private Renderbuffer depthbuffer;
    private Sampler2D renderTexture;
    private VertexArray va;
    private VertexArray pa;

    private readonly Vector4[] pts = new Vector4[C3_lines.Length * 2];
    private Vector3 cameraLocation = 2 * Vector3.UnitZ;
    private Vector3 modelPosition = new();
    private Quaternion modelOrientation = Quaternion.Identity;

    public Experiment () {
        var vertices = Array.ConvertAll(C3_vertices, v => new Vector4(v / 128, 1));
        for (var i = 0; i < C3_lines.Length; ++i) {
            var (j, k) = C3_lines[i];
            pts[2 * i] = vertices[j];
            pts[2 * i + 1] = vertices[k];
        }
        SetSwapInterval(0);
        Recyclables.Add(modelVertices = new(pts));
        Recyclables.Add(presentationVertices = new(PresentationQuad));
        Recyclables.Add(framebuffer = new());
        Recyclables.Add(presentation = new());
        Recyclables.Add(lines = new());
        Recyclables.Add(va = new());
        Recyclables.Add(pa = new());
        va.Assign(modelVertices, lines.VertexPosition);
        pa.Assign(presentationVertices, presentation.VertexPosition);
    }

    protected override void OnLoad () {
        base.OnLoad();

        renderTexture = new(ClientSize, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Linear };// 1627, 0.0001627
        depthbuffer = new(ClientSize, RenderbufferFormat.Depth24Stencil8);// 802, 8.02E-05
        framebuffer.Attach(renderTexture, FramebufferAttachment.Color0);// 59, 5.9E-06
        framebuffer.Attach(depthbuffer, FramebufferAttachment.DepthStencil);// 10, 1E-06
        Debug.Assert(FramebufferStatus.Complete == framebuffer.CheckStatus());

        Disposables.Add(depthbuffer);
        Disposables.Add(renderTexture);
    }

    protected override void OnKeyDown (Key key, bool repeat) {
        switch (key) {
            case Key.Escape:
                User32.PostQuitMessage(0);
                return;
        }
        base.OnKeyDown(key, repeat);
    }


    protected override void OnInput (int dx, int dy) {
    }

    private void Update (long ticks) {

        const float Velocity = 1; // /s, implied length unit is whatever opengl considers it to be
        const float AngularVelocity = (float)(Maths.dPi / 2);
        Vector3 cameraMovement = new(Axis(Key.C, Key.Z, ticks), Axis(Key.Q, Key.A, ticks), Axis(Key.X, Key.D, ticks));
        cameraLocation += Velocity * cameraMovement;
        var yaw = Axis(Key.Left, Key.Right, ticks) * AngularVelocity;
        var pitch = Axis(Key.Up, Key.Down, ticks) * AngularVelocity;
        modelOrientation = Quaternion.Concatenate(modelOrientation, Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0));
        GetCoordinateSystem(modelOrientation, out var xLocal, out var yLocal, out var zLocal);
        modelPosition += xLocal * Axis(Key.Insert, Key.Delete, ticks);
        modelPosition += yLocal * Axis(Key.Home, Key.End, ticks);
        modelPosition += zLocal * Axis(Key.PageUp, Key.PageDown, ticks);
    }

    protected override void Render (double dt) {
        // if less time than 10 us has passed, it is assumed that we've stopped
        Update(Ticks());
        var size = ClientSize;
        BindFramebuffer(framebuffer, FramebufferTarget.Draw);
        Disable(Capability.DepthTest);
        Viewport(in Vector2i.Zero, size);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        BindVertexArray(va);
        UseProgram(lines);
        lines.Color(Vector4.One);
        lines.Model(Matrix4x4.CreateFromQuaternion(modelOrientation) * Matrix4x4.CreateTranslation(modelPosition));
        lines.View(Matrix4x4.CreateTranslation(-cameraLocation));
        lines.Projection(Matrix4x4.CreatePerspectiveFieldOfView(Maths.fPi / 2, (float)size.X / size.Y, 1f, 100f));
        DrawArrays(Primitive.Lines, 0, 2 * C3_lines.Length);

        BindDefaultFramebuffer(FramebufferTarget.Draw);
        Disable(Capability.DepthTest);
        Viewport(in Vector2i.Zero, size);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        BindVertexArray(pa);
        UseProgram(presentation);
        presentation.Tex0(0);
        renderTexture.BindTo(0);
        DrawArrays(Primitive.Triangles, 0, 6);
    }

    static void GetCoordinateSystem (Quaternion q, out Vector3 ux, out Vector3 uy, out Vector3 uz) {
        ux = Vector3.Transform(Vector3.UnitX, q);
        uy = Vector3.Transform(Vector3.UnitY, q);
        uz = Vector3.Transform(Vector3.UnitZ, q);
    }
}
