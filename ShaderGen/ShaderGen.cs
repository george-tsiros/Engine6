namespace ShaderGen;

using System;
using System.IO;
using System.Runtime.InteropServices;
using Gl;
using static Gl.Opengl;
using static Gl.Utilities;
using Win32;
using System.Text;

class ShaderGen {
    private static bool CreateFrom (string[] args) {
        const int expectedArgumentCount = 2;
        if (args.Length != expectedArgumentCount)
            throw new ArgumentException($"expected {expectedArgumentCount}, not {args.Length} arguments", nameof(args));
        var missing = Array.FindIndex(args, f => !Directory.Exists(f));
        if (missing >= 0)
            throw new DirectoryNotFoundException(args[missing]);


        try {
            using var window = new GlWindow();
            foreach (var vertexShaderFilepath in Directory.EnumerateFiles(args[0], "*.vert")) {
                var shaderName = UppercaseFirst(Path.GetFileNameWithoutExtension(vertexShaderFilepath));
                Trace($"creating {shaderName}");
                var fragmentShaderFilepath = Path.Combine(Path.GetDirectoryName(vertexShaderFilepath), shaderName + ".frag");
                if (File.Exists(fragmentShaderFilepath))
                    using (var f = new StreamWriter(Path.Combine(args[1], shaderName + ".cs")) { })
                        DoProgram(vertexShaderFilepath, fragmentShaderFilepath, shaderName, f);

            }
        } catch (TypeInitializationException ex) {
            Trace($"'{ex.Message}' for '{ex.TypeName}'");
            if (ex.InnerException is MarshalDirectiveException inner)
                Trace($"({inner.Message})");
            return false;
        } catch (GlException e) {
            Trace(e.ToString());
            return false;
        }
        return true;
    }
    private static string UppercaseFirst (string str) {
        var chars = str.ToCharArray();
        chars[0] = char.ToUpper(chars[0]);
        return new string(chars);
    }
    private static bool IsPrimitive (UniformType type) => type == UniformType.Double || type == UniformType.Float || type == UniformType.Int || type == UniformType.UInt;

    private static string UniformTypeToTypeName (UniformType type) {
        if (IsPrimitive(type))
            return type.ToString().ToLower();
        if (type == UniformType.Sampler2D)
            return "int";
        return type.ToString();
    }

    private static bool IsPrimitive (string name) => name == "float" || name == "double" || name == "byte" || name == "char";
    private static readonly char[] SplitChars = { '\n', '\r', ' ' };

    // really horrible
    private static string Pack (string text) => 
        Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Join(' ', text.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries))));

    private static void DoProgram (string vertexShaderFilepath, string fragmentShaderFilepath, string className, StreamWriter f) {
        var vertexShaderSource = File.ReadAllText(vertexShaderFilepath);
        var fragmentShaderSource = File.ReadAllText(fragmentShaderFilepath);
        var program = ProgramFromStrings(vertexShaderSource, fragmentShaderSource);

        f.Write($@"namespace Shaders;
using Gl;
using static Gl.Opengl;
using System.Numerics;
using Linear;
public static class {className} {{
#pragma warning disable CS0649
");
        f.Write("    public const string VertexSource = \"");
        f.Write(Pack(vertexShaderSource));
        f.Write("\";\n");

        f.Write("    public const string FragmentSource = \"");
        f.Write(Pack(fragmentShaderSource));
        f.Write("\";\n");

        int attrCount = GetProgram(program, ProgramParameter.ActiveAttributes);
        for (var i = 0; i < attrCount; ++i) {
            var x = GetActiveAttrib(program, i);
            if (x.name.StartsWith("gl_"))
                continue;
            f.Write($@"
    //size {x.size}, type {x.type}
    [GlAttrib(""{x.name}"")]
    public static int {UppercaseFirst(x.name)} {{ get; }}
");
        }

        int uniformCount = GetProgram(program, ProgramParameter.ActiveUniforms);
        for (var i = 0; i < uniformCount; ++i) {
            var y = GetActiveUniform(program, i);
            if (y.name.StartsWith("gl_"))
                continue;
            var fieldName = IsPrimitive(y.name) ? "@" + y.name : y.name;
            var rawTypeName = y.type.ToString();
            f.Write($@"
    //size {y.size}, type {y.type}
    [GlUniform(""{y.name}"")]
    private readonly static int {fieldName};
    public static void {UppercaseFirst(y.name)} ({UniformTypeToTypeName(y.type)} v) => Uniform({fieldName}, v);
");
        }

        f.Write($@"
    public static int Id {{ get; }}
    static {className} () => ParsedShader.Prepare(typeof({className}));
#pragma warning restore CS0649
}}");

        DeleteProgram(program);
    }
    private static readonly string dashes = new('-', 100);
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
