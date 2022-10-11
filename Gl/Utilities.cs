namespace Gl;

using System;
using System.Diagnostics;
using System.Reflection;
using static GlContext;

public static class Utilities {

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

    private static string TraceFormat (string message, int skip = 0) =>
        $"{Method(skip + 2)} {message}";

    public static FieldInfo GetBackingField (Type type, PropertyInfo prop, BindingFlags flags = BindingFlags.Instance) =>
        type.GetField($"<{prop.Name}>k__BackingField", BindingFlags.NonPublic | flags);

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

    public unsafe static int ShaderFromString (ShaderType type, string source) {
        var shader = CreateShader(type);
        var (version, profile) = GetCurrentContextVersion();
        var characters = Array.Find(ValidOpenglVersions, x => x.major == version.Major && x.minor == version.Minor).characters;
        if (0 == characters)
            throw new InvalidOperationException($"{version} not a known opengl version");
        var core = ProfileMask.Core == profile ? " core" : string.Empty;
        ShaderSource(shader, $"#version {characters:x}0{core}\n{source}");
        CompileShader(shader);
        return 0 != GetShader(shader, ShaderParameter.CompileStatus) ? shader : throw new ApplicationException(GetShaderInfoLog(shader));
    }

    public unsafe static int ProgramFromStrings (string vertexSource, string fragmentSource) {
        var vertexShader = ShaderFromString(ShaderType.Vertex, vertexSource);
        var fragmentShader = ShaderFromString(ShaderType.Fragment, fragmentSource);
        var program = CreateProgram();
        AttachShader(program, vertexShader);
        AttachShader(program, fragmentShader);
        LinkProgram(program);
        DeleteShader(vertexShader);
        DeleteShader(fragmentShader);
        return 0 != GetProgram(program, ProgramParameter.LinkStatus) ? program : throw new ApplicationException(GetProgramInfoLog(program));
    }
}
