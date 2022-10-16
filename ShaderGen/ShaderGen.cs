namespace ShaderGen;

using System;
using System.IO;
using System.Runtime.InteropServices;
using Gl;
using Common;
using static Gl.GlContext;
using static Gl.Utilities;
using static Common.Functions;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

class ShaderGen {

    private static bool CreateFrom (string[] args) {

        const int expectedArgumentCount = 2;

        if (args.Length != expectedArgumentCount)
            throw new ArgumentException($"expected {expectedArgumentCount}, not {args.Length} arguments", nameof(args));

        var (sourceDir, targetDir) = (args[0], args[1]);

        if (!Directory.Exists(sourceDir))
            throw new DirectoryNotFoundException(sourceDir);

        if (!Directory.Exists(targetDir))
            throw new DirectoryNotFoundException(targetDir);

        try {

            using Win32.Window window = new();
            using GlContext ctx = new(window.Dc);
            foreach (var vertexShaderFilepath in Directory.EnumerateFiles(sourceDir, "*.vert")) {
                var vertexShaderFilename = Path.GetFileName(vertexShaderFilepath);
                var shaderName = Path.GetFileNameWithoutExtension(vertexShaderFilename);
                var fragmentShaderFilepath = Path.Combine(sourceDir, shaderName + ".frag");

                if (File.Exists(fragmentShaderFilepath)) {

                    var shaderNameUppercased = UppercaseFirst(shaderName);
                    using (MemoryStream mem = new()) {
                        using (StreamWriter f = new(mem, Encoding.ASCII, -1, true))
                            DoProgram(vertexShaderFilepath, fragmentShaderFilepath, shaderNameUppercased, f);


                        var outputFilepath = Path.Combine(targetDir, shaderNameUppercased + ".cs");
                        if (File.Exists(outputFilepath) && 0 != new FileInfo(outputFilepath).Length) {
                            var existing = File.ReadAllText(outputFilepath).Trim();
                            var newlyCreated = Encoding.ASCII.GetString(mem.ToArray()).Trim();
                            if (existing == newlyCreated) {
                                Console.Write($"{outputFilepath} already exists and is the same\n");
                                continue;
                            }
                        }

                        Console.Write($"{sourceDir}\\{shaderName}(.vert,.frag) => ");
                        mem.Position = 0;
                        using (var f = File.Create(outputFilepath))
                            mem.CopyTo(f);
                        Console.Write($"{outputFilepath}\n");
                    }
                } else {
                    Trace($"no fragment shader file (\"{fragmentShaderFilepath}\") for vertex shader file \"{vertexShaderFilename}\"");
                }
            }
        } catch (TypeInitializationException ex) {
            Trace($"'{ex.Message}' for '{ex.TypeName}'");
            if (ex.InnerException is MarshalDirectiveException inner)
                Trace($"({inner.Message})");
            return false;
        } catch (Exception e) {
            Trace(e.ToString());
            return false;
        }
        return true;
    }

    private static readonly UniformType[] PrimitiveUniformTypes = { UniformType.Double, UniformType.Float, UniformType.Int, UniformType.UInt, };
    private static readonly AttribType[] PrimitiveAttribTypes = { AttribType.SByte, AttribType.Byte, AttribType.Short, AttribType.UShort, AttribType.Int, AttribType.UInt, AttribType.Float, AttribType.Double, };

    private static bool IsPrimitive (AttribType type) =>
        0 <= Array.IndexOf(PrimitiveAttribTypes, type);

    private static bool IsPrimitive (UniformType type) =>
        0 <= Array.IndexOf(PrimitiveUniformTypes, type);

    private static string AttribTypeToTypeName (AttribType type) {
        var str = type.ToString();
        return IsPrimitive(type) ? str.ToLower() : str;
    }

    private static string UniformTypeToTypeName (UniformType type) {
        if (IsPrimitive(type))
            return type.ToString().ToLower();
        if (type == UniformType.Sampler2D)
            return "int";
        return $"in {type}";
    }

    private static readonly char[] SplitChars = { ' ', '\r', '\n' };

    // really horrible
    private static string Pack (string text) =>
        Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Join(' ', text.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries))));

    private const string ShaderHeader = @"namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class {0}:Program {{
#pragma warning disable CS0649

";

    private const string UniformFormat = @"    private readonly int {0};
    public void {1} ({2} v) => Uniform({3}, v);

";

    private static void DoProgram (string vertexShaderFilepath, string fragmentShaderFilepath, string className, TextWriter f) {
        List<string> vertexShaderSourceLines = new();
        List<string> vertexShaderCommentLines = new();
        foreach (var line in EnumLines(vertexShaderFilepath, EnumLinesOption.SkipBlankOrWhitespace | EnumLinesOption.Trim))
            (line.StartsWith("//") ? vertexShaderCommentLines : vertexShaderSourceLines).Add(line);

        List<string> fragmentShaderSourceLines = new();
        List<string> fragmentShaderCommentLines = new();
        foreach (var line in EnumLines(fragmentShaderFilepath, EnumLinesOption.SkipBlankOrWhitespace | EnumLinesOption.Trim))
            (line.StartsWith("//") ? fragmentShaderCommentLines : fragmentShaderSourceLines).Add(line);

        var vertexShaderSource = string.Join("\n", vertexShaderSourceLines);
        var fragmentShaderSource = string.Join("\n", fragmentShaderSourceLines);
        var program = ProgramFromStrings(vertexShaderSource, fragmentShaderSource);
        f.Write(ShaderHeader, className);
        f.Write("    protected override string VertexSource { get; } = \"");
        f.Write(Pack(vertexShaderSource));
        f.Write("\";\n");

        f.Write("    protected override string FragmentSource { get; } = \"");
        f.Write(Pack(fragmentShaderSource));
        f.Write("\";\n\n");

        Stack<string> ctorStatements = new();


        var outCount = GetProgramInterfaceiv(program, ProgramInterface.ProgramOutput, InterfaceParameter.ActiveResources);
        for (var i = 0; i < outCount; ++i) {
            var name = GetProgramResourceName(program, i);

            // needs improvement
            if (2 < name.Length && 'g' == name[0] && 'l' == name[1] && '_' == name[2])
                continue;

            var propertyName = UppercaseFirst(name);
            Debug.Assert(name != propertyName);
            f.Write("    public FragOut {0} {{ get; }}\n\n", propertyName);
            ctorStatements.Push(string.Format("{0} = GetFragDataLocation(this, \"{1}\");\n", propertyName, name));
        }

        int attrCount = GetProgram(program, ProgramParameter.ActiveAttributes);
        for (var i = 0; i < attrCount; ++i) {
            var (size, type, name) = GetActiveAttrib(program, i);
            if (name.StartsWith("gl_"))
                continue;

            var propertyName = UppercaseFirst(name);
            Debug.Assert(name != propertyName);
            f.Write("    public Attrib<{0}> {1} {{ get; }}\n\n", AttribTypeToTypeName(type), propertyName);
            ctorStatements.Push(string.Format("{0} = GetAttribLocation(this, \"{1}\");\n", propertyName, name));
        }

        int uniformCount = GetProgram(program, ProgramParameter.ActiveUniforms);
        for (var i = 0; i < uniformCount; ++i) {
            var (size, type, name) = GetActiveUniform(program, i);
            if (name.StartsWith("gl_"))
                continue;

            var fieldName = IsKeyword(name) ? "@" + name : name;
            f.Write(UniformFormat, fieldName, UppercaseFirst(name), UniformTypeToTypeName(type), fieldName);
            ctorStatements.Push(string.Format("{0} = GetUniformLocation(this, nameof({1}));\n", fieldName, name));
        }

        if (0 < ctorStatements.Count) {
            f.Write("    public {0} () {{\n", className);
            const string pad = "        ";
            while (ctorStatements.TryPop(out var statement)) {
                f.Write(pad);
                f.Write(statement);
            }
            f.Write("    }\n");
        }

        f.Write("\n#pragma warning restore CS0649\n}");

        DeleteProgram(program);
    }

    [STAThread]
    public static int Main (string[] args) {
        try {
            return CreateFrom(args) ? 0 : -1;
        } catch (Exception ex) {
            Trace(ex.Message);
            return -1;
        }
    }
}
