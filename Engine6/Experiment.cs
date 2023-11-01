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

    private BufferObject<Vector2> presentationBuffer;
    private VertexArray presentationVertexArray;
    private Presentation presentationProgram;

    private BufferObject<Vector4> sphereBuffer;
    private VertexArray sphereVertexArray;
    private FlatColor sphereProgram;

    private BufferObject<Vector4> starBuffer;
    private VertexArray starVertexArray;
    private PointStar starProgram;

    private Framebuffer framebuffer;
    private Renderbuffer depthbuffer;
    private Sampler2D renderTexture;

    private Camera camera = new(new((float)(SolTerraDistance + TerraLunaDistance + 2 * LunaRadius), 0, 5 * (float)LunaRadius));

    private static readonly Vector2i loPolySphereSubdivisions = new(10, 5);
    private static readonly Vector2i highPolySphereSubdivisions = new(50, 25);
    private static readonly int loPolySphereVertexCount = 3 * SphereTriangleCount(loPolySphereSubdivisions);
    private static readonly int highPolySphereVertexCount = 3 * SphereTriangleCount(highPolySphereSubdivisions);

    private readonly struct Body {
        public Vector3d Position { get; init; }
        public double Radius { get; init; }
        public Vector4 Color { get; init; }
        public float Mass { get; init; }
    };

    private static readonly Body Sol = new() { Position = Vector3d.Zero, Radius = SolRadius, Color = new(1, 1, .5f, 1), Mass = SolMass };
    private static readonly Body Terra = new() { Position = new(SolTerraDistance, 0, 0), Radius = TerraRadius, Color = new(0, .6f, .7f, 1), Mass = TerraMass, };
    private static readonly Body Luna = new() { Position = new(SolTerraDistance + TerraLunaDistance, 0, 0), Radius = LunaRadius, Color = new(.7f, .7f, .7f, 1), Mass = LunaMass };
    private static readonly Body[] Solar = { Sol, Terra, Luna };

    private const double SolTerraDistance = 150e9;
    private const double TerraLunaDistance = 384.4e6;
    private const double SolRadius = 695.7e6;
    private const double TerraRadius = 6.371e6;
    private const double LunaRadius = 1.737e6;

    private const float SolMass = 1.989e30f;
    private const float TerraMass = 5.972e24f;
    private const float LunaMass = 7.342e22f;
    private const float NearPlane = 1.0e3f;
    private const float FarPlane = 1000e9f;

    public Experiment () {
        ClientSize = new(1280, 720);
        var allVertices = new Vector4[loPolySphereVertexCount + highPolySphereVertexCount];
        Sphere(in loPolySphereSubdivisions, 1, allVertices.AsSpan(0, loPolySphereVertexCount));
        Sphere(in highPolySphereSubdivisions, 1, allVertices.AsSpan(loPolySphereVertexCount, highPolySphereVertexCount));

        Reusables.Add(starVertexArray = new());
        Reusables.Add(starProgram = new());
        var stars = new Vector4[1000];
        Random random = new(1);

        for (var i = 0; i < stars.Length; ++i) {

            Vector3 star = new(random.RandomSign() * random.NextFloat(), random.RandomSign() * random.NextFloat(), random.RandomSign() * random.NextFloat());
            while (star.LengthSquared() < .0001f)
                star = new(random.RandomSign() * random.NextFloat(), random.RandomSign() * random.NextFloat(), random.RandomSign() * random.NextFloat());
            stars[i] = new(10 * NearPlane * Vector3.Normalize(star), 1);
        }
        Reusables.Add(starBuffer = new(stars));
        starVertexArray.Assign(starBuffer, starProgram.VertexPosition);
        Reusables.Add(sphereBuffer = new(allVertices));
        Reusables.Add(presentationBuffer = new(PresentationQuad));
        Reusables.Add(framebuffer = new());
        Reusables.Add(presentationProgram = new());
        Reusables.Add(presentationVertexArray = new());
        Reusables.Add(sphereVertexArray = new());
        Reusables.Add(sphereProgram = new());
        presentationVertexArray.Assign(presentationBuffer, presentationProgram.VertexPosition);
        sphereVertexArray.Assign(sphereBuffer, sphereProgram.VertexPosition);
        //SetSwapInterval(-1);
    }

    protected override void OnLoad () {
        base.OnLoad();
        renderTexture = new(ClientSize, SizedInternalFormat.RGBA8) { Mag = MagFilter.Nearest, Min = MinFilter.Linear };
        depthbuffer = new(ClientSize, InternalFormat.DEPTH24_STENCIL8);
        framebuffer.Attach(renderTexture, FramebufferAttachment.COLOR_ATTACHMENT0);
        framebuffer.Attach(depthbuffer, FramebufferAttachment.DEPTH_STENCIL_ATTACHMENT);
        Debug.Assert(FramebufferStatus.FRAMEBUFFER_COMPLETE == framebuffer.CheckStatus());

        Disposables.Add(depthbuffer);
        Disposables.Add(renderTexture);
    }

    private bool outputFrame;

    protected override void OnKeyUp (Key key) {
        if (Key.F1 == key) {
            outputFrame = true;
            return;
        }
        base.OnKeyUp(key);
    }

    private Vector2i cumulativeCursorMovement;

    protected override void OnInput (int dx, int dy) {
        cumulativeCursorMovement += new Vector2i(dx, dy);
    }

    protected override void Render () {
        var size = ClientSize;
        var (minPointSize, maxPointSize) = GetPointSizeRange();
        var pointSize = size.Y / 108f;
        Debug.Assert(minPointSize <= pointSize && pointSize <= maxPointSize);
        PointSize(pointSize);
        camera.Rotate(-.001 * cumulativeCursorMovement.Y, .001 * cumulativeCursorMovement.X, Axis(Key.Right, Key.Left));
        cumulativeCursorMovement = Vector2i.Zero;

        var viewRotation = Matrix4x4.CreateFromQuaternion((Quaternion)camera.Orientation);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(float.Pi / 4, (float)size.X / size.Y, NearPlane, FarPlane);
        BindFramebuffer(framebuffer, FramebufferTarget.DRAW_FRAMEBUFFER);
        Viewport(in Vector2i.Zero, in size);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);

        Disable(Capability.DEPTH_TEST);
        Disable(Capability.CULL_FACE);
        BindVertexArray(starVertexArray);
        UseProgram(starProgram);
        starProgram.View(viewRotation);
        starProgram.Projection(projection);
        DrawArrays(PrimitiveType.POINTS, 0, 1000);

        Enable(Capability.DEPTH_TEST);
        DepthFunc(DepthFunction.LEQUAL);
        Enable(Capability.CULL_FACE);
        BindVertexArray(sphereVertexArray);
        UseProgram(sphereProgram);
        sphereProgram.View(Matrix4x4.CreateTranslation(-(Vector3)camera.Position) * viewRotation);
        sphereProgram.Projection(projection);
        foreach (var body in Solar) {
            sphereProgram.Color(body.Color);
            sphereProgram.Model(Matrix4x4.CreateScale((float)body.Radius) * Matrix4x4.CreateTranslation((Vector3)body.Position));
            DrawArrays(PrimitiveType.TRIANGLES, loPolySphereVertexCount, highPolySphereVertexCount);
        }
        BindDefaultFramebuffer(FramebufferTarget.DRAW_FRAMEBUFFER);
        Disable(Capability.DEPTH_TEST);
        Viewport(in Vector2i.Zero, in size);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        BindVertexArray(presentationVertexArray);
        UseProgram(presentationProgram);
        presentationProgram.Tex0(0);
        renderTexture.BindTo(0);
        DrawArrays(PrimitiveType.TRIANGLES, 0, 6);

        if (outputFrame) {

            outputFrame = false;
        }
    }

    //public static Quaternion Append (in Quaternion q, in Vector3 axis, float amount)
    //    => Quaternion.Concatenate(q, Quaternion.CreateFromAxisAngle(Vector3.Transform(axis, q), amount));

    //public static void RotateQuaternion (ref Quaternion q, in Vector3 pitchYawRoll) {
    //    q = Append(in q, Vector3.UnitX, pitchYawRoll.X);
    //    q = Append(in q, Vector3.UnitY, pitchYawRoll.Y);
    //    q = Append(in q, Vector3.UnitZ, pitchYawRoll.Z);
    //}

    public static int SphereTriangleCount (in Vector2i n) =>
        2 * n.X * (n.Y - 1);

    public static int SpherePointCount (in Vector2i n) =>
        2 + n.X * (n.Y - 1);

    public static void Sphere (in Vector2i n, float radius, in Span<Vector4> vertices) {
        var (nTheta, nPhi) = n;
        var spherePointCount = SpherePointCount(in n);
        var triangleCount = SphereTriangleCount(in n);
        var vertexCount = 3 * triangleCount;
        if (vertices.Length != vertexCount)
            throw new ArgumentException($"expected size {vertexCount} exactly, got {vertices.Length} instead", nameof(vertices));
        var dTheta = 2 * double.Pi / nTheta;
        var dPhi = double.Pi / nPhi;
        var vectors = new Vector4[spherePointCount];
        vectors[0] = new(radius * Vector3.UnitY, 1f);
        vectors[spherePointCount - 1] = new(-radius * Vector3.UnitY, 1);

        var phi = dPhi;
        for (int vi = 1, i = 1; vi < nPhi; ++vi, phi += dPhi) {
            var (sp, cp) = double.SinCos(phi);
            var theta = 0.0;
            for (var hi = 0; hi < nTheta; ++hi, ++i, theta += dTheta) {
                var (st, ct) = double.SinCos(theta);
                vectors[i] = new((float)(radius * sp * ct), (float)(radius * cp), (float)(radius * sp * st), 1);
            }
        }
        var indices = new int[vertexCount];

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

    public static int CylinderTriangleCount (int faceCount) =>
        3 <= faceCount ? 4 * faceCount : throw new ArgumentOutOfRangeException(nameof(faceCount));

    public static void Cylinder (int faceCount, Span<Vector3> vertices) {
        var triangleCount = CylinderTriangleCount(faceCount);
        var vertexCount = 3 * triangleCount;
        if (vertices.Length != vertexCount)
            throw new ArgumentException($"expected exactly {vertexCount} length, not {vertices.Length}", nameof(vertices));
        var topCenter = Vector3.UnitY;
        var bottomCenter = -Vector3.UnitY;
        for (var (face, i) = (0, 0); face < faceCount; ++face) {
            var theta0 = (float)face / faceCount * float.Tau;
            var theta1 = (face + 1 < faceCount ? face : 0f) / faceCount * float.Tau;
            var (x0, z0) = float.SinCos(theta0);
            var (x1, z1) = float.SinCos(theta1);
            var (a, b, c, d) = (new Vector3(x0, 1, z0), new Vector3(x0, -1, z0), new Vector3(x1, -1, z1), new Vector3(x1, 1, z1));
            vertices[i++] = topCenter;
            vertices[i++] = a;
            vertices[i++] = d;

            vertices[i++] = a;
            vertices[i++] = b;
            vertices[i++] = c;

            vertices[i++] = a;
            vertices[i++] = c;
            vertices[i++] = d;

            vertices[i++] = bottomCenter;
            vertices[i++] = c;
            vertices[i++] = b;
        }
    }
}
