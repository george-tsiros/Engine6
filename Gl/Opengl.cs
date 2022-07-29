namespace Gl;

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Numerics;
using System.Runtime.InteropServices;
using Win32;
using System.IO;

public delegate void DebugProc (DebugSource sourceEnum, DebugType typeEnum, int id, DebugSeverity severityEnum, int length, IntPtr message, IntPtr userParam);

unsafe public static class Opengl {
    private const string opengl32 = nameof(opengl32) + ".dll";
    internal enum StringName {
        Vendor = 0x1F00,
        Renderer = 0x1F01,
        Version = 0x1F02,
        Extensions = 0x1F03,
    }

    [DllImport(opengl32, EntryPoint = "glGetError", ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
    public static extern int GetError ();
    [DllImport(opengl32, EntryPoint = "wglCreateContext", ExactSpelling = true, SetLastError = true)]
    public static extern IntPtr CreateContext (IntPtr dc);
    [DllImport(opengl32, EntryPoint = "wglGetProcAddress", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern IntPtr GetProcAddress ([MarshalAs(UnmanagedType.LPStr)] string name);
    [DllImport(opengl32, EntryPoint = "wglGetCurrentDC", ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
    public static extern IntPtr GetCurrentDC ();
    [DllImport(opengl32, EntryPoint = "wglGetCurrentContext", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern IntPtr GetCurrentContext ();
    [DllImport(opengl32, EntryPoint = "glGetFloatv", ExactSpelling = true)]
    public static extern void GetFloatv (int name, float* v);
    [DllImport(opengl32, EntryPoint = "glClear", ExactSpelling = true)]
    public static extern void Clear (BufferBit mask);
    [DllImport(opengl32, EntryPoint = "glClearColor", ExactSpelling = true)]
    public static extern void ClearColor (float r, float g, float b, float a);
    [DllImport(opengl32, ExactSpelling = true)]
    private static extern byte glIsEnabled (Capability cap);
    [DllImport(opengl32, EntryPoint = "glEnable", ExactSpelling = true)]
    public static extern void Enable (Capability cap);
    [DllImport(opengl32, EntryPoint = "glDrawArrays", ExactSpelling = true)]
    public static extern void DrawArrays (Primitive mode, int first, int count);
    [DllImport(opengl32, EntryPoint = "glDepthMask", ExactSpelling = true)]
    public static extern void DepthMask (bool enable);
    [DllImport(opengl32, EntryPoint = "glDisable", ExactSpelling = true)]
    public static extern void Disable (Capability cap);
    [DllImport(opengl32, EntryPoint = "glBindTexture", ExactSpelling = true)]
    public static extern void BindTexture (int target, int texture);
    [DllImport(opengl32, EntryPoint = "glViewport", ExactSpelling = true)]
    public static extern void Viewport (int x, int y, int w, int h);
    [DllImport(opengl32, EntryPoint = "glBlendFunc", ExactSpelling = true)]
    public static extern void BlendFunc (BlendSourceFactor sfactor, BlendDestinationFactor dfactor);
    [DllImport(opengl32, EntryPoint = "glDepthFunc", ExactSpelling = true)]
    public static extern void DepthFunc (DepthFunction f);
    [DllImport(opengl32, EntryPoint = "glFlush", ExactSpelling = true)]
    public static extern void Flush ();
    [DllImport(opengl32, EntryPoint = "glGetString", ExactSpelling = true)]
    public static extern IntPtr GetString (OpenglString name);
    [DllImport(opengl32, EntryPoint = "glFinish", ExactSpelling = true)]
    public static extern void Finish ();
    [DllImport(opengl32, EntryPoint = "glDeleteTextures", ExactSpelling = true)]
    unsafe static extern void DeleteTextures (int count, int* ints);
    [DllImport(opengl32, EntryPoint = "glScissor", ExactSpelling = true)]
    public static extern void Scissor (int x, int y, int width, int height);
    [DllImport(opengl32, EntryPoint = "glGetIntegerv", ExactSpelling = true)]
    unsafe public static extern void GetIntegerv (int count, int* ints);
    [DllImport(opengl32, EntryPoint = "wglMakeCurrent", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool MakeCurrent (IntPtr dc, IntPtr hglrc);
    [DllImport(opengl32, EntryPoint = "wglDeleteContext", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public extern static bool DeleteContext (IntPtr hglrc);

    private static class Extensions {
#pragma warning disable CS0649
        public static readonly delegate* unmanaged[Stdcall]<int, int*, void> glCreateFramebuffers;
        public static readonly delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, void*, void> glReadnPixels;
        public static readonly delegate* unmanaged[Stdcall]<int, DrawBuffer, void> glNamedFramebufferReadBuffer;
        public static readonly delegate* unmanaged[Stdcall]<int, int*, void> glCreateRenderbuffers;
        public static readonly delegate* unmanaged[Stdcall]<int, int> glCheckFramebufferStatus;
        public static readonly delegate* unmanaged[Stdcall]<int, int, int> glCheckNamedFramebufferStatus;
        public static readonly delegate* unmanaged[Stdcall]<int, int, int, int, void> glNamedFramebufferTexture;
        public static readonly delegate* unmanaged[Stdcall]<int, int, int, int, void> glNamedRenderbufferStorage;
        public static readonly delegate* unmanaged[Stdcall]<int, int, int, int, void> glNamedFramebufferRenderbuffer;
        public static readonly delegate* unmanaged[Stdcall]<int, DrawBuffer, void> glNamedFramebufferDrawBuffer;
        public static readonly delegate* unmanaged[Stdcall]<int, int, DrawBuffer[], void> glNamedFramebufferDrawBuffers;
        //public static readonly delegate* unmanaged[Stdcall]<int, void> glDepthFunc;
        public static readonly delegate* unmanaged[Stdcall]<int, void> glActiveTexture;
        //public static readonly delegate* unmanaged[Stdcall]<Capability, bool> glIsEnabled;
        public static readonly delegate* unmanaged[Stdcall]<int, byte*, int> glGetAttribLocation;
        public static readonly delegate* unmanaged[Stdcall]<int, byte*, int> glGetUniformLocation;
        public static readonly delegate* unmanaged[Stdcall]<int> glCreateProgram;
        public static readonly delegate* unmanaged[Stdcall]<ShaderType, int> glCreateShader;
        public static readonly delegate* unmanaged[Stdcall]<int, int, void> glAttachShader;
        public static readonly delegate* unmanaged[Stdcall]<BufferTarget, int, void> glBindBuffer;
        public static readonly delegate* unmanaged[Stdcall]<int, int, void> glBindFramebuffer;
        public static readonly delegate* unmanaged[Stdcall]<int, int, void> glBindRenderbuffer;
        //public static readonly delegate* unmanaged[Stdcall]<int, int, void> glBindTexture;
        public static readonly delegate* unmanaged[Stdcall]<int, void> glBindVertexArray;
        //public static readonly delegate* unmanaged[Stdcall]<BufferBit, void> glClear;
        //public static readonly delegate* unmanaged[Stdcall]<float, float, float, float, void> glClearColor;
        public static readonly delegate* unmanaged[Stdcall]<int, void> glCompileShader;
        public static readonly delegate* unmanaged[Stdcall]<int, int*, void> glCreateBuffers;
        public static readonly delegate* unmanaged[Stdcall]<int, int*, void> glCreateVertexArrays;
        public static readonly delegate* unmanaged[Stdcall]<int, int*, void> glDeleteBuffers;
        public static readonly delegate* unmanaged[Stdcall]<int, int*, void> glDeleteRenderbuffers;
        public static readonly delegate* unmanaged[Stdcall]<int, int*, void> glDeleteVertexArrays;
        public static readonly delegate* unmanaged[Stdcall]<int, int*, void> glDeleteFramebuffers;
        public static readonly delegate* unmanaged[Stdcall]<int, int, int*, void> glCreateTextures;
        public static readonly delegate* unmanaged[Stdcall]<DebugProc, IntPtr, void> glDebugMessageCallback;
        public static readonly delegate* unmanaged[Stdcall]<int, void> glDeleteShader;
        public static readonly delegate* unmanaged[Stdcall]<int, void> glDeleteProgram;
        //public static readonly delegate* unmanaged[Stdcall]<DepthFunction, void> glDepthFunc;
        //public static readonly delegate* unmanaged[Stdcall]<Primitive, int, int, void> glDrawArrays;
        public static readonly delegate* unmanaged[Stdcall]<Primitive, int, int, int, void> glDrawArraysInstanced;
        //public static readonly delegate* unmanaged[Stdcall]<Capability, void> glEnable;
        //public static readonly delegate* unmanaged[Stdcall]<Capability, void> glDisable;
        public static readonly delegate* unmanaged[Stdcall]<int, int, void> glEnableVertexArrayAttrib;
        //public static readonly delegate* unmanaged[Stdcall]<bool, void> glDepthMask;
        public static readonly delegate* unmanaged[Stdcall]<int, ProgramParameter, int*, void> glGetProgramiv;
        public static readonly delegate* unmanaged[Stdcall]<int, ShaderParameter, int*, void> glGetShaderiv;
        public static readonly delegate* unmanaged[Stdcall]<int, int, int*, byte*, void> glGetProgramInfoLog;
        public static readonly delegate* unmanaged[Stdcall]<int, int, int*, byte*, void> glGetShaderInfoLog;
        public static readonly delegate* unmanaged[Stdcall]<int, int, int, int*, int*, UniformType*, byte*, void> glGetActiveUniform;
        public static readonly delegate* unmanaged[Stdcall]<int, int, int, int*, int*, AttribType*, byte*, void> glGetActiveAttrib;
        //public static readonly delegate* unmanaged[Stdcall]<int, int*, void> glGetIntegerv;
        public static readonly delegate* unmanaged[Stdcall]<int, void> glLinkProgram;
        //public static readonly delegate* unmanaged[Stdcall]<int, float*, void> glGetFloatv;
        public static readonly delegate* unmanaged[Stdcall]<int, long, IntPtr, int, void> glNamedBufferStorage;
        public static readonly delegate* unmanaged[Stdcall]<int, long, long, void*, void> glNamedBufferSubData;
        //public static readonly delegate* unmanaged[Stdcall]<int, int, int, int, void> glScissor;
        public static readonly delegate* unmanaged[Stdcall]<int, int, byte**, int*, void> glShaderSource;
        public static readonly delegate* unmanaged[Stdcall]<int, int, int, void> glTextureParameteri;
        public static readonly delegate* unmanaged[Stdcall]<int, int, TextureFormat, int, int, void> glTextureStorage2D;
        public static readonly delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, void*, void> glTextureSubImage2D;
        public static readonly delegate* unmanaged[Stdcall]<int, int, void> glUniform1i;
        public static readonly delegate* unmanaged[Stdcall]<int, float, void> glUniform1f;
        public static readonly delegate* unmanaged[Stdcall]<int, float, float, void> glUniform2f;
        public static readonly delegate* unmanaged[Stdcall]<int, float, float, float, float, void> glUniform4f;
        public static readonly delegate* unmanaged[Stdcall]<int, long, bool, Matrix4x4, void> glUniformMatrix4fv;
        public static readonly delegate* unmanaged[Stdcall]<int, void> glUseProgram;
        public static readonly delegate* unmanaged[Stdcall]<int, int, void> glVertexAttribDivisor;
        public static readonly delegate* unmanaged[Stdcall]<int, int, AttribType, bool, int, long, void> glVertexAttribPointer;
        public static readonly delegate* unmanaged[Stdcall]<IntPtr> wglGetExtensionsStringEXT;
        public static readonly delegate* unmanaged[Stdcall]<IntPtr, IntPtr> wglGetExtensionsStringARB;
        public static readonly delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int*, IntPtr> wglCreateContextAttribsARB;
        public static readonly delegate* unmanaged[Stdcall]<int, int> wglSwapIntervalEXT;
        public static readonly delegate* unmanaged[Stdcall]<int> wglGetSwapIntervalEXT;
        public static readonly delegate* unmanaged[Stdcall]<IntPtr, int, int, uint, int*, int*, int> wglGetPixelFormatAttribivARB;

        static Extensions () {
            try {
                foreach (var f in typeof(Extensions).GetFields(BindingFlags.Public | BindingFlags.Static))
                    if (f.IsInitOnly && (IntPtr)f.GetValue(null) == IntPtr.Zero)
                        f.SetValue(null, GetProcAddress(f.Name));
            } catch (Exception e) {
                Console.WriteLine(e);
                throw;
            }
        }

        private static IntPtr GetProcAddress (string name) {
            var ptr = Opengl.GetProcAddress(name);
            return ptr != IntPtr.Zero ? ptr : throw new ApplicationException($"failed to get proc address of {name}");
        }
#pragma warning restore CS0649
    }

    public static int GetSwapIntervalEXT () => Extensions.wglGetSwapIntervalEXT();
    public static bool SwapIntervalEXT (int frames) => 0 != Extensions.wglSwapIntervalEXT(frames);
    public static IntPtr GetExtensionsString () {
        if (Extensions.wglGetExtensionsStringARB is not null)
            return Extensions.wglGetExtensionsStringARB(GetCurrentDC());
        if (Extensions.wglGetExtensionsStringEXT is not null)
            return Extensions.wglGetExtensionsStringEXT();
        return IntPtr.Zero;
    }
    public static bool ExtensionsSupported => Extensions.wglGetPixelFormatAttribivARB is not null;

    public static int GetPixelFormatCount (IntPtr dc, int a, int b, uint c) {
        int WGL_NUMBER_PIXEL_FORMATS_ARB = 0x2000;
        var count = 0;
        GetPixelFormatAttribivARB(dc, a, b, c, ref WGL_NUMBER_PIXEL_FORMATS_ARB, ref count);
        return count;
    }

    private static void GetPixelFormatAttribivARB (IntPtr deviceContext, int pixelFormatIndex, int b, uint c, ref int attributes, ref int values) {
        fixed (int* a = &attributes)
        fixed (int* v = &values)
            _ = Extensions.wglGetPixelFormatAttribivARB(deviceContext, pixelFormatIndex, b, c, a, v);
    }

    public static void GetPixelFormatAttribivARB (IntPtr deviceContext, int pixelFormatIndex, int b, uint c, int[] attributes, int[] values) {
        fixed (int* a = attributes)
        fixed (int* v = values)
            if (0 == Extensions.wglGetPixelFormatAttribivARB(deviceContext, pixelFormatIndex, b, c, a, v))
                throw new Exception($"{nameof(Extensions.wglGetPixelFormatAttribivARB)}failed");
    }

    public static IntPtr CreateContextAttribsARB (IntPtr dc, IntPtr sharedContext, int[] attribs) {
        fixed (int* p = attribs)
            return Extensions.wglCreateContextAttribsARB(dc, sharedContext, p);
    }

    public static void GetDepthRange (out float near, out float far) {
        float* floats = stackalloc float[2];

        GetFloatv(0x0B70, floats);
        near = floats[0];
        far = floats[1];
    }
    public static void ReadOnePixel (int x, int y, int width, int height, out uint pixel) {
        fixed (uint* p = &pixel)
            Extensions.glReadnPixels(x, y, width, height, Const.RED_INTEGER, Const.INT, sizeof(uint), p);
    }
    //public static void ReadnPixels (int x, int y, int width, int height, int format, int type, int bufSize, void* data) => Extensions.glReadnPixels(x, y, width, height, format, type, bufSize, data);
    public static void NamedFramebufferReadBuffer (int framebuffer, DrawBuffer mode) => Extensions.glNamedFramebufferReadBuffer(framebuffer, mode);
    public static void NamedFramebufferDrawBuffer (int framebuffer, DrawBuffer attachment) => Extensions.glNamedFramebufferDrawBuffer(framebuffer, attachment);
    public static void NamedFramebufferDrawBuffers (int framebuffer, params DrawBuffer[] attachments) => Extensions.glNamedFramebufferDrawBuffers(framebuffer, attachments.Length, attachments);
    public static void NamedFramebufferRenderbuffer (int framebuffer, FramebufferAttachment attachment, int renderbuffer) => Extensions.glNamedFramebufferRenderbuffer(framebuffer, (int)attachment, Const.RENDERBUFFER, renderbuffer);
    public static void NamedFramebufferTexture (int framebuffer, FramebufferAttachment attachment, int texture) => Extensions.glNamedFramebufferTexture(framebuffer, (int)attachment, texture, 0);
    public static FramebufferStatus CheckNamedFramebufferStatus (int framebuffer, FramebufferTarget target) => (FramebufferStatus)Extensions.glCheckNamedFramebufferStatus(framebuffer, (int)target);
    public static int CheckFramebufferStatus (FramebufferTarget target) => Extensions.glCheckFramebufferStatus((int)target);
    public static bool IsEnabled (Capability cap) => 0 != glIsEnabled(cap);
    public static void ActiveTexture (int i) => Extensions.glActiveTexture(i);
    public static void AttachShader (int p, int s) => Extensions.glAttachShader(p, s);
    public static void BindBuffer (BufferTarget target, int buffer) => Extensions.glBindBuffer(target, buffer);
    public static void BindFramebuffer (FramebufferTarget target, int buffer) => Extensions.glBindFramebuffer((int)target, buffer);
    public static void BindRenderbuffer (int buffer) => Extensions.glBindRenderbuffer(Const.RENDERBUFFER, buffer);
    public static void BindVertexArray (int vao) => Extensions.glBindVertexArray(vao);
    public static void CompileShader (int s) => Extensions.glCompileShader(s);
    public static int CreateProgram () => Extensions.glCreateProgram();
    public static int CreateShader (ShaderType shaderType) => Extensions.glCreateShader(shaderType);
    public static void DebugMessageCallback (DebugProc proc, IntPtr userParam) => Extensions.glDebugMessageCallback(proc, userParam);
    public static void DeleteProgram (int program) => Extensions.glDeleteProgram(program);
    public static void DeleteShader (int shader) => Extensions.glDeleteShader(shader);
    public static void DeleteTexture (int texture) => DeleteTextures(1, &texture);
    public static void DeleteBuffer (int i) => Extensions.glDeleteBuffers(1, &i);
    public static void DeleteRenderbuffer (int i) => Extensions.glDeleteRenderbuffers(1, &i);
    public static void DeleteFramebuffer (int i) => Extensions.glDeleteFramebuffers(1, &i);
    public static void DeleteVertexArray (int vao) => Extensions.glDeleteVertexArrays(1, &vao);
    public static void DrawArraysInstanced (Primitive mode, int firstIndex, int indicesPerInstance, int instancesCount) => Extensions.glDrawArraysInstanced(mode, firstIndex, indicesPerInstance, instancesCount);
    public static void EnableVertexArrayAttrib (int id, int i) => Extensions.glEnableVertexArrayAttrib(id, i);
    public static void LinkProgram (int p) => Extensions.glLinkProgram(p);
    public static void NamedRenderbufferStorage (int renderbuffer, RenderbufferFormat format, int width, int height) => Extensions.glNamedRenderbufferStorage(renderbuffer, (int)format, width, height);
    public static void NamedBufferStorage (int buffer, long size, IntPtr data, int flags) => Extensions.glNamedBufferStorage(buffer, size, data, flags);
    public static void NamedBufferSubData (int buffer, long offset, long size, void* data) => Extensions.glNamedBufferSubData(buffer, offset, size, data);
    //public static void Scissor (int x, int y, int width, int height) => Extensions.glScissor(x, y, width, height);
    public static void TextureBaseLevel (int texture, int level) => Extensions.glTextureParameteri(texture, Const.TEXTURE_BASE_LEVEL, level);
    public static void TextureFilter (int texture, MagFilter filter) => Extensions.glTextureParameteri(texture, Const.TEXTURE_MAG_FILTER, (int)filter);
    public static void TextureFilter (int texture, MinFilter filter) => Extensions.glTextureParameteri(texture, Const.TEXTURE_MIN_FILTER, (int)filter);
    public static void TextureMaxLevel (int texture, int level) => Extensions.glTextureParameteri(texture, Const.TEXTURE_MAX_LEVEL, level);
    public static void TextureStorage2D (int texture, int levels, TextureFormat sizedFormat, int width, int height) => Extensions.glTextureStorage2D(texture, levels, sizedFormat, width, height);
    public static void TextureSubImage2D (int texture, int level, int xOffset, int yOffset, int width, int height, PixelFormat format, int type, void* pixels) => Extensions.glTextureSubImage2D(texture, level, xOffset, yOffset, width, height, (int)format, type, pixels);
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

    private static int GetLocation (int program, string name, delegate* unmanaged[Stdcall]<int, byte*, int> f) {
        Span<byte> bytes = stackalloc byte[name.Length + 1];
        var l = Encoding.ASCII.GetBytes(name, bytes);
        if (l != name.Length)
            throw new Exception();
        bytes[name.Length] = 0;
        fixed (byte* p = bytes)
            return f(program, p);
    }

    public static int CreateBuffer () => Create(Extensions.glCreateBuffers);
    public static int CreateVertexArray () => Create(Extensions.glCreateVertexArrays);
    public static int CreateFramebuffer () => Create(Extensions.glCreateFramebuffers);
    public static int CreateRenderbuffer () => Create(Extensions.glCreateRenderbuffers);

    private static int Create (delegate* unmanaged[Stdcall]<int, int*, void> f) {
        int i;
        f(1, &i);
        return i;
    }

    public static void ShaderSource (int id, string source) {
        var bytes = new byte[source.Length + 1];
        var l = Encoding.ASCII.GetBytes(source, bytes);
        if (source.Length != l)
            throw new Exception();
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
    public unsafe static IntPtr CreateSimpleContext (IntPtr dc, Predicate<PixelFormatDescriptor> condition) {
        if (GetCurrentContext() != IntPtr.Zero)
            throw new WinApiException("context already exists");
        var descriptor = new PixelFormatDescriptor { size = PixelFormatDescriptor.Size, version = 1 };
        var pfIndex = FindPixelFormat(dc, ref descriptor, condition);
        if (0 == pfIndex)
            throw new Exception("no pixelformat found");
        if (0 == Gdi.SetPixelFormat(dc, pfIndex, ref descriptor))
            throw new WinApiException("failed SetPixelFormat");
        var rc = CreateContext(dc);
        if (rc == IntPtr.Zero)
            throw new WinApiException("failed wglCreateContext");
        if (!MakeCurrent(dc, rc))
            throw new WinApiException("failed wglMakeCurrent");
        var versionString = Marshal.PtrToStringAnsi(GetString(OpenglString.Version));
        Console.WriteLine(versionString);
        var m = Regex.Match(versionString, @"^(\d\.\d\.\d+) ");
        if (!m.Success)
            throw new Exception($"'{versionString}' not a version string");
        ShaderVersion = Version.Parse(m.Groups[1].Value);
        VersionString = $"{ShaderVersion.Major}{ShaderVersion.Minor}0";
        return rc;
    }
    public static Version ShaderVersion { get; private set; }
    public static string VersionString { get; private set; }

    unsafe static int FindPixelFormat (IntPtr dc, ref PixelFormatDescriptor pfd, Predicate<PixelFormatDescriptor> condition) {
        var formatCount = Gdi.GetPixelFormatCount(dc);
        if (formatCount == 0)
            throw new WinApiException("formatCount == 0");
        var x = 0;
        for (var i = 1; i <= formatCount; i++) {
            Gdi.DescribePixelFormat(dc, i, ref pfd);
            Console.WriteLine(pfd);
            if (condition(pfd) && x == 0)
                x = i;
        }

        return x;
    }
}
