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

    private static double Density (double h) =>
        Maths.DoubleMax(0, 1.227726 - 1.23707613953 * (1 - Maths.DoubleExp(-1.140971e-3 * h)));

    private static double GravAccel (double mass, double distance) {
        const double G = 6.6743e-11; // m^3 kg^-1 s^-2
        var r2 = distance * distance;
        if (r2 < 1e-6)
            throw new ArgumentOutOfRangeException(nameof(distance), "too small");
        return G * mass / r2;
    }

    private ShowDepth depthProgram;
    private Sampler2D depthSampler;
    private Directional program;
    private VertexArray va;
    private VertexArray depthVA;
    private BufferObject<Vector4> vertexBuffer;
    private BufferObject<Vector3> normalBuffer;
    private BufferObject<Vector2> uiBuffer;
    private SingleChannelTexture uiProgram;
    private Raster uiRaster;
    private Sampler2D uiSampler;
    private VertexArray uiVA;
    private Camera camera = new(new(0, 0, 10 * PlanetRadius));
    private Vector2i cursor;
    private static readonly PixelFont UiFont = new(@"data\Spleen_8x16.txt");
    private static readonly Vector2i UiSize = new(40 * UiFont.Width, 10 * UiFont.Height);
    private readonly int VerticesPerFace, VerticesPerCube;

    private const int CursorCap = 1000;
    private const int Deadzone = 50;
    private const float CameraHeight = 1e3f;
    private const float PlanetRadius = 6.371e6f; // m
    private const float PlanetMass = 1.989e30f; // kg
    private const float Velocity = -3000f; // m/s
    private const int Subdivisions = 10;

    private const int VerticesPerTriangle = 3;
    private const int TrianglesPerQuad = 2;
    private const int FacesPerCube = 6;
    public static int VertexCountPerFace (int subdivisions) =>
        subdivisions * subdivisions * TrianglesPerQuad * VerticesPerTriangle;

    public static int VertexCountPerCube (int subdivisions) =>
        VertexCountPerFace(subdivisions) * FacesPerCube;

    private static readonly Vector2[] quad = { new(0, 0), new(1, 0), new(1, 1), new(0, 0), new(1, 1), new(0, 1) };
    private static readonly Func<Vector3, Vector3>[] FaceRotation = {
        v => v,
        v => new(1, v.Y, -v.X),
        v => new(-v.X, v.Y, -1),
        v => new(-1, v.Y, v.X),
        v => new(v.X, -1, v.Y),
        v => new(v.X, 1, -v.Y),
    };

    public static void CreateCubeSphere (int subdivisions, float radius, Span<Vector4> vertices, Span<Vector3> normals) {
        const double MinRadius = 1e-6;
        var expected = VertexCountPerCube(subdivisions);
        if (radius < MinRadius)
            throw new ArgumentOutOfRangeException(nameof(radius), $"must be at least {MinRadius}, got {radius}");
        if (subdivisions < 2)
            throw new ArgumentOutOfRangeException(nameof(subdivisions), $"must be at least 2, got {subdivisions}");
        if (vertices.Length != expected)
            throw new ArgumentException($"expected length of {expected}, not {vertices.Length}", nameof(vertices));
        if (normals.Length != expected)
            throw new ArgumentException($"expected length of {expected}, not {normals.Length}", nameof(normals));
        var (min, max) = (-subdivisions / 2, subdivisions / 2);
        Debug.Assert(2 * max == subdivisions);
        var factor = (float)subdivisions;
        for (var (f, i) = (0, 0); f < FacesPerCube; ++f) {
            var rotator = FaceRotation[f];
            for (var y = min; y < max; ++y)
                for (var x = min; x < max; ++x) {
                    Vector2 xy = new(x, y);
                    for (var q = 0; q < 6; ++q) {
                        var v = Vector3.Normalize(rotator(new((quad[q] + xy) / factor, 1)));
                        normals[i] = v;
                        vertices[i++] = new(radius * v, 1);
                    }
                }
        }
    }


    public ExampleDrawArrays () : this(new(1280, 720)) { }

    public ExampleDrawArrays (Vector2i size) {
        ClientSize = size;
        VerticesPerFace = VertexCountPerFace(Subdivisions);
        VerticesPerCube = VerticesPerFace * FacesPerCube;
        var vertices = new Vector4[VerticesPerCube];
        var normals = new Vector3[VerticesPerCube];
        CreateCubeSphere(Subdivisions, PlanetRadius, vertices, normals);
        Reusables.Add(va = new());
        Reusables.Add(program = new());
        Reusables.Add(vertexBuffer = new(vertices));
        Reusables.Add(normalBuffer = new(normals));

        va.Assign(vertexBuffer, program.VertexPosition);
        va.Assign(normalBuffer, program.VertexNormal);

        Reusables.Add(uiVA = new());
        Reusables.Add(uiSampler = new(UiSize, SizedInternalFormat.R8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest });
        Reusables.Add(uiRaster = new(UiSize, 1, 1));
        Reusables.Add(uiProgram = new());
        Reusables.Add(uiBuffer = new(PresentationQuad));
        uiVA.Assign(uiBuffer, uiProgram.VertexPosition);

        Reusables.Add(depthVA = new());
        Reusables.Add(depthProgram = new());
        depthVA.Assign(uiBuffer, depthProgram.VertexPosition);
    }

    protected override void OnLoad () {
        base.OnLoad();
        var size = ClientSize;
        Disposables.Add(depthSampler = new(new(size.X / 2, size.Y / 2), SizedInternalFormat.DEPTH_COMPONENT24));
        depthSampler.Min = MinFilter.Nearest;
        depthSampler.Mag = MagFilter.Nearest;
    }

    protected override Key[] AxisKeys { get; } = { Key.C, Key.Z, Key.Q, Key.A };

    protected override void OnInput (int dx, int dy) {
        var x = Maths.Int32Clamp(cursor.X + dx, -CursorCap, CursorCap);
        var y = Maths.Int32Clamp(cursor.Y + dy, -CursorCap, CursorCap);
        cursor = new(x, y);
    }

    double throttle = 0;

    protected override void Render () {
        var xActual = Functions.ApplyDeadzone(cursor.X, Deadzone) / (double)(CursorCap - Deadzone);
        var yActual = Functions.ApplyDeadzone(cursor.Y, Deadzone) / (double)(CursorCap - Deadzone);
        throttle = Maths.DoubleClamp(throttle + Axis(Key.Q, Key.A), 0, 1);
        var roll = 2e-2 * xActual;
        var pitch = -1e-2 * yActual;
        camera.Rotate(pitch, Axis(Key.C, Key.Z), roll);
        camera.Move(LastFramesInterval * Velocity * throttle);
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
            orientation = Matrix4d.RotationY(Maths.dPi) * orientation;
        var translation = Matrix4d.CreateTranslation(-camera.Position);
        program.View((Matrix4x4)(translation * orientation));
        program.Projection(Matrix4x4.CreatePerspectiveFieldOfView(Maths.fPi / 4, aspectRatio, 100f, 10f * CameraHeight));
        program.LightDirection(-Vector4.UnitZ);
        program.Model(Matrix4x4.Identity);
        for (var i = 0; i < FacesPerCube; ++i) {
            program.Color(Colors[i]);
            DrawArrays(PrimitiveType.TRIANGLES, i * VerticesPerFace, VerticesPerFace);
        }
        uiRaster.ClearU8();
        var height = camera.Position.Magnitude() - PlanetRadius;
        var heightString = ((int)height).ToString();
        uiRaster.DrawStringU8(heightString, UiFont, 0, 0);
        var throttleString = ((int)(100 * throttle)).ToString();
        uiRaster.DrawStringU8(throttleString, UiFont, 0, UiFont.Height);
        var densityString = Density(height).ToString();
        uiRaster.DrawStringU8(densityString, UiFont, 0, 2 * UiFont.Height);
        uiSampler.Upload(uiRaster);
        UseProgram(uiProgram);
        BindVertexArray(uiVA);
        uiProgram.Tex0(0);
        uiSampler.BindTo(0);
        Disable(Capability.DEPTH_TEST);
        Viewport(Vector2i.Zero, UiSize);
        DrawArrays(PrimitiveType.TRIANGLES, 0, 6);
        //Viewport(new(UiSize.X, 0), UiSize);
        //depthSampler.BindTo(0);
        //ReadBuffer(ReadBufferComponent.BACK);
        //CopyTexImage2D(TextureTarget.TEXTURE_2D, 0, InternalFormat.DEPTH_COMPONENT, new(), depthSampler.Size);
        //UseProgram(depthProgram);
        //BindVertexArray(depthVA);
        //depthProgram.Depth(0);
        //DrawArrays(PrimitiveType.TRIANGLES, 0, 6);
    }
}

