namespace Engine6;
using Common;
using Gl;
using static Gl.GlContext;
using System.Numerics;
using System;
using Shaders;
using Win32;
using System.Diagnostics;

public class ExampleDrawArrays:ExampleBase {

    private Directional program;
    private VertexArray va;
    private BufferObject<Vector4> vertexBuffer;
    private BufferObject<Vector3> normalBuffer;
    private BufferObject<Vector2> uiBuffer;
    private UiTexture uiProgram;
    private Raster uiRaster;
    private Sampler2D uiSampler;
    private VertexArray uiVA;
    private Camera camera = new(new(0, 0, 10));
    private Vector2i cursor;
    private Vector2i UiSize;
    private readonly int VerticesPerFace, VerticesPerCube;

    private const int CursorCap = 1000;
    private const int Deadzone = 50;
    //private const float CameraHeight = 1e3f;
    //private const float PlanetRadius = 6.371e6f; // m
    //private const float PlanetMass = 1.989e30f; // kg
    //private const float Velocity = -3000f; // m/s
    private const int Subdivisions = 16;

    private const int VerticesPerTriangle = 3;
    private const int TrianglesPerQuad = 2;
    private const int FacesPerCube = 6;
    public static int VertexCountPerFace (int subdivisions) =>
        subdivisions * subdivisions * TrianglesPerQuad * VerticesPerTriangle;

    public static int VertexCountPerCube (int subdivisions) =>
        VertexCountPerFace(subdivisions) * FacesPerCube;

    private static readonly Vector2i[] quad = { new(0, 0), new(1, 0), new(1, 1), new(0, 0), new(1, 1), new(0, 1) };

    private static readonly Matrix4x4[] FaceRotation = {
        Matrix4x4.Identity,
        Matrix4x4.CreateRotationY(float.Pi/2),
        Matrix4x4.CreateRotationY(float.Pi),
        Matrix4x4.CreateRotationY(-float.Pi/2),
        Matrix4x4.CreateRotationX(float.Pi/2),
        Matrix4x4.CreateRotationX(-float.Pi/2),
    };

    public static void CreateCubeSphere (int subdivisions, float radius, Span<Vector4> vertices, Span<Vector3> normals) {

        const double MinRadius = 1e-6;
        var expected = VertexCountPerCube(subdivisions);

        if (radius < MinRadius)
            throw new ArgumentOutOfRangeException(nameof(radius), $"must be at least {MinRadius}, got {radius}");
        if (subdivisions < 2)
            throw new ArgumentOutOfRangeException(nameof(subdivisions), $"must be at least 2, got {subdivisions}");
        if (0 != (1 & subdivisions))
            throw new ArgumentOutOfRangeException(nameof(subdivisions), $"must be divisible by 2, {subdivisions} is not");
        if (vertices.Length != expected)
            throw new ArgumentException($"expected length of {expected}, not {vertices.Length}", nameof(vertices));
        if (normals.Length != expected)
            throw new ArgumentException($"expected length of {expected}, not {normals.Length}", nameof(normals));

        var dtheta = double.Pi / 2 / subdivisions;
        var k = 0;
        foreach (var rotator in FaceRotation)

            for (var i = 0; i < subdivisions; ++i)
                for (var j = 0; j < subdivisions; ++j)
                    foreach (var offset in quad) {
                        var u = -double.Pi / 4 + (i + offset.X) * dtheta;
                        var v = -double.Pi / 4 + (j + offset.Y) * dtheta;
                        var (su, cu) = double.SinCos(u);
                        var (sv, cv) = double.SinCos(v);
                        var x = (float)(cv * su);
                        var y = (float)(sv * cu);
                        var z = (float)(cv * cu);
                        var r = Vector3.Normalize(Vector3.Transform(new(x, y, z), rotator));
                        normals[k] = r;
                        vertices[k++] = new(radius * r, 1);
                    }
    }


    public ExampleDrawArrays () : this(new(1280, 720)) { }

    public ExampleDrawArrays (Vector2i size) {
        UiSize = new(40 * PixelFont.Width, 10 * PixelFont.Height);
        ClientSize = size;
        VerticesPerFace = VertexCountPerFace(Subdivisions);
        VerticesPerCube = VerticesPerFace * FacesPerCube;

        var vertices = new Vector4[VerticesPerCube];
        var normals = new Vector3[VerticesPerCube];

        CreateCubeSphere(Subdivisions, 2, vertices, normals);
        Reusables.Add(va = new());
        Reusables.Add(program = new());
        Reusables.Add(vertexBuffer = new(vertices));
        Reusables.Add(normalBuffer = new(normals));

        va.Assign(vertexBuffer, program.VertexPosition);
        va.Assign(normalBuffer, program.VertexNormal);

        Reusables.Add(uiVA = new());
        Reusables.Add(uiProgram = new());
        Reusables.Add(uiSampler = new(UiSize, SizedInternalFormat.RGBA8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest });
        Reusables.Add(uiRaster = new(UiSize));
        Reusables.Add(uiBuffer = new(PresentationQuad));
        uiVA.Assign(uiBuffer, uiProgram.VertexPosition);

        Enable(Capability.CULL_FACE);
    }

    protected override Key[] AxisKeys { get; } = { Key.C, Key.Z, Key.Q, Key.A };

    protected override void OnInput (int dx, int dy) {
        var x = int.Clamp(cursor.X + dx, -CursorCap, CursorCap);
        var y = int.Clamp(cursor.Y + dy, -CursorCap, CursorCap);
        cursor = new(x, y);
    }

    double throttle = 0;
    double planetAngle = 0;
    bool showStatus = false;

    protected override void OnKeyDown (Key key, bool repeat) {
        if (!repeat && Key.S == key) {
            showStatus = !showStatus;
            return;
        }
        base.OnKeyDown(key, repeat);
    }

    protected override void Render () {
        planetAngle += .5 * LastFramesInterval;
        if (double.Tau <= planetAngle)
            planetAngle -= double.Tau;
        var xActual = Functions.ApplyDeadzone(cursor.X, Deadzone) / (double)(CursorCap - Deadzone);
        var yActual = Functions.ApplyDeadzone(cursor.Y, Deadzone) / (double)(CursorCap - Deadzone);
        throttle = double.Clamp(throttle + Axis(Key.Q, Key.A), 0, 1);
        var roll = 2e-2 * xActual;
        var pitch = -1e-2 * yActual;
        camera.Rotate(pitch, Axis(Key.C, Key.Z), roll);
        camera.Move(100 * LastFramesInterval * throttle);
        var size = ClientSize;
        var aspectRatio = (float)size.X / size.Y;
        Viewport(in Vector2i.Zero, in size);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        BindVertexArray(va);
        UseProgram(program);
        Enable(Capability.DEPTH_TEST);
        Enable(Capability.CULL_FACE);
        var orientation = Matrix4d.CreateFromQuaternion(camera.Orientation);
        if (IsKeyDown(Key.R))
            orientation = Matrix4d.RotationY(double.Pi) * orientation;
        var translation = Matrix4d.CreateTranslation(-camera.Position);
        program.View((Matrix4x4)(translation * orientation));
        program.Projection(Matrix4x4.CreatePerspectiveFieldOfView(float.Pi / 4, aspectRatio, 10f, 1000f));
        program.LightDirection(-Vector4.UnitZ);
        program.Model(Matrix4x4.CreateRotationY((float)planetAngle));
        for (var i = 0; i < FacesPerCube; ++i) {
            program.Color(PrimaryColors[i]);
            DrawArrays(PrimitiveType.TRIANGLES, i * VerticesPerFace, VerticesPerFace);
        }
        uiRaster.Clear(Color.Black);
        var distanceFromOrigin = camera.Position.Magnitude();
        var heightString = ((int)distanceFromOrigin).ToString();
        uiRaster.DrawString(heightString, PixelFont, 0, 0);
        var throttleString = ((int)(100 * throttle)).ToString();
        uiRaster.DrawString(throttleString, PixelFont, 0, PixelFont.Height);
        uiRaster.DrawString(IsKeyDown(Key.R) ? "rear" : "forward", PixelFont, 0, 2 * PixelFont.Height);
        uiSampler.Upload(uiRaster);
        UseProgram(uiProgram);
        BindVertexArray(uiVA);
        uiProgram.Tex0(0);
        uiSampler.BindTo(0);
        Disable(Capability.DEPTH_TEST);
        Viewport(Vector2i.Zero, UiSize);
        if (showStatus)
            DrawArrays(PrimitiveType.TRIANGLES, 0, 6);
    }
}

