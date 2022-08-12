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
    readonly Model model;
    public MovementTest (Vector2i size, Model m = null, Vector2i? position = null) : base(size, position) {
        const string TeapotFilepath = @"data\teapot.obj";
        model = m ?? new(TeapotFilepath, true);
        Load += Load_self;
        MouseMove += MouseMove_self;
        CursorGrabbed = true;
        CursorVisible = false;
    }
    void MouseMove_self (object sender, Vector2i e) {
        camera.Rotate(.001f * (Vector2)e);
    }

    VertexArray vao;
    VertexBuffer<Vector4> vertexBuffer;
    VertexBuffer<Vector3> normalBuffer;
    Vector4 lightDirection = -new Vector4(Vector3.Normalize(Vector3.One),0);
    Camera camera = new(new(0, 1.8f, 10));
    void Load_self (object sender, EventArgs args) {
        State.Program = DirectionalFlat.Id;
        var faceCount = model.Faces.Count;
        var vertexCount = faceCount * 3;
        var normals = new Vector3[vertexCount];
        for (var i = 0; i < faceCount; ++i) {
            var (a, b, c) = model.Faces[i];
            var (v0, v1, v2) = (model.Vertices[a], model.Vertices[b], model.Vertices[c]);
            normals[3 * i + 2] = normals[3 * i + 1] = normals[3 * i] = (Vector3)Vector3d.Normalize(Vector3d.Cross(v1 - v0, v2 - v0));
        }

        var vertices = new Vector4[vertexCount];
        for (var (i, j) = (0, 0); j < vertexCount; ++i, ++j) {
            var (a, b, c) = model.Faces[i];
            var (v0, v1, v2) = (model.Vertices[a], model.Vertices[b], model.Vertices[c]);
            vertices[j] = new((Vector3)v0, 1);
            vertices[++j] = new((Vector3)v1, 1);
            vertices[++j] = new((Vector3)v2, 1);
        }

        vertexBuffer = new(vertices);
        normalBuffer = new(normals);
        vao = new();
        vao.Assign(vertexBuffer, DirectionalFlat.VertexPosition);
        vao.Assign(normalBuffer, DirectionalFlat.FaceNormal);
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
        camera.Walk(velocity* dt * Vector3.Normalize(new(dx,0, dz)));
    }
    protected override void Render () {
        Viewport(0, 0, Width, Height);
        Clear(BufferBit.ColorDepth);
        var dt = Dt;
        if (0f < dt)
            Move(Dt);
        previousSync = LastSync;
        State.Program = DirectionalFlat.Id;
        State.VertexArray = vao;
        State.DepthTest = true;
        State.DepthFunc = DepthFunction.LessEqual;
        State.CullFace = true;
        DirectionalFlat.LightDirection(lightDirection);
        DirectionalFlat.Model(Matrix4x4.CreateTranslation(0, (float)model.Min.Y, 0));
        DirectionalFlat.View(camera.LookAtMatrix);
        DirectionalFlat.Projection(Matrix4x4.CreatePerspectiveFieldOfView(fPi / 4, (float)Width / Height, .1f, 100f));
        DrawArrays(Primitive.Triangles, 0, 3 * model.Faces.Count);
    }
}
