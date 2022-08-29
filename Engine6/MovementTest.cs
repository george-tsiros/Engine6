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
    Vector2i lastCursorPosition = new(-1, -1);
    protected override void OnButtonUp (MouseButton released) {
        if (released.HasFlag(MouseButton.Right))
            lastCursorPosition = new(-1, -1);
        else
            base.OnButtonUp(released);
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
        base.OnMouseMove(e);
    }

    Vector4 lightDirection = new(0, -1, 0, 0);
    Camera camera = new(new(0, 20f, 5));
    VertexArray renderingVertexArray, presentationVertexArray;
    Framebuffer renderingFramebuffer;
    int vertexCount = 0;
    DirectionalFlat directionalFlat;
    PassThrough passThrough;

    protected override void OnLoad () {
        renderingFramebuffer = new();
        var size = Rect.Size;
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

    long previousSync;
    float Dt => 0 < previousSync ? (float)(LastSync - previousSync) / Stopwatch.Frequency : 0;
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
        State.FramebufferBinding = renderingFramebuffer;
        State.VertexArrayBinding = renderingVertexArray;
        var (w, h) = Rect.Size;
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
        State.FramebufferBinding = 0;
        State.VertexArrayBinding = presentationVertexArray;
        Viewport(0, 0, w, h);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        Disable(Capability.DepthTest);
        DrawArrays(Primitive.Triangles, 0, 6);
        previousSync = LastSync;
    }
}
