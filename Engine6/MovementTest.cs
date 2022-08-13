namespace Engine;

using System;
using System.Numerics;
using Shaders;
using Gl;
using static Gl.Opengl;
using static Linear.Maths;
using Linear;
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

    public MovementTest (Vector2i size) : base(size) {
        Load += Load_self;
        MouseMove += MouseMove_self;
        CursorGrabbed = true;
        CursorVisible = false;
    }
    void MouseMove_self (object sender, Vector2i e) {
        camera.Rotate(.001f * (Vector2)e);
    }

    Vector4 lightDirection = new(0, -1, 0, 0);
    Camera camera = new(new(0, 1.8f, 5));
    VertexArray renderingVertexArray, presentationVertexArray;
    Framebuffer renderingFramebuffer;

    void Load_self (object sender, EventArgs args) {
        renderingFramebuffer = new();
        renderingFramebuffer.Attach(new Renderbuffer(Size, RenderbufferFormat.Depth24Stencil8), FramebufferAttachment.DepthStencil);
        var renderingSurface = new Sampler2D(Size, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest };
        renderingFramebuffer.Attach(renderingSurface, FramebufferAttachment.Color0);
        NamedFramebufferDrawBuffer(renderingFramebuffer, DrawBuffer.Color0);
        State.Program = DirectionalFlat.Id;
        renderingVertexArray = new();
        var vertices = new VertexBuffer<Vector4>(new Vector4[] { 
            new(-1, 0, +1, +1), 
            new(+1, 0, +1, +1), 
            new(+1, 0, -1, +1), 
            new(-1, 0, +1, +1), 
            new(+1, 0, -1, +1), 
            new(-1, 0, -1, +1), 
        });
        renderingVertexArray.Assign(vertices, DirectionalFlat.VertexPosition);
        var normals = new VertexBuffer<Vector4>(new Vector4[] {
            new(0, 1, 0, 0),
            new(0, 1, 0, 0),
            new(0, 1, 0, 0),
            new(0, 1, 0, 0),
            new(0, 1, 0, 0),
            new(0, 1, 0, 0),
        });
        renderingVertexArray.Assign(normals, DirectionalFlat.FaceNormal);
        State.Program = PassThrough.Id;
        presentationVertexArray = new();
        presentationVertexArray.Assign(new VertexBuffer<Vector4>(QuadVertices), PassThrough.VertexPosition);
        renderingSurface.BindTo(1);
        PassThrough.Tex(1);

    }

    long previousSync;
    float Dt => 0 < previousSync ? (float)(LastSync - previousSync) / Stopwatch.Frequency : 0;
    void Move (float dt) {
        var dx = IsKeyDown(Keys.C) ? 1 : 0;
        if (IsKeyDown(Keys.Z))
            dx -= 1;
        //var dy = IsKeyDown(Keys.ShiftKey) ? 1 : 0;
        //if (IsKeyDown(Keys.ControlKey))
        //    dy -= 1;
        var dz = IsKeyDown(Keys.X) ? 1 : 0;
        if (IsKeyDown(Keys.D))
            dz -= 1;
        if (0 == dx && 0 == dz)// && 0 == dz)
            return;
        var velocity = IsKeyDown(Keys.ShiftKey) ? 8f : 5f;
        camera.Walk(velocity * dt * Vector3.Normalize(new(dx, 0, dz)));
    }
    protected override void Render () {
        var dt = Dt;
        if (0f < dt)
            Move(Dt);
        State.Program = DirectionalFlat.Id;
        State.Framebuffer = renderingFramebuffer;
        State.VertexArray = renderingVertexArray;
        Viewport(0, 0, Width, Height);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        DirectionalFlat.LightDirection(lightDirection);
        DirectionalFlat.Model(Matrix4x4.Identity);
        DirectionalFlat.View(camera.LookAtMatrix);
        DirectionalFlat.Projection(Matrix4x4.CreatePerspectiveFieldOfView(fPi / 3, (float)Width / Height, .1f, 100));
        DrawArrays(Primitive.Triangles, 0, 6);
        State.Program = PassThrough.Id;
        State.Framebuffer = 0;
        State.VertexArray = presentationVertexArray;
        Viewport(0, 0, Width, Height);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        DrawArrays(Primitive.Triangles, 0, 6);
        previousSync = LastSync;
    }
}
