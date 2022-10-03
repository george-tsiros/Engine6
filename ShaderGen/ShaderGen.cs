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

                        Console.Write($"{sourceDir}{shaderName}(.vert,.frag) => ");
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

    private static bool IsPrimitive (UniformType type) =>
        type == UniformType.Double || type == UniformType.Float || type == UniformType.Int || type == UniformType.UInt;

    private static string UniformTypeToTypeName (UniformType type) {
        if (IsPrimitive(type))
            return type.ToString().ToLower();
        if (type == UniformType.Sampler2D)
            return "int";
        return $"in {type}";
    }

    private static bool IsKeyword (string term) =>
        term == "float" || term == "double" || term == "byte" || term == "char";

    private static readonly char[] SplitChars = "\r\n ".ToCharArray();

    // really horrible
    private static string Pack (string text) =>
        Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Join(' ', text.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries))));

    private const string ShaderHeader = @"
namespace Shaders;

using Gl;
using static Gl.GlContext;
using System.Numerics;
using Common;

public class {0}:Program {{
#pragma warning disable CS0649
";

    private const string UniformFormat = @"
    //size {0}, type {1}
    [GlUniform]
    private readonly int {2};
    public void {3} ({4} v) => Uniform({5}, v);
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
        f.Write("\";\n");

        var outCount = GetProgramInterfaceiv(program, ProgramInterface.ProgramOutput, InterfaceParameter.ActiveResources);
        //if (1 < outCount) {
        for (var i = 0; i < outCount; ++i) {
            using var name = GetProgramResourceName(program, i);
            using var propertyName = UppercaseFirst(name);
            using var lowercaseFirst = LowercaseFirst(propertyName);

            if (lowercaseFirst == name)
                f.Write("    [GlFragOut]\n");
            else
                f.Write("    [GlFragOut(\"{0}\")]\n", name);
            
            f.Write("    public int {0} {{ get; }}\n", propertyName);

            //var fragOut = GetFragDataLocation(program, name);
            //var locationIndex = GetProgramResourceLocationIndex(program, n);
            //if (-1 == locationIndex)
            //    throw new Exception($"GetProgramResourceLocationIndex({program}, \"{name}\") returned -1");
            //var location = GetProgramResourceLocation(program, ProgramInterface.ProgramOutput, n);
            //if (-1 == location)
            //    throw new Exception($"GetProgramResourceLocation({program}, ProgramInterface.ProgramOutput, \"{name}\") returned -1");
            //var index = GetProgramResourceIndex(program, ProgramInterface.ProgramOutput, n);
            //Console.Write($"{program} \"{name}\" => LocationIndex = {locationIndex}, Location = {location}, Index = {index}\r\n");
        }
        //}

        int attrCount = GetProgram(program, ProgramParameter.ActiveAttributes);
        for (var i = 0; i < attrCount; ++i) {
            var (size, type, name) = GetActiveAttrib(program, i);
            if (name.StartsWith("gl_"))
                continue;

            var propertyName = UppercaseFirst(name);
            f.Write("    //size {0}, type {1}\n", size, type);

            // if glsl attribute name is the same as the property name with first letter lowercase, we can find it at creation.
            if (LowercaseFirst(propertyName) == name)
                f.Write("    [GlAttrib]\n");
            else
                f.Write("    [GlAttrib(\"{0}\")]\n", name);

            f.Write("    public int {0} {{ get; }}", propertyName);
        }

        int uniformCount = GetProgram(program, ProgramParameter.ActiveUniforms);
        for (var i = 0; i < uniformCount; ++i) {
            var (size, type, name) = GetActiveUniform(program, i);
            if (name.StartsWith("gl_"))
                continue;

            // for uniforms, we can _always_ get the field name (even if it ends up starting with '@')
            var fieldName = IsKeyword(name) ? "@" + name : name;
            f.Write(UniformFormat, size, type, fieldName, UppercaseFirst(name), UniformTypeToTypeName(type), fieldName);
        }

        f.Write($"\n#pragma warning restore CS0649\n}}");

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
