namespace Gl;

using System;
using System.Diagnostics;
using System.Reflection;

public static class Utilities {
    public static string Method (int skip = 0) => new StackFrame(skip + 1, true).GetMethod().Name;
    public static void Trace (string message) {
        var formatted = TraceFormat(message);
        if (Debugger.IsAttached)
            Debug.WriteLine(formatted);
        else
            Console.WriteLine(formatted);
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

    private static string TraceFormat (string message) => $"{DateTime.Now:mm:ss.fff}> {Method(2)} {message}";

    public static FieldInfo GetBackingField (Type type, PropertyInfo prop, BindingFlags flags = BindingFlags.Instance) => type.GetField($"<{prop.Name}>k__BackingField", BindingFlags.NonPublic | flags);

    public static bool TryGetBackingField (Type type, PropertyInfo prop, out FieldInfo eh, BindingFlags flags = BindingFlags.Instance) => (eh = GetBackingField(type, prop, flags)) != null;
    unsafe public static int ShaderFromString (ShaderType type, string source) {
        var vs = Opengl.CreateShader(type);
        Opengl.ShaderSource(vs, source);
        Opengl.CompileShader(vs);
        var log = Opengl.GetShaderInfoLog(vs);
        if (log.Length > 0)
            throw new ApplicationException(log);
        return vs;
    }
    unsafe public static int ProgramFromStrings (string vertexSource, string fragmentSource) {
        var vertexShader = ShaderFromString(ShaderType.Vertex, vertexSource);
        var fragmentShader = ShaderFromString(ShaderType.Fragment, fragmentSource);
        var program = Opengl.CreateProgram();
        Opengl.AttachShader(program, vertexShader);
        Opengl.AttachShader(program, fragmentShader);
        Opengl.LinkProgram(program);
        var log = Opengl.GetProgramInfoLog(program);
        if (log.Length > 0)
            throw new ApplicationException(log);
        Opengl.DeleteShader(vertexShader);
        Opengl.DeleteShader(fragmentShader);
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
