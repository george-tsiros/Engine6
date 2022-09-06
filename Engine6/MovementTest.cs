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

    private Vector2i lastCursorPosition = new(-1, -1);
    private Vector4 lightDirection = new(0, -1, 0, 0);
    private Camera camera = new(new(0, 20f, 5));
    private VertexArray renderingVertexArray;
    private VertexArray presentationVertexArray;
    private Framebuffer renderingFramebuffer;
    private int vertexCount = 0;
    private DirectionalFlat directionalFlat;
    private PassThrough passThrough;
    private long previousSync;

    private float Dt => 0 < previousSync ? (float)(LastSync - previousSync) / Stopwatch.Frequency : 0;

    protected override void OnButtonUp (MouseButton released, PointShort p) {
        if (released.HasFlag(MouseButton.Right))
            lastCursorPosition = new(-1, -1);
    }

    protected override void OnMouseMove (in Vector2i e) {
        if (Buttons.HasFlag(MouseButton.Right)) {
            if (0 <= lastCursorPosition.X && 0 <= lastCursorPosition.Y) {
                var delta = lastCursorPosition - e;
                camera.Rotate(-.001f * (Vector2)delta);
            }
            lastCursorPosition = e;
            return;
        }
    }

    protected override void OnLoad () {
        renderingFramebuffer = new();
        var size = Size;
        renderingFramebuffer.Attach(new Renderbuffer(size, RenderbufferFormat.Depth24Stencil8), FramebufferAttachment.DepthStencil);
        var renderingSurface = new Sampler2D(size, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest };
        renderingFramebuffer.Attach(renderingSurface, FramebufferAttachment.Color0);
        NamedFramebufferDrawBuffer(renderingFramebuffer, DrawBuffer.Color0);
        directionalFlat = new();
        Debug.Assert(0 < directionalFlat);
        UseProgram(directionalFlat);
        renderingVertexArray = new();
        var plane = Model.Plane(new(200, 200), new(100, 100));
        vertexCount = 3 * plane.Faces.Count;
        var vertices = new Vector4[vertexCount];
        var vi = 0;
        foreach (var (i, j, k) in plane.Faces) {
            var (a, b, c) = (plane.Vertices[i], plane.Vertices[j], plane.Vertices[k]);
            vertices[vi++] = new((float)a.X, (float)a.Y, (float)a.Z, 1);
            vertices[vi++] = new((float)b.X, (float)b.Y, (float)b.Z, 1);
            vertices[vi++] = new((float)c.X, (float)c.Y, (float)c.Z, 1);
        }
        renderingVertexArray.Assign(new VertexBuffer<Vector4>(vertices), directionalFlat.VertexPosition);
        var normals = new Vector4[vertexCount];
        for (var i = 0; i < normals.Length; ++i)
            normals[i] = Vector4.UnitY;
        renderingVertexArray.Assign(new VertexBuffer<Vector4>(normals), directionalFlat.FaceNormal);
        passThrough = new();
        Debug.Assert(0 < passThrough);
        UseProgram(passThrough);
        presentationVertexArray = new();
        presentationVertexArray.Assign(new VertexBuffer<Vector4>(QuadVertices), passThrough.VertexPosition);
        renderingSurface.BindTo(1);
        passThrough.Tex(1);
    }

    void Move (float dt) {
        var dx = IsKeyDown(Key.C) ? 1 : 0;
        if (IsKeyDown(Key.Z))
            dx -= 1;
        //var dy = IsKeyDown(Keys.ShiftKey) ? 1 : 0;
        //if (IsKeyDown(Keys.ControlKey))
        //    dy -= 1;
        var dz = IsKeyDown(Key.X) ? 1 : 0;
        if (IsKeyDown(Key.D))
            dz -= 1;
        if (0 == dx && 0 == dz)// && 0 == dz)
            return;
        var velocity = IsKeyDown(Key.ShiftKey) ? 8f : 5f;
        camera.Walk(velocity * dt * Vector3.Normalize(new(dx, 0, dz)));
    }

    protected override void Render () {
        var dt = Dt;
        if (0f < dt)
            Move(Dt);
        UseProgram(directionalFlat);
        BindFramebuffer(renderingFramebuffer);
        BindVertexArray(renderingVertexArray);
        var (w, h) = Size;
        Viewport(0, 0, w, h);
        ClearColor(0.2f, 0.2f, 0.2f, 1);
        Clear(BufferBit.ColorDepth);
        Enable(Capability.DepthTest);
        directionalFlat.LightDirection(lightDirection);
        directionalFlat.View(camera.LookAtMatrix);
        directionalFlat.Projection(Matrix4x4.CreatePerspectiveFieldOfView(fPi / 3, (float)w / h, 1f, 1000));
        directionalFlat.Model(Matrix4x4.Identity);
        DrawArrays(Primitive.Triangles, 0, vertexCount);
        UseProgram(passThrough);
        BindDefaultFramebuffer();
        BindVertexArray(presentationVertexArray);
        Viewport(0, 0, w, h);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        Disable(Capability.DepthTest);
        DrawArrays(Primitive.Triangles, 0, 6);
        previousSync = LastSync;
    }
}
