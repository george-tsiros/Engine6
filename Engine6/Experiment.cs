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

    protected override Key[] AxisKeys { get; } = { Key.C, Key.X, Key.Z, Key.D, Key.Q, Key.A, Key.PageUp, Key.PageDown, Key.Home, Key.End, Key.Insert, Key.Delete, Key.Left, Key.Right, Key.Up, Key.Down };

    private FlatColorRadius flatColor;
    private VertexArray sa;
    private BufferObject<Vector4> sphereVertices;
    private Presentation presentation;
    private VertexArray pa;
    private BufferObject<Vector2> presentationVertices;
    private Framebuffer framebuffer;
    private Sampler2D skyboxSampler;
    private Raster skyboxRaster;
    private Skybox skybox;
    private VertexArray skyboxArray;
    private BufferObject<Vector4> skyboxVertices;
    private BufferObject<Vector2> skyboxTexCoords;
    private Renderbuffer depthbuffer;
    private Sampler2D renderTexture;

    private Vector3 cameraLocation = new(0, 0, 2 * TerraRadius);
    private Quaternion cameraOrientation = Quaternion.Identity;

    private static readonly Vector2i loPolySphereSubdivisions = new(10, 5);
    private static readonly Vector2i highPolySphereSubdivisions = new(50, 25);
    private static readonly int loPolySphereVertexCount = 3 * SphereTriangleCount(loPolySphereSubdivisions);
    private static readonly int highPolySphereVertexCount = 3 * SphereTriangleCount(highPolySphereSubdivisions);

    private readonly struct Body {
        public Vector3 Position { get; init; }
        public float Radius { get; init; }
        public Vector4 Color { get; init; }
        public float Mass { get; init; }
    };

    private static readonly Body Terra = new() { Position = new(), Radius = TerraRadius, Color = new(0, .6f, .7f, 1), Mass = TerraMass, };
    private static readonly Body Luna = new() { Position = new(TerraLunaDistance, 0, 0), Radius = LunaRadius, Color = new(.7f, .7f, .7f, 1), Mass = LunaMass };
    private static readonly Body[] TerraLunaSystem = { Luna, Terra };
    private const float Scale = 1.0e-6f;
    private const float TerraLunaDistance = 384.4e6f * Scale;
    private const float TerraRadius = 6.371e6f * Scale;
    private const float LunaRadius = 1.737e6f * Scale;
    private const float NearPlane = 1.0e6f * Scale;
    private const float FarPlane = 1.0e9f * Scale;

    private const float TerraMass = 5.972e24f;
    private const float LunaMass = 7.342e22f;

    public Experiment () {

        var allVertices = new Vector4[loPolySphereVertexCount + highPolySphereVertexCount];
        Sphere(loPolySphereSubdivisions, 1, allVertices.AsSpan(0, loPolySphereVertexCount));
        Sphere(highPolySphereSubdivisions, 1, allVertices.AsSpan(loPolySphereVertexCount, highPolySphereVertexCount));

        var centerCube = Matrix4x4.CreateTranslation(-.5f * Vector3.One) * Matrix4x4.CreateScale(100f);
        var cv = Cube.Vertices();
        for (var i = 0; i < cv.Length; ++i)
            cv[i] = Vector4.Transform(cv[i], centerCube);
        var cuv = Cube.UvVectors();

        var cubeIndices = Cube.Indices();
        Debug.Assert(36 == cubeIndices.Length);
        var cubeUvIndices = Cube.UvIndices();
        Debug.Assert(36 == cubeUvIndices.Length);
        var cubeVertices = new Vector4[cubeIndices.Length];
        var cubeTexCoords = new Vector2[cubeIndices.Length];
        for (var i = 0; i < cubeVertices.Length; ++i) {
            cubeVertices[i] = cv[cubeIndices[i]];
            cubeTexCoords[i] = cuv[cubeUvIndices[i]];
        }

        Reusables.Add(sphereVertices = new(allVertices));
        Reusables.Add(presentationVertices = new(PresentationQuad));
        Reusables.Add(skyboxVertices = new(cubeVertices));
        Reusables.Add(skyboxTexCoords = new(cubeTexCoords));
        Reusables.Add(framebuffer = new());
        Reusables.Add(presentation = new());
        Reusables.Add(pa = new());
        Reusables.Add(sa = new());
        Reusables.Add(skyboxArray = new());
        Reusables.Add(flatColor = new());
        Reusables.Add(skybox = new());
        Reusables.Add(skyboxRaster = Raster.FromFile("data\\skybox.bin"));
        Reusables.Add(skyboxSampler = new(skyboxRaster.Size, TextureFormat.Rgba8) { Mag = MagFilter.Linear, Min = MinFilter.Linear, Wrap = Wrap.ClampToEdge } );
        skyboxSampler.Upload(skyboxRaster);
        pa.Assign(presentationVertices, presentation.VertexPosition);
        sa.Assign(sphereVertices, flatColor.VertexPosition);
        skyboxArray.Assign(skyboxVertices, skybox.VertexPosition);
        skyboxArray.Assign(skyboxTexCoords, skybox.TexCoords);
        SetSwapInterval(1);
    }

    protected override void OnLoad () {
        base.OnLoad();
        renderTexture = new(ClientSize, TextureFormat.Rgba8) { Mag = MagFilter.Nearest, Min = MinFilter.Linear };
        depthbuffer = new(ClientSize, RenderbufferFormat.Depth24Stencil8);
        framebuffer.Attach(renderTexture, FramebufferAttachment.Color0);
        framebuffer.Attach(depthbuffer, FramebufferAttachment.DepthStencil);
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

    public static void Rotate (ref Quaternion q, float yaw, float pitch, float roll) {
        var localXaxis = Vector3.Transform(Vector3.UnitX, q);
        var qPitch = Quaternion.CreateFromAxisAngle(localXaxis, pitch);
        q = Quaternion.Concatenate(q, qPitch);
        var localYaxisAfterPitch = Vector3.Transform(Vector3.UnitY, q);
        var qYaw = Quaternion.CreateFromAxisAngle(localYaxisAfterPitch, yaw);
        q = Quaternion.Concatenate(q, qYaw);
        var localZaxisAfterYawPitch = Vector3.Transform(Vector3.UnitZ, q);
        var qRoll = Quaternion.CreateFromAxisAngle(localZaxisAfterYawPitch, roll);
        q = Quaternion.Concatenate(q, qRoll);

    }

    public static void Translate (ref Vector4 p, in Quaternion q, in Vector3 dr) {
        var dx = dr.X * Vector3.Transform(Vector3.UnitX, q);
        var dy = dr.Y * Vector3.Transform(Vector3.UnitY, q);
        var dz = dr.Z * Vector3.Transform(Vector3.UnitZ, q);
        p += new Vector4(dx + dy + dz, 0);
    }

    public static void CameraRotate (ref Quaternion q, float yaw, float pitch, float roll) {
        var qPitch = Quaternion.CreateFromAxisAngle(Vector3.UnitX, pitch);
        q = Quaternion.Concatenate(q, qPitch);
        var localYaxisAfterPitch = Vector3.Transform(Vector3.UnitY, qPitch);
        var qYaw = Quaternion.CreateFromAxisAngle(localYaxisAfterPitch, yaw);
        q = Quaternion.Concatenate(q, qYaw);
        var localZaxisAfterYawPitch = Vector3.Transform(Vector3.UnitZ, qPitch);
        var qRoll = Quaternion.CreateFromAxisAngle(localZaxisAfterYawPitch, roll);
        q = Quaternion.Concatenate(q, qRoll);

    }

    Vector2i cumulativeCursorMovement;

    protected override void OnInput (int dx, int dy) {
        cumulativeCursorMovement += new Vector2i(dx, dy);
    }

    protected override void Render () {
        var size = ClientSize;
        //(float)Axis(Key.Left, Key.Right)
        CameraRotate(ref cameraOrientation, (float)Axis(Key.Right, Key.Left), -.001f * cumulativeCursorMovement.Y, .001f * cumulativeCursorMovement.X);
        cumulativeCursorMovement = Vector2i.Zero;

        var viewRotation = Matrix4x4.CreateFromQuaternion(cameraOrientation);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(Maths.fPi / 4, (float)size.X / size.Y, NearPlane, FarPlane);
        BindFramebuffer(framebuffer, FramebufferTarget.Draw);
        Viewport(in Vector2i.Zero, size);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        
        Disable(Capability.DepthTest);
        Disable(Capability.CullFace);
        BindVertexArray(skyboxArray);
        UseProgram(skybox);
        skyboxSampler.BindTo(0);
        skybox.Tex0(0);
        skybox.Projection(projection);
        skybox.Orientation(viewRotation);
        DrawArrays(Primitive.Triangles, 0, 36);

        Enable(Capability.DepthTest);
        DepthFunc(DepthFunction.LessEqual);
        Enable(Capability.CullFace);
        BindVertexArray(sa);
        UseProgram(flatColor);
        flatColor.View(Matrix4x4.CreateTranslation(-cameraLocation) * viewRotation);
        flatColor.Projection(projection);

        foreach (var body in TerraLunaSystem) {
            var translation = Matrix4x4.CreateTranslation(body.Position);
            var model = translation;
            flatColor.Scale(body.Radius);
            flatColor.Color(body.Color);
            flatColor.Model(model);
            var cameraDistanceFromSphere = (body.Position - cameraLocation).Length();
            if (cameraDistanceFromSphere < 120f * body.Radius)
                DrawArrays(Primitive.Triangles, loPolySphereVertexCount, highPolySphereVertexCount);
            else
                DrawArrays(Primitive.Triangles, 0, loPolySphereVertexCount);
        }
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

    private static void GetCoordinateSystem (Quaternion q, out Vector3 ux, out Vector3 uy, out Vector3 uz) {
        ux = Vector3.Transform(Vector3.UnitX, q);
        uy = Vector3.Transform(Vector3.UnitY, q);
        uz = Vector3.Transform(Vector3.UnitZ, q);
    }

    public static int SphereTriangleCount (Vector2i n) =>
        2 * n.X * (n.Y - 1);

    public static void Sphere (Vector2i n, float radius, Span<Vector4> vertices) {
        var (nTheta, nPhi) = n;
        var spherePointCount = 2 + nTheta * (nPhi - 1);
        var triangleCount = 2 * nTheta * (nPhi - 1);
        var vertexCount = 3 * triangleCount;
        if (vertices.Length != vertexCount)
            throw new ArgumentException($"expected size {vertexCount} exactly, got {vertices.Length} instead", nameof(vertices));
        var dTheta = 2 * Maths.dPi / nTheta;
        var dPhi = Maths.dPi / nPhi;
        var vectors = new Vector4[spherePointCount];
        vectors[0] = new(radius * Vector3.UnitY, 1f);
        vectors[spherePointCount - 1] = new(-radius * Vector3.UnitY, 1);

        var phi = dPhi;
        for (int vi = 1, i = 1; vi < nPhi; ++vi, phi += dPhi) {
            var (sp, cp) = Maths.DoubleSinCos(phi);
            var theta = 0.0;
            for (var hi = 0; hi < nTheta; ++hi, ++i, theta += dTheta) {
                var (st, ct) = Maths.DoubleSinCos(theta);
                vectors[i] = new((float)(radius * sp * ct), (float)(radius * cp), (float)(radius * sp * st), 1);
            }
        }
        var indices = new int[triangleCount * 3];

        var faceIndex = 0;

        // top 
        for (var i = 1; i <= nTheta; ++i, ++faceIndex) {
            indices[3 * faceIndex] = 0;
            indices[3 * faceIndex + 2] = i;
            indices[3 * faceIndex + 1] = i % nTheta + 1; // i = nTheta
        }

        for (var y = 1; y < nPhi - 1; ++y)
            for (var x = 1; x <= nTheta; ++x) {
                // a--d
                // |\ |
                // | \|
                // b--c
                var a = (y - 1) * nTheta + x;
                var b = y * nTheta + x;
                var c = y * nTheta + x % nTheta + 1;
                var d = (y - 1) * nTheta + x % nTheta + 1;

                indices[3 * faceIndex] = a;
                indices[3 * faceIndex + 2] = b;
                indices[3 * faceIndex + 1] = c;
                ++faceIndex;
                indices[3 * faceIndex] = a;
                indices[3 * faceIndex + 2] = c;
                indices[3 * faceIndex + 1] = d;
                ++faceIndex;
            }

        for (var i = 1; i <= nTheta; ++i) {
            // y = 0 => 1 vertex
            // y = 1 ntheta vertices, starting from '1' = (y-1) * nTheta + 1
            // y = nphi-2 , second to last, starting from (nphi-3) * ntheta +1
            // y = nphi - 1, last row, 1 vertex (the last one)

            var a = (nPhi - 2) * nTheta + i;
            var b = (nPhi - 2) * nTheta + i % nTheta + 1;

            Debug.Assert(a < vectors.Length);
            Debug.Assert(b < vectors.Length);
            indices[3 * faceIndex] = a;
            indices[3 * faceIndex + 2] = vectors.Length - 1;
            indices[3 * faceIndex + 1] = b;
            ++faceIndex;
        }
        for (var i = 0; i < indices.Length; ++i)
            vertices[i] = vectors[indices[i]];
    }
}
