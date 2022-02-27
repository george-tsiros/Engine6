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
using System.Runtime.Serialization.Formatters.Binary;

class Engine {

    static void Render (Model m, Vector2i imageSize, string filepath) {
        var faceCount = m.Faces.Count;
        const float yFov = (float)(Math.PI / 4);
        var aspectRatio = (double)imageSize.X / imageSize.Y;
        var xFov = yFov * aspectRatio;

        //Geometry.Transform(m.Vertices, Matrix4x4.CreateRotationX((float)Math.PI / 6));
        //Geometry.Transform(m.Vertices, Matrix4x4.CreateTranslation(0, -1, -10));

        var faces = new (Vector3 A, Vector3 B, Vector3 C)[faceCount];
        for (int i = 0; i < faceCount; i++) {
            var f = m.Faces[i];
            faces[i] = (m.Vertices[f.i], m.Vertices[f.j], m.Vertices[f.k]);
        }

        var pixelCount = imageSize.X * imageSize.Y;
        var hitData = new HitData[pixelCount];
        var (dphi, dtheta) = (yFov / imageSize.Y, xFov / imageSize.X);

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
                var faceIndex = Geometry.Hit(ray, faces);
                hitData[rowStart + x] = new(faceIndex, ray);
            }
        });
        var sec = (double)(Stopwatch.GetTimestamp() - t0) / Stopwatch.Frequency;
        Console.WriteLine($"{faceCount} faces, {sec} s, {pixelCount / sec} px/s");
        using var file = File.Create(filepath);
        Ser(file, hitData);
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
        const string renderPath = "teapot.bin";
        const string modelPath = @"data\teapot.obj";


        var size = new Vector2i(640, 480);
        using (var o = new BlitTest(size)) {
            o.Run();
        }

        //Model model = new(modelPath);

        //Geometry.Transform(model.Vertices, Matrix4x4.CreateRotationX((float)Math.PI / 6));
        //Geometry.Transform(model.Vertices, Matrix4x4.CreateTranslation(0, -1, -10));

        //if (!File.Exists(renderPath))
        //    Render(model, size, renderPath);

        //var hitData = ReadArray<HitData>(renderPath);
        //var pixelCount = hitData.Length;
        //var image = new Raster(size, 1, 1);

        //var (dmin, dmax) = (float.MaxValue, float.MinValue);
        //var floats = new float[pixelCount];

        //for (var i = 0; i < hitData.Length; i++) {
        //    var h = hitData[i];
        //    if (h.FaceIndex >= 0) {
        //        var (fi, fj, fk) = model.Faces[h.FaceIndex];
        //        var f = (model.Vertices[fi], model.Vertices[fj], model.Vertices[fk]);
        //        _ = Geometry.IsHit(f, h.Ray, out var x);
        //        (dmin, dmax) = (Math.Min(dmin, x.Z), Math.Max(dmax, x.Z));
        //        floats[i] = x.Z;
        //    }
        //}

        //var a = 255f / (dmin - dmax);
        //var b = 255f * dmax / (dmax - dmin);

        //for (var i = 0; i < pixelCount; i++)
        //    if (hitData[i].FaceIndex >= 0)
        //        image.Pixels[i] = (byte)(a * floats[i] + b);

        //using var w = new ImageWindow(image);
        //w.Run();
    }
    unsafe public static T[] ReadArray<T> (string filepath) where T : struct {
        var longFileSize = new FileInfo(filepath).Length;
        var itemCount = Math.DivRem(longFileSize, Marshal.SizeOf<T>(), out var r);
        if (r != 0)
            throw new Exception();
        var fileSize = longFileSize <= int.MaxValue ? (int)longFileSize : throw new Exception();
        var a = new T[itemCount];
        var h = GCHandle.Alloc(a, GCHandleType.Pinned);

        try {
            var span = new Span<byte>(h.AddrOfPinnedObject().ToPointer(), fileSize);
            using (var f = File.OpenRead(filepath))
                f.Read(span);
            return a;
        } finally {
            h.Free();
        }
    }

    public static void Ser<T> (Stream stream, IEnumerable<T> items) where T : struct {
        var size = Marshal.SizeOf<T>();
        foreach (var t in items)
            Ser<T>(stream, t, size);
    }
    unsafe public static void Ser<T> (Stream stream, T t, int? size = null) where T : struct {
        var s = size ?? Marshal.SizeOf<T>();
        var h = GCHandle.Alloc(t, GCHandleType.Pinned);
        try {
            var span = new Span<byte>(h.AddrOfPinnedObject().ToPointer(), s);
            stream.Write(span);
        } finally {
            h.Free();
        }
    }

    public static IEnumerable<T> Enum<T> (Stream stream) where T : struct {
        var size = Marshal.SizeOf<T>();
        while (stream.Position < stream.Length)
            yield return Deser<T>(stream, size);
    }
    unsafe public static T Deser<T> (Stream stream, int? size = null) where T : struct {
        var t = new T();
        var s = size ?? Marshal.SizeOf<T>();
        var h = GCHandle.Alloc(t, GCHandleType.Pinned);
        try {
            var span = new Span<byte>(h.AddrOfPinnedObject().ToPointer(), s);
            var read = stream.Read(span);
            return read == size ? t : throw new Exception("not enough data");
        } finally {
            h.Free();
        }
    }
}
readonly struct HitData {
    public readonly int FaceIndex;
    public readonly Ray Ray;
    public HitData (int face, Ray ray) => (FaceIndex, Ray) = (face, ray);
}
