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

    [STAThread]
    static void Main (string[] args) {
        const string ModelDir = @"C:\Users\tsiros\Downloads\wavefront\";
        //TestDirectSound();
        //foreach (var f in Directory.EnumerateFiles(ModelDir, "*.zip", SearchOption.TopDirectoryOnly))
        //    TryUnzip(f);
        //_ = Parallel.ForEach(Directory.EnumerateFiles(ModelDir, "*.zip", SearchOption.TopDirectoryOnly), TryUnzip);
        const string modelFilepath = ModelDir + "Fence post_fencepost2.obj";

        //Console.WriteLine("done");
        //_ = Console.ReadLine();
        //var raster = Render(new(modelFilepath), new(512,256));
        //using var iw = new ImageWindow(raster);
        //iw.Run();
        //using var x = new TextureTest(new(1280, 720));
        //x.Run();
        var m = Model.Cube(1,1,1);
        using var bt = new BlitTest(new(1280, 720));
        bt.Run();
        //using var teapot = new Teapot(new(1024, 512), new(modelFilepath));
        //teapot.Run();
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
