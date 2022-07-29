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
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

class Engine {

    static Raster Render (List<Vector3i> faces, List<Vector3> vertices, Vector2i imageSize) {

        var faceCount = faces.Count;
        const double yFov = double.Pi / 4;
        var aspectRatio = (double)imageSize.X / imageSize.Y;
        var yTop = double.Tan(yFov / 2);
        var xRight = yTop * aspectRatio;
        var dy = 2 * yTop / imageSize.Y;
        var dx = 2 * xRight / imageSize.X;

        //var vectors = vertices.ToArray();

        Geometry.Transform(vertices, Matrix4x4.CreateTranslation(0, 0, -5));

        var triangles = new List<(Vector3, Vector3, Vector3)>();// [faceCount];
        foreach (var f in faces)
            triangles.Add((vertices[f.X], vertices[f.Y], vertices[f.Z]));

        var pixelCount = imageSize.X * imageSize.Y;
        var depthBuffer = new float[pixelCount];

        var t0 = Stopwatch.GetTimestamp();
        _ = Parallel.For(0, imageSize.Y, iy => {
            Console.Write('.');
            var rowStart = iy * imageSize.X;
            var y = -yTop + dy * iy;
            for (var (ix, x) = (0, -xRight); ix < imageSize.X; ix++, x += dx) {
                var ray = new Ray(Vector3.Zero, new((float)x, (float)y, -1f));
                depthBuffer[rowStart + ix] = Geometry.Distance(ray, triangles);
            }
        });

        var sec = (double)(Stopwatch.GetTimestamp() - t0) / Stopwatch.Frequency;
        Console.WriteLine($"{sec} s, {pixelCount / sec} px/s");

        var (dmin, dmax) = (float.MaxValue, float.MinValue);
        foreach (var d in depthBuffer)
            if (d < float.MaxValue)
                (dmin, dmax) = (float.Min(dmin, d), float.Max(dmax, d));

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

    static void TryUnzip (string filepath) {
        try {
            Unzip(filepath);
        } catch (Exception e) {
            Debug.WriteLine(e.Message);
        }
    }

    static void Unzip (string filepath) {
        using var memory = new MemoryStream();
        using (var stream = File.OpenRead(filepath)) {
            using var zip = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Read, false);
            var objects = zip.Entries.Where(entry => entry.Name.ToLower().EndsWith(".obj")).ToList();
            if (objects.Count != 1)
                throw new ArgumentException($"{filepath} has {objects.Count} objects in it?");
            using (var zipStream = objects[0].Open())
                zipStream.CopyTo(memory);
        }
        memory.Position = 0;
        using (var f = new StreamReader(memory, leaveOpen: true))
            _ = new Model(f);

        memory.Position = 0;
        var copyFilepath = Path.Combine(Path.GetDirectoryName(filepath), Path.GetFileNameWithoutExtension(filepath) + ".obj");
        Console.WriteLine(copyFilepath);
        using (var copy = File.Create(copyFilepath))
            memory.CopyTo(copy);
    }

    static string HashFile (string filepath) {
        using var md5 = System.Security.Cryptography.MD5.Create();
        using var f = File.OpenRead(filepath);
        return Convert.ToBase64String(md5.ComputeHash(f));
    }

    static string HashStream (Stream stream) {
        using var md5 = System.Security.Cryptography.MD5.Create();
        return Convert.ToBase64String(md5.ComputeHash(stream));
    }

    static void Omg () {
        var hashes = new HashSet<string>(Directory.EnumerateFiles(ModelDir, "*.obj").Select(HashFile));

        foreach (var filepath in Directory.EnumerateFiles(ModelDir + "archive", "*.zip")) {
            using var f = File.OpenRead(filepath);
            using var archive = new System.IO.Compression.ZipArchive(f, System.IO.Compression.ZipArchiveMode.Read, false);
            foreach (var entry in archive.Entries.Where(entry => string.Equals(Path.GetExtension(entry.Name), ".obj", StringComparison.OrdinalIgnoreCase))) {
                using (var m = new MemoryStream()) {
                    using (var z = entry.Open())
                        z.CopyTo(m);
                    m.Position = 0;
                    using (var md5 = System.Security.Cryptography.MD5.Create())
                        if (!hashes.Add(HashStream(m)))
                            continue;
                    var unzipped = Path.Combine(ModelDir, entry.Name);
                    var name = Path.GetFileNameWithoutExtension(entry.Name);
                    for (var i = 0; File.Exists(unzipped); ++i)
                        unzipped = Path.Combine(ModelDir, $"{name}_{i}.obj");
                    m.Position = 0;
                    using (var g = File.Create(unzipped))
                        m.CopyTo(g);
                }
            }
        }
    }
    static float Edge (in Vector2 a, in Vector2 b, in Vector2 p) =>
        (p.X - a.X) * (b.Y - a.Y) - (p.Y - a.Y) * (b.X - a.X);

    //static Vector2 Scale(Vector2 a, Vector2

    static Rectangle BoundingRectangle (Vector2i a, Vector2i b, Vector2i c) => new(
        int.Min(int.Min(a.X, b.X), c.X),
        int.Max(int.Max(a.X, b.X), c.X),
        int.Min(int.Min(a.Y, b.Y), c.Y),
        int.Max(int.Max(a.Y, b.Y), c.Y)
    );

    static void Test (Vector2i size, Model model) {
        var (Width, Height) = size;
        var modelMatrix = Matrix4x4.CreateRotationY(float.Pi / 4);
        var viewMatrix = Matrix4x4.CreateLookAt(new(0, 0, float.Max(model.Max.Length(), model.Min.Length()) + 2), -Vector3.UnitZ, Vector3.UnitY);
        var projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(float.Pi / 4, (float)Width / Height, 1, 100);
        var mvp = modelMatrix * viewMatrix * projectionMatrix;
        var PointCount = model.Vertices.Count;
        var clipSpace = new Vector3[PointCount];
        var screenSpace = new Vector2i[PointCount];
        for (int i = 0; i < PointCount; i++) {
            var w = Vector4.Transform(model.Vertices[i], mvp);
            Debug.Assert(w.W > 1e-10);
            //var xy = w.Xy() / w.W;
            var x = w.X / w.W;
            var y = w.Y / w.W;
            clipSpace[i] = new(x, y, w.Z / w.W);
            // origin is at bottom left
            // orientation matches that of clipspace
            screenSpace[i] = new((int)float.Round(size.X * (.5f + .5f * x)), (int)float.Round(size.Y * (.5f + .5f * y)));
        }
        var screen = new Rectangle(Vector2i.Zero, size);
        //Console.WriteLine(p);
        for (var y = 1f; y > -1f; y += -.02f) {
            for (var x = -1f; x < 1f; x += .02f) {
                var p = new Vector2(x, y);
                var filled = false;
                foreach (var f in model.Faces) {

                    var (a, b, c) = clipSpace.Dex(f);
                    var (ax, ay, az) = a;
                    var (bx, by, bz) = b;
                    var (cx, cy, cz) = c;
                    // a = v1, b = v0
                    var wa = (p.X - cx) * (by - cy) - (p.Y - cy) * (bx - cx);
                    var wb = (p.X - ax) * (cy - ay) - (p.Y - ay) * (cx - ax);
                    var wc = (p.X - bx) * (ay - by) - (p.Y - by) * (ax - bx);
                    //var wc = Edge(b.Xy(), a.Xy(), p);
                    //var wa = Edge(c.Xy(), b.Xy(), p);
                    //var wb = Edge(a.Xy(), c.Xy(), p);
                    var isIn = wc > 0 && wa > 0 && wb > 0;
                    //var eh = (e0 > 0 ? 1 : 0) + (e1 > 0 ? 2 : 0) + (e2 > 0 ? 4 : 0);
                    if (isIn) {
                        var (ia, ib, ic) = screenSpace.Dex(f);
                        var br = BoundingRectangle(ia, ib, ic);
                        var r = br.Clip(screen);
                        filled = true;
                        break;
                    }
                }
                Console.Write(filled ? 'X' : '.');
            }
            Console.WriteLine();
        }
        //foreach (var f in model.Faces) {
        //    Console.WriteLine(f);
        //    var (v0, v1, v2) = normalizedPoints3D.Dex(f);
        //    Console.WriteLine(v0);
        //    Console.WriteLine(v1);
        //    Console.WriteLine(v2);
        //    var e = (p.X - v0.X) * (v1.Y - v0.Y) - (p.Y - v0.Y) * (v1.X - v0.X);
        //    Console.WriteLine(e);
        //    //var (v0xy, v1xy, v2xy) = (v0.Xy(), v1.Xy(), v2.Xy());
        //    //Console.WriteLine(v0xy);
        //    //Console.WriteLine(v1xy);
        //    //Console.WriteLine(v2xy);

        //}
        _ = Console.ReadLine();
    }

    const string ModelDir = @"C:\Users\tsiros\Downloads\wavefront\"; //"C:\Users\tsiros\Downloads\wavefront\Satellite dish_antenaParabolica.obj"
    [STAThread]
    static void Main (string[] args) {
        var modelFilepath = Path.Combine(ModelDir, "largecouch.obj");
#if !true
        var model = new Model(modelFilepath, true);
#else
        var model = Model.Quad(1, 1);
#endif
        var size = args.Length == 2 && int.TryParse(args[0], out var width) && int.TryParse(args[1], out var height) && 320 <= width && width <= 2560 && 240 <= height && height <= 1440 ? new Vector2i(width, height) : new(1280, 720);
        using var bt = new BlitTest(size, model);
        bt.Run();
    }

    static void Bench () {
        Raster raster = new(new(1280, 720), 4, 1);
        const int Repetitions = 100;
        long[] ticks = new long[Repetitions];
        var r = new Rectangle(new(100, 100), new(100, 100));
        for (var i = 0; i < Repetitions; ++i) {
            var t0 = Stopwatch.GetTimestamp();
            raster.FillRectU32(r);
            ticks[i] = Stopwatch.GetTimestamp() - t0;
        }
        var written = raster.Pixels.Count(b => b != 0) / 4;
        Console.WriteLine(written);
        var (min, max) = (ticks.Min(), ticks.Max());
        var (minFound, maxFound) = (false, false);
        var kept = new List<long>(Repetitions);
        foreach (var t in ticks) {
            if (t == min) {
                if (!minFound) {
                    minFound = true;
                    continue;
                }
            } else if (t == max) {
                if (!maxFound) {
                    maxFound = true;
                    continue;
                }
            }
            kept.Add(t);
        }
        Console.WriteLine(kept.Min());
        Console.WriteLine(kept.Max());
        Console.WriteLine(kept.Sum() / (Repetitions - 2.0));
    }

    unsafe private static void TestDirectSound () {
        throw new NotImplementedException();
        //var g = Guid.NewGuid();
        //void* ptr = null;
        //var eh = DSound.DirectSoundCreate(null, &ptr, null);
        //if (eh != DsResult.Ok)
        //    throw new Exception(eh.ToString());
        //if (ptr == null)
        //    throw new Exception("ptr is null");
        //var wf = new WaveFormatEx() { 
        //    formatTag=1,
        //    channels=2,
        //    samplesPerSec=48000,
        //    bitsPerSample=16,
        //    blockAlign= 4,
        //    avgBytesPerSec=48000 * 4
        //};
        //wf.size = (ushort)Marshal.SizeOf<WaveFormatEx>();
        //var bd = new BufferDescriptor() {
        //    flags=DsFlags.PrimaryBuffer,
        //};
        //bd.size = (uint)Marshal.SizeOf<BufferDescriptor>();
    }
}
//[Flags]
//enum DsFlags:uint {
//    PrimaryBuffer = 0x1,
//    Static = 0x2,
//    LocHardware = 0x4,
//    LocSoftware = 0x8,
//    Ctrl3d = 0x10,
//    CtrlFrequency = 0x20,
//    CtrlPan = 0x40,
//    CtrlVolume = 0x80,
//    CtrlPositionNotify = 0x100,
//    CtrlFx = 0x200,
//    StickyFocus = 0x4000,
//    GlobalFocus = 0x8000,
//    GetCurrentPosition2 = 0x10000,
//    Mute3DatMaxDistance = 0x20000,
//    LocDefer = 0x40000,
//    TruePlayPosition = 0x80000,
//}

//enum DsResult:uint {
//    Ok = 0x00000000,
//    OutOfMemory = 0x00000007,
//    NoInterface = 0x000001AE,
//    NoVirtualization = 0x0878000A,
//    Incomplete = 0x08780014,
//    Unsupported = 0x80004001,
//    Generic = 0x80004005,
//    AccessDenied = 0x80070005,
//    InvalidParam = 0x80070057,
//    Allocated = 0x8878000A,
//    ControlUnavail = 0x8878001E,
//    InvalidCall = 0x88780032,
//    PrioLevelNeeded = 0x88780046,
//    BadFormat = 0x88780064,
//    NoDriver = 0x88780078,
//    AlreadyInitialized = 0x88780082,
//    BufferLost = 0x88780096,
//    OtherAppHasPrio = 0x887800A0,
//    Uninitialized = 0x887800AA,
//    BufferTooSmall = 0x887810B4,
//    DS8Required = 0x887810BE,
//    SendLoop = 0x887810C8,
//    BadSendBufferFuid = 0x887810D2,
//    FxUnavailable = 0x887810DC,
//    ObjectNotFound = 0x88781161,
//}

//unsafe struct WaveFormatEx {
//    public ushort formatTag;
//    public ushort channels;
//    public uint samplesPerSec;
//    public uint avgBytesPerSec;
//    public ushort blockAlign;
//    public ushort bitsPerSample;
//    public ushort size;
//}

//[StructLayout(LayoutKind.Sequential)]
//unsafe struct BufferDescriptor {
//    public uint size;
//    public DsFlags flags;
//    public uint bufferBytes;
//    public uint reserved;
//    public WaveFormatEx* fxFormat;
//    public ulong guidA;
//    public ulong guidB;
//}
//static class DSound {

//    private const string dsound = nameof(dsound) + ".dll";
//    [DllImport(dsound, CallingConvention = CallingConvention.Winapi)]
//    unsafe public extern static DsResult DirectSoundCreate (Guid* lpGuid, void** ppDS, void* zero);
//    [DllImport(dsound, CallingConvention = CallingConvention.Winapi)]
//    unsafe public extern static DsResult CreateSoundBuffer (BufferDescriptor* bufferDesc, void* buffer, void* zero);
//}
