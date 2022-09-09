namespace Engine6;

using System.Numerics;
using Shaders;
using Gl;
using static Gl.Opengl;
using static Common.Maths;
using Common;
using System.Diagnostics;
using Win32;

class MovementTest:GlWindowArb {
    static readonly Vector4[] QuadVertices = {
        new(-1f, -1f, 0, 1),
        new(+1f, -1f, 0, 1),
        new(+1f, +1f, 0, 1),
        new(-1f, -1f, 0, 1),
        new(+1f, +1f, 0, 1),
        new(-1f, +1f, 0, 1),
    };

    private Vector4 lightDirection = new(0, -1, 0, 0);
    private ICamera camera;// = new Camera(new(0, 0, EarthRadius + 100e3f));
    private VertexArray renderingVertexArray;
    private VertexArray presentationVertexArray;
    private Framebuffer renderingFramebuffer;
    private Sampler2D renderingSurface;
    private int vertexCount = 0;
    private DirectionalFlat directionalFlat;
    private PassThrough passThrough;
    private long previousSync;

    private float Dt => 0 < previousSync ? (float)(LastSync - previousSync) / Stopwatch.Frequency : 0;

    protected override void OnInput (int dx, int dy) {
        camera.Rotate(.01f * dy, .01f * dx, 0);
    }
    //protected override void OnMouseMove (in Vector2i e) {
    //    if (Buttons.HasFlag(MouseButton.Right)) {
    //        if (0 <= lastCursorPosition.X && 0 <= lastCursorPosition.Y) {
    //            var delta = lastCursorPosition - e;
    //            camera.Rotate(-.001f * delta.Y, -.001f * delta.X, 0);
    //        }
    //        lastCursorPosition = e;
    //        return;
    //    }
    //}

    protected override void OnIdle () =>
        Invalidate();

    const float EarthRadius = 6.3e6f;

    protected override void OnLoad () {
        var size = ClientSize = new(1280, 720);
        renderingFramebuffer = new();
        renderingFramebuffer.Attach(new Renderbuffer(size, RenderbufferFormat.Depth24Stencil8), FramebufferAttachment.DepthStencil);
        renderingSurface = new Sampler2D(size, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest };
        renderingFramebuffer.Attach(renderingSurface, FramebufferAttachment.Color0);
        NamedFramebufferDrawBuffer(renderingFramebuffer, DrawBuffer.Color0);
        directionalFlat = new();
        Debug.Assert(0 < directionalFlat);
        UseProgram(directionalFlat);
        renderingVertexArray = new();
        var model = new Model("data/teapot.obj", true);// Model.Sphere(200, 100, EarthRadius);
        camera = new Camera(new(0, 0, 2 * model.Max.Y));
        vertexCount = 3 * model.Faces.Count;
        var vertices = new Vector4[vertexCount];
        var vi = 0;
        foreach (var (i, j, k) in model.Faces) {
            vertices[vi++] = new(model.Vertices[i], 1);
            vertices[vi++] = new(model.Vertices[j], 1);
            vertices[vi++] = new(model.Vertices[k], 1);
        }
        renderingVertexArray.Assign(new VertexBuffer<Vector4>(vertices), directionalFlat.VertexPosition);

        var normals = new Vector4[vertexCount];
        for (var i = 0; i < normals.Length; ++i)
            normals[i] = Vector4.UnitY;

        renderingVertexArray.Assign(new VertexBuffer<Vector4>(normals), directionalFlat.FaceNormal);

        passThrough = new();
        UseProgram(passThrough);
        presentationVertexArray = new();

        presentationVertexArray.Assign(new VertexBuffer<Vector4>(QuadVertices), passThrough.VertexPosition);

        renderingSurface.BindTo(1);
        passThrough.Tex(1);
    }

    protected override void OnKeyDown (Key k) {
        switch (k) {
            case Key.Tab:
                goFast = !goFast;
                return;
        }
        base.OnKeyDown(k);
    }

    bool goFast = false;
    void Move (float dt) {
        var dx = IsKeyDown(Key.C) ? 1 : 0;
        if (IsKeyDown(Key.Z))
            dx -= 1;
        var dy = IsKeyDown(Key.ShiftKey) ? 1 : 0;
        if (IsKeyDown(Key.ControlKey))
            dy -= 1;
        var dz = IsKeyDown(Key.X) ? 1 : 0;
        if (IsKeyDown(Key.D))
            dz -= 1;
        if (0 == dx && 0 == dy && 0 == dz)
            return;
        var velocity = goFast ? 10f : 1f;
        var (x, y, z) = velocity * dt * Vector3.Normalize(new(dx, dy, dz));
        camera.Walk(x, y, z);
    }

    protected override void Render () {
        var dt = Dt;
        if (0f < dt)
            Move(Dt);
        UseProgram(directionalFlat);
        BindFramebuffer(renderingFramebuffer);
        BindVertexArray(renderingVertexArray);
        var size = ClientSize;
        Viewport(new(), size);
        ClearColor(0.2f, 0.2f, 0.2f, 1);
        Clear(BufferBit.ColorDepth);
        DepthFunc(DepthFunction.LessEqual);
        Enable(Capability.DepthTest);
        directionalFlat.LightDirection(lightDirection);
        directionalFlat.View(camera.LookAtMatrix);
        directionalFlat.Projection(Matrix4x4.CreatePerspectiveFieldOfView(fPi / 3, (float)size.X / size.Y, .1f, 100f));
        directionalFlat.Model(Matrix4x4.Identity);
        DrawArrays(Primitive.Triangles, 0, vertexCount);
        UseProgram(passThrough);
        BindDefaultFramebuffer();
        BindVertexArray(presentationVertexArray);
        Viewport(new(), size);
        //ClearColor(0, 0, 0, 1);
        //Clear(BufferBit.ColorDepth);
        Disable(Capability.DepthTest);
        DrawArrays(Primitive.Triangles, 0, 6);
        previousSync = LastSync;
    }
}
