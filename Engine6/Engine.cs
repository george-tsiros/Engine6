#define __ROWBYROW
namespace Engine;

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Gl;
using Win32;
using System.Text;
using System.Numerics;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.Serialization;

class Engine {

    static Raster RenderPrepared (List<(int i, int j, int k)> indices, List<Vector3> vertices, Vector2i imageSize) {
        var faceCount = indices.Count;
        const float yFov = (float)(Math.PI / 4);
        var aspectRatio = (double)imageSize.X / imageSize.Y;
        var xFov = yFov * aspectRatio;

        var vectors = vertices.ToArray();

        Geometry.Transform(vectors, Matrix4x4.CreateRotationX((float)Math.PI / 6));
        Geometry.Transform(vectors, Matrix4x4.CreateTranslation(0, -1, -10));

        var faces = new (Vector3, Vector3)[faceCount];
        for (int i = 0; i < faceCount; i++) {
            var f = indices[i];
            var c = vectors[f.k];
            faces[i] = (vectors[f.i] - c, vectors[f.j] - c);
        }

        var pixelCount = imageSize.X * imageSize.Y;
        var depthBuffer = new float[pixelCount];
        var dphi = yFov / imageSize.Y;
        var dtheta = xFov / imageSize.X;

        var t0 = Stopwatch.GetTimestamp();
#if __ROWBYROW
        _ = Parallel.For(0, imageSize.Y, y => {
            var rowStart = y * imageSize.X;
            var phi = (.5 - (double)y / imageSize.Y) * yFov;
            var cosPhi = Math.Cos(phi);
            var yRay = (float)Math.Sin(phi);
            for (var (x, t) = (0, -.5f * xFov); x < imageSize.X; x++, t += dtheta) {
                var xRay = (float)(Math.Sin(t) * cosPhi);
                var zRay = (float)(Math.Cos(t) * cosPhi);
                depthBuffer[rowStart + x] = Geometry.Distance(new(xRay, yRay, -zRay), faces);
            }
        });
#else
        for (var (y, phi) = (0, -.5f * yFov); y < imageSize.Y; y++, phi += dphi) {
            var cosPhi = Math.Cos(phi);
            var yRay = (float)Math.Sin(phi);
            for (var (x, t) = (0, -.5f * xFov); x < imageSize.X; x++, t += dtheta) {
                var xRay = (float)(Math.Sin(t) * cosPhi);
                var zRay = (float)(Math.Cos(t) * cosPhi);
                depthBuffer[y * imageSize.X + x] = Geometry.Distance(new(Vector3.Zero, new(xRay, yRay, -zRay)), triangles);
            }
        }
#endif

        var sec = (double)(Stopwatch.GetTimestamp() - t0) / Stopwatch.Frequency;
        Console.WriteLine($"{faceCount} faces, {sec} s, {pixelCount / sec} px/s");

        var (dmin, dmax) = (float.MaxValue, float.MinValue);
        foreach (var d in depthBuffer)
            if (d < float.MaxValue)
                (dmin, dmax) = (Math.Min(dmin, d), Math.Max(dmax, d));

        var a = 255f / (dmin - dmax);
        var b = 255f * dmax / (dmax - dmin);

        var raster = new Raster(imageSize, 1, 1);

        Debug.Assert(pixelCount == raster.Pixels.Length);
        for (int i = 0; i < pixelCount; i++) {
            var d = depthBuffer[i];
            if (d < float.MaxValue)
                raster.Pixels[i] = (byte)(a * d + b);
        }
        return raster;
    }

    static HitData[] Render (Model m, Vector2i imageSize) {
        var faceCount = m.Faces.Count;
        const float yFov = (float)(Math.PI / 4);
        var aspectRatio = (double)imageSize.X / imageSize.Y;
        var xFov = yFov * aspectRatio;

        Geometry.Transform(m.Vertices, Matrix4x4.CreateRotationX((float)Math.PI / 6));
        Geometry.Transform(m.Vertices, Matrix4x4.CreateTranslation(0, -1, -10));

        var faces = new (Vector3 A, Vector3 B, Vector3 C)[faceCount];
        for (int i = 0; i < faceCount; i++) {
            var f = m.Faces[i];
            faces[i] = (m.Vertices[f.i], m.Vertices[f.j], m.Vertices[f.k]);
        }

        var pixelCount = imageSize.X * imageSize.Y;
        var hitData = new HitData[pixelCount];
        var dphi = yFov / imageSize.Y;
        var dtheta = xFov / imageSize.X;

        var t0 = Stopwatch.GetTimestamp();
        _ = Parallel.For(0, imageSize.Y, y => {
            var rowStart = y * imageSize.X;
            var phi = (.5 - (double)y / imageSize.Y) * yFov;
            var cosPhi = Math.Cos(phi);
            var yRay = (float)Math.Sin(phi);
            for (var (x, t) = (0, -.5f * xFov); x < imageSize.X; x++, t += dtheta) {
                var xRay = (float)(Math.Sin(t) * cosPhi);
                var zRay = (float)(Math.Cos(t) * cosPhi);
                var direction = new Vector3(xRay, yRay, -zRay);
                var ray = new Ray(Vector3.Zero, direction);
                var faceIndex = Geometry.Hit(ray, faces, out var hit);
                hitData[rowStart + x] = new(faceIndex, ray);
            }
        });
        var sec = (double)(Stopwatch.GetTimestamp() - t0) / Stopwatch.Frequency;
        Console.WriteLine($"{faceCount} faces, {sec} s, {pixelCount / sec} px/s");
        return hitData;
        //var (dmin, dmax) = (float.MaxValue, float.MinValue);
        //foreach (var d in hitData)
        //    if (d < float.MaxValue)
        //        (dmin, dmax) = (Math.Min(dmin, d), Math.Max(dmax, d));

        //var a = 255f / (dmin - dmax);
        //var b = 255f * dmax / (dmax - dmin);

        //var raster = new Raster(imageSize, 1, 1);

        //Debug.Assert(pixelCount == raster.Pixels.Length);
        //for (int i = 0; i < pixelCount; i++) {
        //    var d = hitData[i];
        //    if (d < float.MaxValue)
        //        raster.Pixels[i] = (byte)(a * d + b);
        //}
        //return raster;
    }

    [STAThread]
    static void Main (string[] args) {
        throw new NotImplementedException();
    }
}
readonly struct HitData {
    public readonly int FaceIndex;
    public readonly Ray Ray;
    public HitData (int face, Ray ray) => (FaceIndex, Ray) = (face, ray);
}
