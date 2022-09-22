namespace Engine6;

using Win32;
using Gl;
using Shaders;
using System.Numerics;
using static Gl.GlContext;
using static Common.Maths;
using Common;

public class CubeTest:GlWindow {

    private const float MouseSensitivity = .0005f;
    private const float KeyboardRotationSensitivity = .01f;
    private const float KeyboardTranslationSensitivity = .1f;

    public CubeTest (ContextConfiguration? c = null) : base(c, WindowStyle.OverlappedWindow, WindowStyleEx.None) { }

    private ICamera camera = new Camera(new(0, 5, 0));
    private DirectionalFlat program;
    private VertexArray vertexArray;
    private VertexBuffer<Vector4> vertexBuffer, normalBuffer;
    private int drawCalls;

    protected override void OnKeyDown (in KeyArgs args) {
        switch (args.Key) {
            case Key.Escape:
                User32.PostQuitMessage(0);
                return;
        }
    }

    protected override void OnLoad () {
        ClientSize = new(1280, 720);
        SetSwapInterval(1);
        var scene = new Scene()
            .Add(Model.Plane(new(100, 100), new(10, 10)), Vector3.Zero);
        var cube = Model.Cube(.5f);
        for (var z = -100; z < 100; z += 20) {
            for (var x = -100; x < 100; x += 20) {
                var v = new Vector3(x, 10, z);
                _ = scene.Add(cube, in v);
            }
        }
        scene.Complete();
        drawCalls = scene.Vertices.Length;
        BindVertexArray(vertexArray = new());
        UseProgram(program = new());
        vertexArray.Assign(vertexBuffer = new(scene.Vertices), program.VertexPosition);
        vertexArray.Assign(normalBuffer = new(scene.Normals), program.VertexNormal);
        Enable(Capability.DepthTest);
        DepthFunc(DepthFunction.LessEqual);
        Enable(Capability.CullFace);
        ClearColor(0, 0, 0, 1);
        Disposables.Add(vertexArray);
        Disposables.Add(vertexBuffer);
        Disposables.Add(normalBuffer);
        Disposables.Add(program);
    }

    private int Axis (Key plus, Key minus) {
        var x = IsKeyDown(plus) ? 1 : 0;
        var y = IsKeyDown(minus) ? -1 : 0;
        return x + y;
    }
    private int mouseDx, mouseDy;
    protected override void OnInput (in InputArgs args) {
        mouseDx += args.Dx;
        mouseDy += args.Dy;
    }

    private void Update (double dt) {
        // timeSkew == 0 : time has stopped, nothing moves
        // 0 < timeSkew < 1 : framerate is too high, pretend as if less time has passed
        // timeSkew == 1 : exactly as planned
        // 1 < timeSkew : framerate is too low, pretend as if _more_ time has passed 
        var timeSkew = (float)(dt / TframeSeconds);
        var pitch = timeSkew * MouseSensitivity * -mouseDy;
        var yaw = timeSkew * KeyboardRotationSensitivity * Axis(Key.F, Key.S);
        var roll = timeSkew * MouseSensitivity * mouseDx;
        camera.Rotate(yaw, pitch, roll);
        var dx = Axis(Key.C, Key.Z);
        var dy = Axis(Key.Q, Key.A);
        var dz = Axis(Key.D, Key.X);
        if (0 != dx || 0 != dy || 0 != dz)
            camera.Walk(timeSkew * KeyboardTranslationSensitivity * dx, timeSkew * KeyboardTranslationSensitivity * dy, timeSkew * KeyboardTranslationSensitivity * dz);
        (mouseDx, mouseDy) = (0, 0);
    }
    Vector4 lightDirection = Vector4.Normalize(new(-1, -1, -1, 0));
    protected override void Render (double dt) {

        Update(dt);
        var size = ClientSize;
        Viewport(new(), size);
        Clear(BufferBit.ColorDepth);
        UseProgram(program);
        BindVertexArray(vertexArray);
        program.LightDirection(in lightDirection);
        program.Model(Matrix4x4.Identity);
        program.View(camera.GetViewMatrix());
        program.Projection(Matrix4x4.CreatePerspectiveFieldOfView(fPi / 3, (float)size.X / size.Y, 1f, 1000f));
        DrawArrays(Primitive.Triangles, 0, drawCalls);
    }
}
