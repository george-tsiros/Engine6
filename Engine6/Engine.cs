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

class Engine {

    static Raster Render (List<(int i, int j, int k)> faces, List<Vector3> vertices, Vector2i imageSize) {

        var faceCount = faces.Count;
        const float yFov = (float)(Math.PI / 4);
        var aspectRatio = (double)imageSize.X / imageSize.Y;
        var xFov = yFov * aspectRatio;

        var vectors = vertices.ToArray();

        Geometry.Transform(vectors, Matrix4x4.CreateRotationX((float)Math.PI / 6));
        Geometry.Transform(vectors, Matrix4x4.CreateTranslation(0, -1, -10));

        var triangles = new (Vector3, Vector3, Vector3)[faceCount];
        for (int i = 0; i < faceCount; i++) {
            var f = faces[i];
            triangles[i] = (vectors[f.i], vectors[f.j], vectors[f.k]);
        }

        var pixelCount = imageSize.X * imageSize.Y;
        var depthBuffer = new float[pixelCount];
        var dphi = yFov / imageSize.Y;
        var dtheta = xFov / imageSize.X;

        var t0 = Stopwatch.GetTimestamp();
        _ = Parallel.For(0, imageSize.Y, y => {
            Console.Write('.');
            var rowStart = y * imageSize.X;
            var phi = (.5f - (float)y / imageSize.Y) * yFov;
            var theta = -0.5f * xFov;
            var cosPhi = Math.Cos(phi);
            var yRay = (float)Math.Sin(phi);
            for (var x = 0; x < imageSize.X; x++) {
                var xRay = (float)(Math.Sin(theta) * cosPhi);
                var zRay = (float)(Math.Cos(theta) * cosPhi);
                var ray = new Ray(Vector3.Zero, new Vector3(xRay, yRay, -zRay));
                depthBuffer[rowStart + x] = Geometry.Distance(ray, triangles);
                theta += dtheta;
            }
        });
        var sec = (double)(Stopwatch.GetTimestamp() - t0) / Stopwatch.Frequency;
        Console.WriteLine($"{sec} s, {pixelCount / sec} px/s");

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
    static Raster Render (Model m, Vector2i size) => Render(m.Faces, m.Vertices, size);

    [STAThread]
    static void Main (string[] args) {
        using var teapot = new Teapot(new(640, 480), new("data/teapot.obj"));
        teapot.Run();
        //var cases = new List<(string name, ConstructorInfo ci)>();

        //var constructorTypeArray = new Type[] { typeof(Vector2i) };
        //foreach (var t in Assembly.GetExecutingAssembly().GetTypes())
        //    if (t.BaseType == typeof(GlWindow) && t.GetConstructor(constructorTypeArray) is ConstructorInfo ci)
        //        cases.Add((t.Name, ci));

        //for (var i = 0; i < cases.Count; ++i)
        //    Console.WriteLine($"{i + 1}: {cases[i].name}");

        //var choice = -1;
        //while (choice < 0 || cases.Count < choice)
        //    choice = int.TryParse(Console.ReadLine(), out var i) && 0 <= i && i <= cases.Count ? i : -1;

        //Debug.Assert(0 <= choice && choice <= cases.Count);

        //if (choice == 0)
        //    return;

        //using var eh = cases[choice - 1].ci.Invoke(new object[] { new Vector2i(640, 480) }) as GlWindow;
        //eh.Run();
    }
}
