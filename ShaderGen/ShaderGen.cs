namespace ShaderGen;

using System;
using System.IO;
using System.Runtime.InteropServices;
using Gl;
using static Gl.GlContext;
using static Gl.Utilities;
using System.Text;

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

            using GlWindow window = new();

            foreach (var vertexShaderFilepath in Directory.EnumerateFiles(sourceDir, "*.vert")) {
                var vertexShaderFilename = Path.GetFileName(vertexShaderFilepath);
                var shaderName = Path.GetFileNameWithoutExtension(vertexShaderFilename);
                var fragmentShaderFilepath = Path.Combine(sourceDir, shaderName + ".frag");

                if (File.Exists(fragmentShaderFilepath)) {

                    var shaderNameUppercased = UppercaseFirst(shaderName);
                    var outputFilepath = Path.Combine(targetDir, shaderName + ".cs");
                    Console.Write($"{sourceDir}{shaderName}(.vert,.frag) => {targetDir}{shaderNameUppercased}.cs\n");
                    using StreamWriter f = new(outputFilepath, false, Encoding.ASCII);
                    DoProgram(vertexShaderFilepath, fragmentShaderFilepath, shaderNameUppercased, f);
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

    private static string UppercaseFirst (string str) {
        var chars = str.ToCharArray();
        chars[0] = char.ToUpper(chars[0]);
        return new(chars);
    }

    private static bool IsPrimitive (UniformType type) =>
        type == UniformType.Double || type == UniformType.Float || type == UniformType.Int || type == UniformType.UInt;

    private static string UniformTypeToTypeName (UniformType type) {
        if (IsPrimitive(type))
            return type.ToString().ToLower();
        if (type == UniformType.Sampler2D)
            return "int";
        return type.ToString();
    }

    private static bool IsKeyword (string term) =>
        term == "float" || term == "double" || term == "byte" || term == "char";

    private static readonly char[] SplitChars = "\r\n ".ToCharArray();

    // really horrible
    private static string Pack (string text) =>
        Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Join(' ', text.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries))));

    private static void DoProgram (string vertexShaderFilepath, string fragmentShaderFilepath, string className, StreamWriter f) {
        var vertexShaderSource = File.ReadAllText(vertexShaderFilepath);
        var fragmentShaderSource = File.ReadAllText(fragmentShaderFilepath);
        var program = ProgramFromStrings(vertexShaderSource, fragmentShaderSource);

        f.Write($"namespace Shaders;\n\nusing Gl;\nusing static Gl.GlContext;\nusing System.Numerics;\nusing Common;\n\npublic class {className}:Program {{\n#pragma warning disable CS0649\n");
        f.Write("    protected override string VertexSource { get; } = \"");
        f.Write(Pack(vertexShaderSource));
        f.Write("\";\n");

        f.Write("    protected override string FragmentSource { get; } = \"");
        f.Write(Pack(fragmentShaderSource));
        f.Write("\";\n");

        int attrCount = GetProgram(program, ProgramParameter.ActiveAttributes);
        for (var i = 0; i < attrCount; ++i) {
            var (size, type, name) = GetActiveAttrib(program, i);
            if (name.StartsWith("gl_"))
                continue;
            f.Write($"\n    //size {size}, type {type}\n    [GlAttrib(\"{name}\")]\n    public int {UppercaseFirst(name)} {{ get; }}\n");
        }

        int uniformCount = GetProgram(program, ProgramParameter.ActiveUniforms);
        for (var i = 0; i < uniformCount; ++i) {
            var (size, type, name) = GetActiveUniform(program, i);
            if (name.StartsWith("gl_"))
                continue;
            var fieldName = IsKeyword(name) ? "@" + name : name;
            var rawTypeName = type.ToString();
            f.Write($"\n    //size {size}, type {type}\n    [GlUniform(\"{name}\")]\n    private readonly int {fieldName};\n    public void {UppercaseFirst(name)} ({UniformTypeToTypeName(type)} v) => Uniform({fieldName}, v);\n");
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
