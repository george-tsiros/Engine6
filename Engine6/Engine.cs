//#define __PARALLEL
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
    //unsafe private static void TestRawDevices () {
    //    uint deviceCount = 0;
    //    _ = User.GetRawInputDeviceList(null, &deviceCount, (uint)RawInputDeviceList.Size);
    //    var devices = new RawInputDeviceList[deviceCount];
    //    fixed (RawInputDeviceList* p = devices) {
    //        var eh = User.GetRawInputDeviceList(p, &deviceCount, (uint)RawInputDeviceList.Size);
    //        if (eh != devices.Length)
    //            throw new Exception();
    //    }

    //    var devInfo = new RawInputDeviceInfo() { size = (uint)Marshal.SizeOf<RawInputDeviceInfo>() };
    //    var infoSize = devInfo.size;
    //    var charCount = 0u;
    //    var array = new ushort[1024];
    //    for (var i = 0; i < devices.Length; i++) {

    //        var ptr = devices[i].device;
    //        var x = User.GetRawInputDeviceInfoW(ptr, User.RawInputDeviceCommand.DeviceInfo, &devInfo, &infoSize);
    //        Debug.Assert(x > 0, $"{Kernel.GetLastError():x}");
    //        var type = devInfo.type;
    //        Console.WriteLine($"{i}: {type}, ");
    //        switch (type) {
    //            case RawInputDeviceType.Hid: {
    //                    DebugDump(devInfo.hid, MemberTypes.Field);
    //                }
    //                break;
    //            case RawInputDeviceType.Keyboard: {
    //                    DebugDump(devInfo.keyboard, MemberTypes.Field);
    //                }
    //                break;
    //            case RawInputDeviceType.Mouse: {
    //                    DebugDump(devInfo.mouse, MemberTypes.Field);
    //                }
    //                break;
    //        }

    //        var y = User.GetRawInputDeviceInfoW(ptr, User.RawInputDeviceCommand.DeviceName, null, &charCount);
    //        if (4l * charCount < 1024l) {

    //            fixed (ushort* ushorts = array)
    //                _ = User.GetRawInputDeviceInfoW(ptr, User.RawInputDeviceCommand.DeviceName, ushorts, &charCount);
    //            string name = TryGetString(array, (int)charCount);

    //            Console.WriteLine($"charCount: {charCount}, len: '{name}'");
    //        } else
    //            Console.WriteLine($"{charCount} ?!");
    //    }
    //}

    private static void DebugDump (object ob, MemberTypes types) {
        if (ob is null)
            return;
        var type = ob.GetType();
        Debug.WriteLine(type.Name);
        foreach (var m in type.GetMembers())
            if (types.HasFlag(m.MemberType))
                switch (m) {
                    case PropertyInfo pi:
                        if (pi.CanRead)
                            Debug.WriteLine($"<{pi.PropertyType.Name}> {pi.Name} : '{pi.GetValue(ob)}'");
                        break;
                    case FieldInfo fi:
                        Debug.WriteLine($"<{fi.FieldType.Name}> {fi.Name} : '{fi.GetValue(ob)}'");
                        break;
                }
    }

    private static string TryGetString (ushort[] span, int maxLength) {
        Debug.Assert(maxLength < 1024);
        var l = 0;
        while (l < maxLength && span[l] != 0)
            l++;
        if (l == 0)
            return "";
        Span<byte> bytes = stackalloc byte[l];
        for (var i = 0; i < l; i++)
            bytes[i] = (byte)span[i];
        return Encoding.ASCII.GetString(bytes);
    }

    static void Quaternions () {
        var x = Quaternion.CreateFromAxisAngle(Vector3.UnitX, 0);
        var y = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(Math.PI / 2));
    }

    static int? ParseInt32 (string str) => int.TryParse(str, out var i) ? i : null;

    //static float Depth (in Vector3 ray, in Tri[] faces, int taskId, int taskCount) {
    //    var depth = float.MaxValue;
    //    for (var i = taskId; i < faces.Length; i += taskCount)
    //        depth = Math.Min(Tri.Distance(faces[i], ray), depth);
    //    return depth;
    //}

    static Raster Render (List<(int i, int j, int k)> faces, List<Vector3> vertices, Vector2i imageSize) {
        const float zNear = 1f;
        const float zFar = 100f;

        var faceCount = faces.Count;
        int vertexCount = vertices.Count;

        var yFov = Math.PI / 4;
        var aspectRatio = (double)imageSize.X / imageSize.Y;
        var xFov = yFov * aspectRatio;

        var modelMatrix = Matrix4x4.CreateRotationY((float)Math.PI / 6f) * /*Matrix4x4.CreateRotationX((float)Math.PI / 6f) **/ Matrix4x4.CreateTranslation(0, -1, -15);
        var viewMatrix = Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY);
        var projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), imageSize.X / imageSize.Y, zNear, zFar);

        var triangles = new Tri[faceCount];
        var vectors = new Vector3[vertexCount];
        var mvp = modelMatrix * viewMatrix * projectionMatrix;

        for (int i = 0; i < vertexCount; i++)
            vectors[i] = Vector3.Transform(vertices[i], mvp);

        for (int i = 0; i < faceCount; i++) {
            var f = faces[i];
            triangles[i] = Tri.FromPoints(vectors[f.i], vectors[f.j], vectors[f.k]);
        }

        var pixelCount = imageSize.X * imageSize.Y;
        var depthBuffer = new float[pixelCount];
        var dphi = yFov / imageSize.Y;
        var dtheta = xFov / imageSize.X;
#if __PARALLEL
        const int Parallelism = 2;
        var tasks = new Task<float>[Parallelism];
#else
#endif
        var t0 = Stopwatch.GetTimestamp();

        for (var (yPixel, phi) = (0, 0.5f * yFov); yPixel < imageSize.Y; ++yPixel, phi -= dphi) {
            Console.WriteLine($"{yPixel}/{imageSize.Y}");
            for (var (xPixel, theta) = (0, -0.5f * xFov); xPixel < imageSize.X; ++xPixel, theta += dtheta) {
                var cosPhi = Math.Cos(phi);
                var yRay = (float)Math.Sin(phi);
                var xRay = (float)(Math.Sin(theta) * cosPhi);
                var zRay = (float)(Math.Cos(theta) * cosPhi);
                var r = new Vector3(xRay, yRay, zRay);
                Debug.Assert(Math.Abs(r.Length() - 1f) < 1e-7f);
                var ray = new Ray(Vector3.Zero, r);
                var depth = float.MaxValue;
#if __PARALLEL
                for (var i = 0; i < Parallelism; i++)
                    tasks[i] = Task.Run(() => Depth(ray, triangles, i, Parallelism));
                Task.WaitAll(tasks);
                for (var i = 0; i < Parallelism; i++)
                    depth = Math.Min(depth, tasks[i].Result);
#else
                depth = Depth(ray, triangles, 0, 1);
#endif
                depthBuffer[yPixel * imageSize.X + xPixel] = depth;
            }
        }
        var sec = (double)(Stopwatch.GetTimestamp() - t0) / Stopwatch.Frequency;
        Console.WriteLine($"{sec} s, {pixelCount / sec} px/s");
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
        var b = 255f * dmax / (dmax - dmin);
        for (int i = 0; i < pixelCount; i++) {
            var d = depthBuffer[i];
            if (d < float.MaxValue)
                bytes[i] = (byte)(a * d + b);
        }

        var raster = new Raster(imageSize, 1, 1);
        Debug.Assert(pixelCount == raster.Pixels.Length);
        Array.Copy(bytes, raster.Pixels, pixelCount);
        return raster;
    }
    /*
    _    _    _   _    _
    T + aA + bB = R + dD
     _    _    _   _ _
    aA + bB - dD = R-T

    a * X + b * X - d * X = X - X
         A       B       D   R   T

    a * Y + b * Y - d * Y = Y - Y
         A       B       D   R   T

    a * Z + b * Z - d * Z = Z - Z
         A       B       D   R   T

    [ X  X  -X  ]
    [  A  B   D ]
    [           ] [a]
    [ Y  Y  -Y  ] [b] = R-T
    [  A  B   D ] [d] 
    [           ]
    [ Z  Z  -Z  ]
    [  A  B   D ]

*/
    static float Depth (in Ray ray, in Tri[] faces, int taskId, int taskCount) {
        var depth = float.MaxValue;
        for (var i = taskId; i < faces.Length; i += taskCount) {
            var face = faces[i];
            var m = new Matrix3x3(face.Va, face.Vb, -ray.Direction);
            var p = ray.Origin - face.Origin;
            var possible = m.TrySolve(p, out var x);
            if (possible && 0 < x.X && x.X < 1 && 0 < x.Y && x.Y < 1 && 0 < x.Z && x.X + x.Y <= 0.5f) {
                depth = Math.Min(depth, x.Z);
            }
        }
        return depth;
    }
    static Raster Render (Model m, Vector2i size) => Render(m.Faces, m.Vertices, size);

    [STAThread]
    static void Main (string[] args) {
        var faces = new List<(int, int, int)> {
            (1, 5, 6), (6, 2, 1), // right
            (0, 3, 7), (7, 4, 0), // left
            (4, 7, 6), (6, 5, 4), // top
            (0, 1, 2), (2, 3, 0), // bottom
            (2, 6, 7), (7, 3, 2), // near
            (0, 4, 5), (5, 1, 0), // far

        };
        var vertices = new List<Vector3> {
            new(-0.5f, -0.5f, -0.5f),
            new(+0.5f, -0.5f, -0.5f),
            new(+0.5f, -0.5f, +0.5f),
            new(-0.5f, -0.5f, +0.5f),
            new(-0.5f, +0.5f, -0.5f),
            new(+0.5f, +0.5f, -0.5f),
            new(+0.5f, +0.5f, +0.5f),
            new(-0.5f, +0.5f, +0.5f),
        };

        using var raster = Render(/*new Model(@"data\teapot.obj")*/faces,vertices, new(320, 240));
        using var gl = new ImageWindow(raster);
        gl.Run();
    }
}
