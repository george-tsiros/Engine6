namespace Engine;

using System;
using System.Numerics;
using Shaders;
using Gl;
using static Gl.Opengl;
using static Gl.Utilities;
using Win32;
using System.Diagnostics;
using System.Collections.Generic;
readonly struct Ray {
    public readonly Vector3 Origin, Direction;
    public Ray (Vector3 origin, Vector3 direction) {
        Origin = origin;
        Direction = Vector3.Normalize(direction);
    }
}
readonly struct Tri {
    public readonly Vector3 Origin, Va, Vb;
    public Vector3 Normal => Vector3.Normalize(Vector3.Cross(Va, Vb));
    public Vector3 A => Origin + Va;
    public Vector3 B => Origin + Vb;
    public Vector3 Center => Origin + (Va + Vb) * 0.5f;
    public Tri (Vector3 origin, Vector3 a, Vector3 b) => (Origin, Va, Vb) = (origin, a, b);
    public static Tri FromPoints (Vector3 a, Vector3 b, Vector3 c) => new(a, b - a, c - a);
}



class BlitTest:GlWindow {
    public BlitTest (Vector2i size) : base(size) { }
    private Camera Camera { get; } = new(new(0, 0, 5));
    private Raster raster;
    private Sampler2D sampler;
    private VertexArray quad;
    private VertexBuffer<Vector4> quadBuffer;

    int PointCount;
    Vector4[] points3D;
    Vector2i[] points2Di;

    static readonly byte[] White = { 255, 255, 255, 255 };
    float theta;
    const float TwoPi = (float)(Math.PI * 2);

    protected override void Load () {
        const string TeapotFilepath = @"C:\dev\src\Ogl\GlfwTryout\teapot.obj";
        var teapot = new Model(TeapotFilepath);
        PointCount = teapot.Vertices.Count;
        points3D = new Vector4[PointCount];
        for (var i = 0; i < PointCount; ++i)
            points3D[i] = new(teapot.Vertices[i], 1f);
        points2Di = new Vector2i[PointCount];

        quad = new();
        State.Program = PassThrough.Id;
        quadBuffer = new(Quad.Vertices);
        quad.Assign(quadBuffer, PassThrough.VertexPosition);
        raster = new(new(Width, Height), 4, 1);
        MemSet(raster.Pixels, 0xff00000fu);

        sampler = new(raster.Size, TextureFormat.Rgba8);
        sampler.Mag = MagFilter.Nearest;
        sampler.Min = MinFilter.Nearest;
        sampler.Wrap = Wrap.ClampToEdge;
    }

    unsafe static void PutPixels (Raster b, IEnumerable<Vector2i> points) {
        Debug.Assert(b.Stride == b.Width * 4);

        foreach (var p in points)
            if (0 <= p.X && 0 <= p.Y && p.X < b.Width && p.Y < b.Height)
                Array.Copy(White, 0, b.Pixels, b.Stride * p.Y + 4 * p.X, 4);

    }
        static bool Intersects (Tri q, Ray r, out float f) {
        var intersects = TrySolve(q, r, out Vector3 s);
        f = s.X;
        return intersects && 0 < s.X && 0 < s.Y && s.Y + s.Z < 0.5f && 0 < s.Z;
        //return intersects && 0 < s.X && 0 < s.Y && s.Y < 1 && 0 < s.Z && s.Z < 1;
    }

    static bool TrySolve (Tri vec, Ray ray, out Vector3 solution) {
        solution = Vector3.Zero;
        var q = new Tri(vec.Origin - ray.Origin, vec.Va, vec.Vb);
        var det = Det(ray.Direction, q.Va, q.Vb);
        if (Math.Abs(det) < 1e-10f)
            return false;
        var x = Det(q.Origin, q.Va, q.Vb) / det;
        var y = Det(ray.Direction, q.Origin, q.Vb) / det;
        var z = Det(ray.Direction, q.Va, q.Origin) / det;
        solution = new Vector3(x, y, z);
        return true;
    }
        static float Det (Vector3 d, Vector3 a, Vector3 b) {
        var d0 = d.X * ((double)a.Y * b.Z - b.Y * a.Z);
        var d1 = a.X * ((double)d.Y * b.Z - b.Y * d.Z);
        var d2 = b.X * ((double)d.Y * a.Z - a.Y * d.Z);
        return (float)(d0 - d1 + d2);
    }
        const string TeapotFilepath = @"C:\dev\src\Ogl\GlfwTryout\teapot.obj";
    const float zNear = 1f;
    const float zFar = 50f;
    const float aDepth = 255f / (zNear - zFar);
    const float bDepth = 127.5f * (1f + (zFar + zNear) / (zFar - zNear));

    static void NotMain () { 
            var teapot = new Model(TeapotFilepath);
        var faceCount = teapot.Faces.Count;
        int vertexCount = teapot.Vertices.Count;

        var imageSize = new Vector2i(320, 240);
        var yFov = Math.PI / 4;
        var aspectRatio = (double)imageSize.X / imageSize.Y;
        var xFov = yFov * aspectRatio;

        var model = Matrix4x4.CreateRotationY((float)Math.PI / 4f) * Matrix4x4.CreateTranslation(0, -1, -15);
        //var view = Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY);
        //var projection = Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), imageSize.X / imageSize.Y, zNear, zFar);

        var faces = new Tri[faceCount];
        var vertices = new Vector3[vertexCount];
        for (int i = 0; i < vertexCount; i++) {
            var v = Vector4.Transform(new Vector4(teapot.Vertices[i], 1), model); ;
            vertices[i] = new(v.X, v.Y, v.Z);
        }
        for (int i = 0; i < faceCount; i++) {
            var f = teapot.Faces[i];
            faces[i] = Tri.FromPoints(vertices[f.i], vertices[f.j], vertices[f.k]);
        }
        var pixelCount = imageSize.X * imageSize.Y;
        var depthBuffer = new float[pixelCount];
        var dphi = yFov / imageSize.Y;
        var dtheta = xFov / imageSize.X;
        var depth = float.MaxValue;
        for (var (yPixel, phi) = (0, -0.5f * yFov); yPixel < imageSize.Y; ++yPixel, phi += dphi) {
            Debug.WriteLine(yPixel);
            var cosPhi = Math.Cos(phi);
            var yRay = Math.Sin(phi);

            for (var (xPixel, theta) = (0, -0.5f * xFov); xPixel < imageSize.X; ++xPixel, theta += dtheta) {

                var xRay = Math.Sin(theta) * cosPhi;
                var zRay = Math.Cos(theta) * cosPhi;

                var ray = new Ray(Vector3.Zero, new((float)xRay, (float)yRay, (float)zRay));

                for (var i = 0; i < faceCount; i++)
                    if (Intersects(faces[i], ray, out var d) && d < depth)
                        depth = d;

                depthBuffer[yPixel * imageSize.X + xPixel] = depth;
            }
        }
        var bytes = new byte[pixelCount];
        var (dmin, dmax) = (float.MaxValue, float.MinValue);
        foreach (var d in depthBuffer)
            if (d < float.MaxValue)
                (dmin, dmax) = (Math.Min(dmin, d), Math.Max(dmax, d));

        /*
        dmin -> 255
        dmax -> 0
        255 = a * dmin + b
        0 = a * dmax + b

        255 = a (dmin-dmax) => a = 255/(dmin-dmax)
        b= -a * dmax

*/
        var a = 255f / (dmin - dmax);
        var b /*= -a * dmax;*/= 255f * dmax / (dmax - dmin);
        for (int i = 0; i < pixelCount; i++) {
            var d = depthBuffer[i];
            if (d < float.MaxValue)
                bytes[i] = (byte)(a * d + b);
        }

    }


    protected override void Render (float dt) {
        theta += 0.1f * dt;
        if (theta > TwoPi)
            theta -= TwoPi;
        var t0 = Stopwatch.GetTimestamp();
        var model = Matrix4x4.CreateRotationY(theta) * Matrix4x4.CreateTranslation(0, -1, -20);

        var view = Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY);

        var projection = Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)Width / Height, 1, 100);
        var mvp = model * view * projection;

        for (int i = 0; i < PointCount; i++) {
            var p = Vector4.Transform(points3D[i], mvp);
            Debug.Assert(p.W > 1e-10);
            var n = new Vector2(p.X / p.W, p.Y / p.W);
            points2Di[i] = new((int)(Width * (n.X + 0.5f)), (int)(Height * (-n.Y + 0.5f)));
        }
        MemSet(raster.Pixels, 0xff00000fu);
        PutPixels(raster, points2Di);
        Debug.WriteLine($"{1000f * (Stopwatch.GetTimestamp() - t0) / Stopwatch.Frequency}");
        sampler.Upload(raster);
        glViewport(0, 0, Width, Height);
        glClear(BufferBit.Color | BufferBit.Depth);
        State.Program = PassThrough.Id;
        State.VertexArray = quad;
        State.DepthTest = true;
        State.DepthFunc = DepthFunction.Always;
        State.CullFace = true;
        sampler.BindTo(1);
        PassThrough.Tex(1);
        glDrawArrays(Primitive.Triangles, 0, 6);
    }

    protected override void Closing () {
        raster.Dispose();
        sampler.Dispose();
        quad.Dispose();
        quadBuffer.Dispose();
    }
}
