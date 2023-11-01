namespace Engine6;
using Win32;
using Common;
using Gl;
using static Gl.GlContext;
using System;
using System.Numerics;
using Shaders;
using System.Diagnostics;

public class MatrixTests:GlWindow {

    private static readonly (int, int)[] C3_lines = { (0, 1), (0, 4), (1, 3), (3, 8), (4, 7), (6, 7), (6, 9), (5, 9), (5, 8), (2, 5), (2, 6), (3, 5), (4, 6), (1, 2), (0, 2), (8, 10), (10, 11), (7, 11), (1, 10), (0, 11), (1, 5), (0, 6), (20, 21), (12, 13), (18, 19), (14, 15), (16, 17), (15, 16), (14, 17), (13, 18), (12, 19), (2, 9), (22, 24), (23, 24), (22, 23), (25, 26), (26, 27), (25, 27), };
    private static readonly Vector3[] C3_vertices = { new(32, 0, -76), new(-32, 0, -76), new(0, 26, -24), new(-120, -3, 8), new(120, -3, 8), new(-88, 16, 40), new(88, 16, 40), new(128, -8, 40), new(-128, -8, 40), new(0, 26, 40), new(-32, -24, 40), new(32, -24, 40), new(-36, 8, 40), new(-8, 12, 40), new(8, 12, 40), new(36, 8, 40), new(36, -12, 40), new(8, -16, 40), new(-8, -16, 40), new(-36, -12, 40), new(0, 0, -76), new(0, 0, -90), new(-80, -6, 40), new(-80, 6, 40), new(-88, 0, 40), new(80, 6, 40), new(88, 0, 40), new(80, -6, 40), };
    private static readonly PixelFont UiFont = new(@"data\Spleen_8x16.txt");
    private static readonly Vector2i UiSize = new(40 * UiFont.Width, 10 * UiFont.Height);
    private const int CursorCap = 1000;
    private const int Deadzone = 50;
    private static readonly Key[] _AxisKeys = { Key.Z, Key.C, Key.X, Key.D };
    private const float Velocity = 1f; // m/s
    private static readonly Vector4 LightDirection = new(-Vector3.Normalize(Vector3.One), 0);

    public MatrixTests () : this(new(1280, 720)) { }

    public MatrixTests (Vector2i size) {
        ClientSize = size;
        camera = new(10 * Vector3.UnitZ);
        const int CubeFaceCount = 6;
        const int VerticesPerTriangle = 3;
        const int TrianglesPerFace = 2;
        vertexCount = VerticesPerTriangle * TrianglesPerFace * CubeFaceCount;

        Span<Vector4> modelVertices = new Vector4[vertexCount];
        Span<Vector3> modelNormals = new Vector3[vertexCount];
        Debug.Assert(Cube.Indices.Count == vertexCount);
        Debug.Assert(Cube.NormalIndices.Count == vertexCount);
        for (var i = 0; i < vertexCount; ++i) {
            modelVertices[i] = Cube.Vertices[Cube.Indices[i]];
            modelNormals[i] = Cube.Normals[Cube.NormalIndices[i]];
        }

        Reusables.Add(va = new());
        Reusables.Add(program = new());
        Reusables.Add(vertices = new(modelVertices));
        Reusables.Add(normals = new(modelNormals));
        va.Assign(vertices, program.VertexPosition);
        va.Assign(normals, program.VertexNormal);
        Reusables.Add(uiVA = new());
        Reusables.Add(uiSampler = new(UiSize, SizedInternalFormat.R8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest });
        Reusables.Add(uiRaster = new(UiSize, 1, 1));
        Reusables.Add(uiProgram = new());
        Reusables.Add(uiBuffer = new(PresentationQuad));
        uiVA.Assign(uiBuffer, uiProgram.VertexPosition);
    }
    protected override Key[] AxisKeys => _AxisKeys;
    private BufferObject<Vector3> normals;
    private BufferObject<Vector4> vertices;
    private Directional program;
    private int vertexCount;
    private BufferObject<Vector2> uiBuffer;
    private SingleChannelTexture uiProgram;
    private Raster uiRaster;
    private Sampler2D uiSampler;
    private VertexArray uiVA;
    private readonly Camera camera;
    private Pose model = new();
    private Vector2i cursor;
    private VertexArray va;

    protected override void OnInput (int dx, int dy) {
        var x = int.Clamp(cursor.X + dx, -CursorCap, CursorCap);
        var y = int.Clamp(cursor.Y + dy, -CursorCap, CursorCap);
        cursor = new(x, y);
    }

    protected override void Render () {
        var size = ClientSize;
        var xActual = Functions.ApplyDeadzone(cursor.X, Deadzone) / (double)(CursorCap - Deadzone);
        var yActual = Functions.ApplyDeadzone(cursor.Y, Deadzone) / (double)(CursorCap - Deadzone);
        var roll = 2e-2 * xActual;
        var pitch = -1e-2 * yActual;
        camera.Rotate(pitch, Axis(Key.C, Key.Z), roll);
        camera.Move(LastFramesInterval * Velocity);
        var projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(float.Pi / 4, (float)size.X / size.Y, 0.1f, 100f);

        Viewport(in Vector2i.Zero, in size);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);

        UseProgram(program);
        BindVertexArray(va);
        Enable(Capability.DEPTH_TEST);

        program.Model(model.CreateModelMatrix());
        program.View((Matrix4x4)camera.CreateView());
        program.Projection(projectionMatrix);
        program.Color(Vector4.One);
        program.LightDirection(LightDirection);
        DrawArrays(PrimitiveType.TRIANGLES, 0, vertexCount);

        uiRaster.ClearU8();
        var cx = cursor.X.ToString();
        var cy = cursor.Y.ToString();
        var xp = xActual.ToString();
        var yp = yActual.ToString();
        uiRaster.DrawStringU8(cx, UiFont, 0, 0);
        uiRaster.DrawStringU8(" => ", UiFont, UiFont.Width * cx.Length, 0);
        uiRaster.DrawStringU8(xp, UiFont, UiFont.Width * (cx.Length + 4), 0);
        uiRaster.DrawStringU8(cy, UiFont, 0, UiFont.Height);
        uiRaster.DrawStringU8(" => ", UiFont, UiFont.Width * cy.Length, UiFont.Height);
        uiRaster.DrawStringU8(yp, UiFont, UiFont.Width * (cy.Length + 4), UiFont.Height);
        uiSampler.Upload(uiRaster);
        UseProgram(uiProgram);
        BindVertexArray(uiVA);
        uiProgram.Tex0(0);
        uiSampler.BindTo(0);
        Disable(Capability.DEPTH_TEST);
        Viewport(Vector2i.Zero, UiSize);
        DrawArrays(PrimitiveType.TRIANGLES, 0, 6);
    }
}
