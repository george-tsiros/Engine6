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

    static float Edge (in Vector2 a, in Vector2 b, in Vector2 p) =>
        (p.X - a.X) * (b.Y - a.Y) - (p.Y - a.Y) * (b.X - a.X);

    //static Vector2 Scale(Vector2 a, Vector2


    [STAThread]
    static void Main (string[] args) {
        var model = Model.Quad(1, 1);
        var size = args.Length > 1 && int.TryParse(args[0], out var width) && int.TryParse(args[1], out var height) && 320 <= width && width <= 2560 && 240 <= height && height <= 1440 ? new Vector2i(width, height) : new(1280, 720);
        var f = args.Length > 3 && float.TryParse(args[3], out var emSize) ? new Font(args[2], emSize) : new Font("data\\IBM_3270.txt");
        using var bt = new BlitTest(size, model) { Font = f };
        bt.Run();
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
