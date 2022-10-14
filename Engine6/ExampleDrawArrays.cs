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
    static readonly double[] Heights = { 0.0e0, 1.1e3, 2.0e3, 3.2e3, 4.7e3, 5.1e3, 7.1e3, };
    static readonly (double density, double temperature)[] DensitiesTemperatures = { 
        (1.2250, 288.15),
        (0.36391, 216.65),
        (0.08803, 216.65),
        (0.01322, 228.65),
        (0.00143, 270.65),
        (0.00086, 270.65),
        (0.000064, 214.65),
    };

    private static double Density (double h) {
        const double R = 8.3144598;
        const double g0 = 9.80665;
        const double M = 0.0289644;
        Debug.Assert(0 <= h);
        var b = 0;
        while (b < Heights.Length && Heights[b] < h) 
            ++b;
        if (Heights.Length == b)
            return 0;
        var (d, t) = DensitiesTemperatures[b];
        return d * Maths.DoubleExp(-g0 * M * (h - Heights[b]) / (R * t));
    }

    private static double GravAccel (double mass, double distance) {
        const double G = 6.6743e-11; // m^3 kg^-1 s^-2
        var r2 = distance * distance;
        if (r2 < 1e-6)
            throw new ArgumentOutOfRangeException(nameof(distance), "too small");
        return G * mass / r2;
    }

    private Directional program;
    private VertexArray va;
    private BufferObject<Vector4> vertexBuffer;
    private BufferObject<Vector3> normalBuffer;
    private BufferObject<Vector2> uiBuffer;
    private SingleChannelTexture uiProgram;
    private Raster uiRaster;
    private Sampler2D uiSampler;
    private VertexArray uiVA;
    private Camera camera = new(new(0, 0, PlanetRadius + CameraHeight));
    private Vector2i cursor;
    private static readonly PixelFont UiFont = new(@"data\Spleen_8x16.txt");
    private static readonly Vector2i UiSize = new(40 * UiFont.Width, 10 * UiFont.Height);
    private static readonly Ascii to = new(" => ");

    private const int CursorCap = 1000;
    private const int Deadzone = 50;
    private const float CameraHeight = 1e3f;
    private const float PlanetRadius = 6.371e6f; // m
    private const float PlanetMass = 1.989e30f; // kg
    private const float Velocity = 300f; // m/s

    private const int VerticesPerTriangle = 3;
    private const int TrianglesPerQuad = 2;
    private const int FacesPerCube = 6;
    private const int Subdivisions = 10;
    private const int VertexCountPerFace = Subdivisions * Subdivisions * TrianglesPerQuad * VerticesPerTriangle;
    private const int TotalVertexCount = VertexCountPerFace * FacesPerCube;
    private static readonly Vector2[] quad = { new(0, 0), new(1, 0), new(1, 1), new(0, 0), new(1, 1), new(0, 1) };
    private static readonly Func<Vector3, Vector3>[] FaceRotation = {
        v => v,
        v => new(1, v.Y, -v.X),
        v => new(-v.X, v.Y, -1),
        v => new(-1, v.Y, v.X),
        v => new(v.X, -1, v.Y),
        v => new(v.X, 1, -v.Y),
    };

    public ExampleDrawArrays () : this(new(1280, 720)) { }
    public ExampleDrawArrays (Vector2i size) {
        ClientSize = size;
        var faces = new Vector4[TotalVertexCount];
        var normals = new Vector3[TotalVertexCount];
        var factor = (float)Subdivisions;
        var (min, max) = (-Subdivisions / 2, Subdivisions / 2);
        Debug.Assert(2 * max == Subdivisions);
        for (var (f, i) = (0, 0); f < FacesPerCube; ++f) {
            var rotator = FaceRotation[f];
            for (var y = min; y < max; ++y)
                for (var x = min; x < max; ++x) {
                    Vector2 xy = new(x, y);
                    for (var q = 0; q < 6; ++q) {
                        var v = Vector3.Normalize(rotator(new((quad[q] + xy) / factor, 1)));
                        normals[i] = v;
                        faces[i++] = new(PlanetRadius * v, 1);
                    }
                }
        }

        Reusables.Add(va = new());
        Reusables.Add(program = new());
        Reusables.Add(vertexBuffer = new(faces));
        Reusables.Add(normalBuffer = new(normals));

        va.Assign(vertexBuffer, program.VertexPosition);
        va.Assign(normalBuffer, program.VertexNormal);
        Reusables.Add(uiVA = new());
        Reusables.Add(uiSampler = new(UiSize, TextureFormat.R8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest });
        Reusables.Add(uiRaster = new(UiSize, 1, 1));
        Reusables.Add(uiProgram = new());
        Reusables.Add(uiBuffer = new(PresentationQuad));
        uiVA.Assign(uiBuffer, uiProgram.VertexPosition);
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
        Enable(Capability.DepthTest);
        Enable(Capability.CullFace);
        program.View((Matrix4x4)camera.CreateView());
        program.Projection(Matrix4x4.CreatePerspectiveFieldOfView(Maths.fPi / 4, aspectRatio, CameraHeight / 10f, 10f * CameraHeight));
        program.LightDirection(-Vector4.UnitZ);
        program.Model(Matrix4x4.Identity);
        for (var i = 0; i < FacesPerCube; ++i) {
            program.Color(Colors[i]);
            DrawArrays(Primitive.Triangles, i * VertexCountPerFace, VertexCountPerFace);
        }
        uiRaster.ClearU8();
        var height = camera.Position.Magnitude() - PlanetRadius;
        using var heightString = ((int)height).ToAscii();
        uiRaster.DrawStringU8(heightString, UiFont, 0, 0);
        using var throttleString = ((int)(100 * throttle)).ToAscii();
        uiRaster.DrawStringU8(throttleString, UiFont, 0, UiFont.Height);
        using var densityString = Density(height).ToAscii();
        uiRaster.DrawStringU8(densityString, UiFont, 0, 2 * UiFont.Height);
        uiSampler.Upload(uiRaster);
        UseProgram(uiProgram);
        BindVertexArray(uiVA);
        uiProgram.Tex0(0);
        uiSampler.BindTo(0);
        Disable(Capability.DepthTest);
        Viewport(Vector2i.Zero, UiSize);
        DrawArrays(Primitive.Triangles, 0, 6);
    }
}

