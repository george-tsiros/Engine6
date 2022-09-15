namespace Gl;

using System;
using System.Diagnostics;
using System.Reflection;
using static RenderingContext;

public static class Utilities {
    public static byte[] AssertShorts (byte[] bytes) =>
        Assert(bytes, 1);

    public static byte[] AssertInts (byte[] bytes) =>
        Assert(bytes, 3);

    public static byte[] AssertLongs (byte[] bytes) =>
        Assert(bytes, 7);

    private static byte[] Assert (byte[] bytes, int mask) =>
        (bytes.Length & mask) == 0 ? bytes : throw new ArgumentException($"{bytes.Length} not divisible by {mask + 1}");

    unsafe public static void MemSet (byte[] bytes, byte b) {
        var l = bytes.Length;
        for (var i = 0; i < l; i++)
            bytes[i] = b;
    }
    unsafe public static void MemSet (byte[] bytes, ushort us) {
        if ((bytes.Length & 1) != 0)
            throw new ArgumentException($"array is {bytes.Length} bytes which is not whole divisible by {sizeof(ushort)}", nameof(bytes));
        var l = bytes.Length >> 1;
        fixed (byte* bp = bytes)
            for (var i = 0; i < l; i++)
                ((ushort*)bp)[i] = us;
    }

    public unsafe static void MemSet (byte[] bytes, uint u32) {
        if ((bytes.Length & 3) != 0)
            throw new ArgumentException($"array is {bytes.Length} bytes which is not whole divisible by {sizeof(uint)}", nameof(bytes));
        fixed (byte* bp = bytes) {
            uint* p = (uint*)bp;
            var count = bytes.Length >> 2;
            for (var i = 0; i < count; ++i)
                p[i] = u32;
        }
    }

    public static void Wipe (byte[] bytes) {
        var l = bytes.Length;
        if ((l & 7) == 0)
            WipeLongs(bytes);
        else if ((l & 3) == 0)
            WipeInts(bytes);
        else if ((l & 1) == 0)
            WipeShorts(bytes);
        else
            for (var i = 0; i < l; i++)
                bytes[i] = 0;
    }

    unsafe public static void WipeShorts (byte[] bytes) {
        if ((bytes.Length & 1) != 0)
            throw new ArgumentException($"array is {bytes.Length} bytes which is not whole divisible by {sizeof(ushort)}", nameof(bytes));
        var l = bytes.Length >> 1;
        fixed (byte* bp = bytes)
            for (var i = 0; i < l; i++)
                ((short*)bp)[i] = 0;
    }

    unsafe public static void WipeInts (byte[] bytes) {
        if ((bytes.Length & 3) != 0)
            throw new ArgumentException($"array is {bytes.Length} bytes which is not whole divisible by {sizeof(uint)}", nameof(bytes));
        var l = bytes.Length >> 2;
        fixed (byte* bp = bytes)
            for (var i = 0; i < l; i++)
                ((int*)bp)[i] = 0;
    }

    unsafe public static void WipeLongs (byte[] bytes) {
        if ((bytes.Length & 7) != 0)
            throw new ArgumentException($"array is {bytes.Length} bytes which is not whole divisible by {sizeof(long)}", nameof(bytes));
        var l = bytes.Length >> 4;
        fixed (byte* bp = bytes)
            for (var i = 0; i < l; i++)
                ((long*)bp)[i] = 0;
    }

    public static string VisualStudioLink (StackFrame sf) =>
        $">{sf.GetFileName()}({sf.GetFileLineNumber()},{sf.GetFileColumnNumber()}):";

    public static string Method (int skip = 0) =>
        VisualStudioLink(new StackFrame(skip + 1, true));

    public static void Trace (string message) {
        var formatted = TraceFormat(message, 1);
        if (Debugger.IsAttached)
            Debug.WriteLine(formatted);
        else
            Console.Error.WriteLine(formatted);
    }

    public static void CycleThrough<T> (ref T value, bool backwards = false) where T : struct, Enum {
        var values = Enum.GetValues<T>();
        var index = Array.IndexOf(values, value);

        if (backwards)
            index--;
        else
            index++;

        if (index >= values.Length)
            index = 0;
        else if (index < 0)
            index = values.Length - 1;

        value = values[index];
    }

    private static string TraceFormat (string message, int skip = 0) =>
        $"{Method(skip + 2)} {message}";

    public static FieldInfo GetBackingField (Type type, PropertyInfo prop, BindingFlags flags = BindingFlags.Instance) =>
        type.GetField($"<{prop.Name}>k__BackingField", BindingFlags.NonPublic | flags);

    public static bool TryGetBackingField (Type type, PropertyInfo prop, out FieldInfo eh, BindingFlags flags = BindingFlags.Instance) =>
        (eh = GetBackingField(type, prop, flags)) != null;

    private static readonly (byte major, byte minor, byte characters)[] ValidOpenglVersions = {
        (2, 0, 0x11),
        (2, 1, 0x12),
        (3, 0, 0x13),
        (3, 1, 0x14),
        (3, 2, 0x15),
        (3, 3, 0x33),
        (4, 0, 0x40),
        (4, 1, 0x41),
        (4, 2, 0x42),
        (4, 3, 0x43),
        (4, 4, 0x44),
        (4, 5, 0x45),
        (4, 6, 0x46),
    };

    unsafe public static int ShaderFromString (ShaderType type, string source) {
        var vs = CreateShader(type);
        var (version, profile) = GetCurrentContextVersion();
        var characters = Array.Find(ValidOpenglVersions, x => x.major == version.Major && x.minor == version.Minor).characters;
        if (0 == characters)
            throw new InvalidOperationException($"{version} not a known opengl version");
        var core = ProfileMask.Core == profile ? " core" : string.Empty;
        ShaderSource(vs, $"#version {characters:x}0{core}\n{source}");
        CompileShader(vs);
        var log = GetShaderInfoLog(vs);
        return 0 == log.Length ? vs : throw new ApplicationException(log);
    }

    unsafe public static int ProgramFromStrings (string vertexSource, string fragmentSource) {
        var vertexShader = ShaderFromString(ShaderType.Vertex, vertexSource);
        var fragmentShader = ShaderFromString(ShaderType.Fragment, fragmentSource);
        var program = CreateProgram();
        AttachShader(program, vertexShader);
        AttachShader(program, fragmentShader);
        LinkProgram(program);
        var log = GetProgramInfoLog(program);
        if (log.Length > 0)
            throw new ApplicationException(log);
        DeleteShader(vertexShader);
        DeleteShader(fragmentShader);
        return program;
    }

    unsafe public delegate void GetInfoLog (int i, int j, ref int k, byte* p);

    unsafe public static void ThrowThing (GetInfoLog f, int name, int length) {
        Span<byte> bytes = stackalloc byte[length + 1];
        fixed (byte* ptr = bytes)
            f(name, bytes.Length, ref length, ptr);
        throw new ApplicationException(System.Text.Encoding.ASCII.GetString(bytes.Slice(0, length)));
    }
}
