namespace Gl;

using System;
using System.Text;
using System.Reflection;
using System.Numerics;
using System.Diagnostics;
using System.Runtime.InteropServices;

public delegate void DebugProc (int sourceEnum, int typeEnum, int id, int severityEnum, int length, IntPtr message, IntPtr userParam);
unsafe public static class Opengl {
    private const string opengl32 = nameof(opengl32) + ".dll";
    internal enum StringName {
        Vendor = 0x1F00,
        Renderer = 0x1F01,
        Version = 0x1F02,
        Extensions = 0x1F03,
    }
    [DllImport(opengl32, EntryPoint = "wglDeleteContext", SetLastError = true)]
    public extern static bool DeleteContext (IntPtr hglrc);
    [DllImport(opengl32, EntryPoint = "wglCreateContext", SetLastError = true)]
    public static extern IntPtr CreateContext (IntPtr dc);
    [DllImport(opengl32, EntryPoint = "wglGetCurrentContext", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern IntPtr GetCurrentContext ();
    [DllImport(opengl32, EntryPoint = "wglGetProcAddress", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    public static extern IntPtr GetProcAddress (string name);
    [DllImport(opengl32, SetLastError = true, EntryPoint = "wglMakeCurrent")]
    public static extern bool MakeCurrent (IntPtr dc, IntPtr hglrc);
    [DllImport(opengl32, EntryPoint = "glClear")]
    public static extern void Clear (BufferBit mask);
    [DllImport(opengl32, EntryPoint = "glClearColor")]
    public static extern void ClearColor (float r, float g, float b, float a);
    [DllImport(opengl32, EntryPoint = "glIsEnabled")]
    public static extern bool IsEnabled (Capability cap);
    [DllImport(opengl32, EntryPoint = "glEnable")]
    public static extern void Enable (Capability cap);
    [DllImport(opengl32, EntryPoint = "glDrawArrays")]
    public static extern void DrawArrays (Primitive mode, int first, int count);
    [DllImport(opengl32, EntryPoint = "glDepthMask")]
    public static extern void DepthMask (bool enable);
    [DllImport(opengl32, EntryPoint = "glDisable")]
    public static extern void Disable (Capability cap);
    [DllImport(opengl32, EntryPoint = "glBindTexture")]
    public static extern void BindTexture (int target, int texture);
    [DllImport(opengl32, EntryPoint = "glViewport")]
    public static extern void Viewport (int x, int y, int w, int h);
    [DllImport(opengl32, EntryPoint = "glBlendFunc")]
    public static extern void BlendFunc (BlendSourceFactor sfactor, BlendDestinationFactor dfactor);
    [DllImport(opengl32, EntryPoint = "glDepthFunc")]
    public static extern void DepthFunc (DepthFunction f);
    [DllImport(opengl32, EntryPoint = "glFlush")]
    public static extern void Flush ();
    [DllImport(opengl32, EntryPoint = "glFinish")]
    public static extern void Finish ();
    [DllImport(opengl32, EntryPoint = "glDeleteTextures")]
    unsafe public static extern void DeleteTextures (int count, int* ints);
    [DllImport(opengl32, EntryPoint = "glGetIntegerv")]
    unsafe public static extern void GetIntegerv (int count, int* ints);
    private static class Extensions {
#pragma warning disable CS0649
        public static readonly delegate* unmanaged[Cdecl]<int, void> glActiveTexture;
        //public static readonly delegate* unmanaged[Cdecl]<Capability, bool> glIsEnabled;
        public static readonly delegate* unmanaged[Cdecl]<int, byte*, int> glGetAttribLocation;
        public static readonly delegate* unmanaged[Cdecl]<int, byte*, int> glGetUniformLocation;
        public static readonly delegate* unmanaged[Cdecl]<int> glCreateProgram;
        public static readonly delegate* unmanaged[Cdecl]<ShaderType, int> glCreateShader;
        public static readonly delegate* unmanaged[Cdecl]<int, int, void> glAttachShader;
        public static readonly delegate* unmanaged[Cdecl]<BufferTarget, int, void> glBindBuffer;
        public static readonly delegate* unmanaged[Cdecl]<int, int, void> glBindFramebuffer;
        //public static readonly delegate* unmanaged[Cdecl]<int, int, void> glBindTexture;
        public static readonly delegate* unmanaged[Cdecl]<int, void> glBindVertexArray;
        //public static readonly delegate* unmanaged[Cdecl]<BufferBit, void> glClear;
        //public static readonly delegate* unmanaged[Cdecl]<float, float, float, float, void> glClearColor;
        public static readonly delegate* unmanaged[Cdecl]<int, void> glCompileShader;
        public static readonly delegate* unmanaged[Cdecl]<int, int*, void> glCreateBuffers;
        public static readonly delegate* unmanaged[Cdecl]<int, int*, void> glCreateVertexArrays;
        public static readonly delegate* unmanaged[Cdecl]<int, int*, void> glDeleteBuffers;
        public static readonly delegate* unmanaged[Cdecl]<int, int*, void> glDeleteVertexArrays;
        public static readonly delegate* unmanaged[Cdecl]<int, int, int*, void> glCreateTextures;
        public static readonly delegate* unmanaged[Cdecl]<DebugProc, IntPtr, void> glDebugMessageCallback;
        public static readonly delegate* unmanaged[Cdecl]<int, void> glDeleteShader;
        public static readonly delegate* unmanaged[Cdecl]<int, void> glDeleteProgram;
        //public static readonly delegate* unmanaged[Cdecl]<DepthFunction, void> glDepthFunc;
        //public static readonly delegate* unmanaged[Cdecl]<Primitive, int, int, void> glDrawArrays;
        public static readonly delegate* unmanaged[Cdecl]<Primitive, int, int, int, void> glDrawArraysInstanced;
        //public static readonly delegate* unmanaged[Cdecl]<Capability, void> glEnable;
        //public static readonly delegate* unmanaged[Cdecl]<Capability, void> glDisable;
        public static readonly delegate* unmanaged[Cdecl]<int, int, void> glEnableVertexArrayAttrib;
        //public static readonly delegate* unmanaged[Cdecl]<bool, void> glDepthMask;
        public static readonly delegate* unmanaged[Cdecl]<int, ProgramParameter, int*, void> glGetProgramiv;
        public static readonly delegate* unmanaged[Cdecl]<int, ShaderParameter, int*, void> glGetShaderiv;
        public static readonly delegate* unmanaged[Cdecl]<int, int, int*, byte*, void> glGetProgramInfoLog;
        public static readonly delegate* unmanaged[Cdecl]<int, int, int*, byte*, void> glGetShaderInfoLog;
        public static readonly delegate* unmanaged[Cdecl]<int, int, int, int*, int*, UniformType*, byte*, void> glGetActiveUniform;
        public static readonly delegate* unmanaged[Cdecl]<int, int, int, int*, int*, AttribType*, byte*, void> glGetActiveAttrib;
        //public static readonly delegate* unmanaged[Cdecl]<int, int*, void> glGetIntegerv;
        public static readonly delegate* unmanaged[Cdecl]<int, void> glLinkProgram;
        public static readonly delegate* unmanaged[Cdecl]<int, long, void*, int, void> glNamedBufferStorage;
        public static readonly delegate* unmanaged[Cdecl]<int, long, long, void*, void> glNamedBufferSubData;
        public static readonly delegate* unmanaged[Cdecl]<int, int, byte**, int*, void> glShaderSource;
        public static readonly delegate* unmanaged[Cdecl]<int, int, int, void> glTextureParameteri;
        public static readonly delegate* unmanaged[Cdecl]<int, int, TextureInternalFormat, int, int, void> glTextureStorage2D;
        public static readonly delegate* unmanaged[Cdecl]<int, int, int, int, int, int, int, int, void*, void> glTextureSubImage2D;
        public static readonly delegate* unmanaged[Cdecl]<int, int, void> glUniform1i;
        public static readonly delegate* unmanaged[Cdecl]<int, float, void> glUniform1f;
        public static readonly delegate* unmanaged[Cdecl]<int, float, float, void> glUniform2f;
        public static readonly delegate* unmanaged[Cdecl]<int, float, float, float, float, void> glUniform4f;
        public static readonly delegate* unmanaged[Cdecl]<int, long, bool, Matrix4x4, void> glUniformMatrix4fv;
        public static readonly delegate* unmanaged[Cdecl]<int, void> glUseProgram;
        public static readonly delegate* unmanaged[Cdecl]<int, int, void> glVertexAttribDivisor;
        public static readonly delegate* unmanaged[Cdecl]<int, int, AttribType, bool, int, long, void> glVertexAttribPointer;
        //public static readonly delegate* unmanaged[Cdecl]<int, int, int, int, void> glViewport;
        //public static readonly delegate* unmanaged[Cdecl]<void> glFlush;
        //public static readonly delegate* unmanaged[Cdecl]<void> glFinish;
        //public static readonly delegate* unmanaged[Cdecl]<int, int, void> glBlendFunc;
        static Extensions () {
            //Debugger.Break();
            foreach (var f in typeof(Extensions).GetFields(BindingFlags.Public | BindingFlags.Static))
                if (f.Name.StartsWith("gl") && f.IsInitOnly && (IntPtr)f.GetValue(null) == IntPtr.Zero)
                    f.SetValue(null, GetProcAddress(f.Name));
        }

        public static IntPtr openglLibrary;

        public static IntPtr GetProcAddress (string name) {
            var ptr = Opengl.GetProcAddress(name);
            if (IntPtr.Zero == ptr) {
                if (IntPtr.Zero == openglLibrary)
                    openglLibrary = Kernel.LoadLibraryW("opengl32.dll");
                ptr = Kernel.GetProcAddress(openglLibrary, name);
                Debug.WriteLineIf(ptr != IntPtr.Zero, $"{name} found in via GetProcAddress");
            } else
                Debug.WriteLine($"{name} found via wglGetProcAddress");
            Debug.WriteLineIf(ptr == IntPtr.Zero, $"{name} found nowhere");
            //return ptr;
            return ptr != IntPtr.Zero ? ptr : throw new ApplicationException($"failed to get proc address of {name}");
        }
#pragma warning restore CS0649
    }

    public static void ActiveTexture (int i) => Extensions.glActiveTexture(i);
    public static void AttachShader (int p, int s) => Extensions.glAttachShader(p, s);
    public static void BindBuffer (BufferTarget target, int buffer) => Extensions.glBindBuffer(target, buffer);
    public static void BindFramebuffer (int target, int buffer) => Extensions.glBindFramebuffer((int)target, buffer);
    public static void BindVertexArray (int vao) => Extensions.glBindVertexArray(vao);
    public static void CompileShader (int s) => Extensions.glCompileShader(s);
    public static int CreateProgram () => Extensions.glCreateProgram();
    public static int CreateShader (ShaderType shaderType) => Extensions.glCreateShader(shaderType);
    public static void DebugMessageCallback (DebugProc proc, IntPtr userParam) => Extensions.glDebugMessageCallback(proc, userParam);
    public static void DeleteBuffer (int i) => Extensions.glDeleteBuffers(1, &i);
    public static void DeleteProgram (int program) => Extensions.glDeleteProgram(program);
    public static void DeleteShader (int shader) => Extensions.glDeleteShader(shader);
    public static void DeleteTexture (int texture) => DeleteTextures(1, &texture);
    public static void DeleteVertexArray (int vao) => Extensions.glDeleteVertexArrays(1, &vao);
    public static void DrawArraysInstanced (Primitive mode, int firstIndex, int indicesPerInstance, int instancesCount) => Extensions.glDrawArraysInstanced(mode, firstIndex, indicesPerInstance, instancesCount);
    public static void EnableVertexArrayAttrib (int id, int i) => Extensions.glEnableVertexArrayAttrib(id, i);
    public static void LinkProgram (int p) => Extensions.glLinkProgram(p);
    public static void NamedBufferStorage (int buffer, long size, IntPtr data, int flags) => Extensions.glNamedBufferStorage(buffer, size, data.ToPointer(), flags);
    public static void NamedBufferSubData (int buffer, long offset, long size, void* data) => Extensions.glNamedBufferSubData(buffer, offset, size, data);
    public static void TextureBaseLevel (int texture, int level) => Extensions.glTextureParameteri(texture, Const.TEXTURE_BASE_LEVEL, level);
    public static void TextureFilter (int texture, MagFilter filter) => Extensions.glTextureParameteri(texture, Const.TEXTURE_MAG_FILTER, (int)filter);
    public static void TextureFilter (int texture, MinFilter filter) => Extensions.glTextureParameteri(texture, Const.TEXTURE_MIN_FILTER, (int)filter);
    public static void TextureMaxLevel (int texture, int level) => Extensions.glTextureParameteri(texture, Const.TEXTURE_MAX_LEVEL, level);
    public static void TextureStorage2D (int texture, int levels, TextureInternalFormat sizedFormat, int width, int height) => Extensions.glTextureStorage2D(texture, levels, sizedFormat, width, height);
    public static void TextureSubImage2D (int texture, int level, int xOffset, int yOffset, int width, int height, TextureFormat format, int type, void* pixels) => Extensions.glTextureSubImage2D(texture, level, xOffset, yOffset, width, height, (int)format, type, pixels);
    public static void TextureWrap (int texture, WrapCoordinate c, Wrap w) => Extensions.glTextureParameteri(texture, (int)c, (int)w);
    public static void Uniform (int uniform, float f) => Extensions.glUniform1f(uniform, f);
    public static void Uniform (int uniform, int i) => Extensions.glUniform1i(uniform, i);
    public static void Uniform (int uniform, Matrix4x4 m) => Extensions.glUniformMatrix4fv(uniform, 1, false, m);
    public static void Uniform (int uniform, Vector2 v) => Extensions.glUniform2f(uniform, v.X, v.Y);
    public static void Uniform (int uniform, Vector4 v) => Extensions.glUniform4f(uniform, v.X, v.Y, v.Z, v.W);
    public static void UseProgram (int p) => Extensions.glUseProgram(p);
    public static void VertexAttribDivisor (int index, int divisor) => Extensions.glVertexAttribDivisor(index, divisor);
    public static void VertexAttribPointer (int index, int size, AttribType type, bool normalized, int stride, long ptr) => Extensions.glVertexAttribPointer(index, size, type, normalized, stride, ptr);
    public static void Viewport (Vector2i position, Vector2i size) => Viewport(position.X, position.Y, size.X, size.Y);

    public static int GetAttribLocation (int program, string name) => GetLocation(program, name, Extensions.glGetAttribLocation);
    public static int GetUniformLocation (int program, string name) => GetLocation(program, name, Extensions.glGetUniformLocation);

    private static int GetLocation (int program, string name, delegate* unmanaged[Cdecl]<int, byte*, int> f) {
        Span<byte> bytes = stackalloc byte[name.Length + 1];
        var l = Encoding.ASCII.GetBytes(name, bytes);
        Debug.Assert(l == name.Length);
        bytes[name.Length] = 0;
        fixed (byte* p = bytes)
            return f(program, p);
    }

    public static int CreateBuffer () => Create(Extensions.glCreateBuffers);
    public static int CreateVertexArray () => Create(Extensions.glCreateVertexArrays);

    private static int Create (delegate* unmanaged[Cdecl]<int, int*, void> f) {
        int i;
        f(1, &i);
        return i;
    }

    public static void ShaderSource (int id, string source) {
        var bytes = new byte[source.Length + 1];
        var l = Encoding.ASCII.GetBytes(source, bytes);
        Debug.Assert(source.Length == l);
        bytes[source.Length] = 0;
        fixed (byte* strPtr = bytes)
            Extensions.glShaderSource(id, 1, &strPtr, null);
    }

    public static int GetProgram (int id, ProgramParameter p) { // I wish I could join this with GetShader
        int i;
        Extensions.glGetProgramiv(id, p, &i);
        return i;
    }

    public static int GetShader (int id, ShaderParameter p) {
        int i;
        Extensions.glGetShaderiv(id, p, &i);
        return i;
    }

    public static (int size, AttribType type, string name) GetActiveAttrib (int id, int index) {
        var maxLength = GetProgram(id, ProgramParameter.ActiveAttributeMaxLength);
        int length, size;
        AttribType type;
        Span<byte> bytes = stackalloc byte[maxLength];
        fixed (byte* p = bytes)
            Extensions.glGetActiveAttrib(id, index, maxLength, &length, &size, &type, p);
        var n = length > 0 ? Encoding.ASCII.GetString(bytes.Slice(0, length)) : "";
        return (size, type, n);
    }

    public static (int size, UniformType type, string name) GetActiveUniform (int id, int index) {
        var maxLength = GetProgram(id, ProgramParameter.ActiveUniformMaxLength);
        int length, size;
        UniformType type;
        Span<byte> bytes = stackalloc byte[maxLength];
        fixed (byte* p = bytes)
            Extensions.glGetActiveUniform(id, index, maxLength, &length, &size, &type, p);
        var n = length > 0 ? Encoding.ASCII.GetString(bytes.Slice(0, length)) : "";
        return (size, type, n);
    }

    public static string GetProgramInfoLog (int id) {
        int actualLogLength = GetProgram(id, ProgramParameter.InfoLogLength);
        var bufferLength = Math.Min(1024, actualLogLength);
        Span<byte> bytes = stackalloc byte[bufferLength];
        fixed (byte* p = bytes)
            Extensions.glGetProgramInfoLog(id, bufferLength, null, p);
        return Encoding.ASCII.GetString(bytes);
    }

    public static string GetShaderInfoLog (int id) {
        int actualLogLength = GetShader(id, ShaderParameter.InfoLogLength);
        var bufferLength = Math.Min(1024, actualLogLength);
        Span<byte> bytes = stackalloc byte[bufferLength];
        fixed (byte* p = bytes)
            Extensions.glGetShaderInfoLog(id, bufferLength, null, p);
        return Encoding.ASCII.GetString(bytes);
    }

    unsafe public static int GetIntegerv (IntParameter p) {
        int i;
        GetIntegerv((int)p, &i);
        return i;
    }

    public static int CreateTexture2D () {
        int i;
        Extensions.glCreateTextures(Const.TEXTURE_2D, 1, &i);
        return i;
    }
}