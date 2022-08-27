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
            Console.WriteLine(VersionString);
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
        } catch (Exception e) {
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
    private static bool IsPrimitive (UniformType type) => 
        type == UniformType.Double || type == UniformType.Float || type == UniformType.Int || type == UniformType.UInt;

    private static string UniformTypeToTypeName (UniformType type) {
        if (IsPrimitive(type))
            return type.ToString().ToLower();
        if (type == UniformType.Sampler2D)
            return "int";
        return type.ToString();
    }

    private static bool IsPrimitive (string name) => 
        name == "float" || name == "double" || name == "byte" || name == "char";

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
using Common;

public class {className}:Program {{
#pragma warning disable CS0649
");
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
            f.Write($@"
    //size {size}, type {type}
    [GlAttrib(""{name}"")]
    public int {UppercaseFirst(name)} {{ get; }}
");
        }

        int uniformCount = GetProgram(program, ProgramParameter.ActiveUniforms);
        for (var i = 0; i < uniformCount; ++i) {
            var (size, type, name) = GetActiveUniform(program, i);
            if (name.StartsWith("gl_"))
                continue;
            var fieldName = IsPrimitive(name) ? "@" + name : name;
            var rawTypeName = type.ToString();
            f.Write($@"
    //size {size}, type {type}
    [GlUniform(""{name}"")]
    private readonly int {fieldName};
    public void {UppercaseFirst(name)} ({UniformTypeToTypeName(type)} v) => Uniform({fieldName}, v);
");
        }

        f.Write($@"
#pragma warning restore CS0649
}}");

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
