namespace Gl;

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Numerics;
using System.Runtime.InteropServices;
using Win32;
using System.IO;
using Common;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml.Linq;

public delegate void DebugProc (DebugSource sourceEnum, DebugType typeEnum, int id, DebugSeverity severityEnum, int length, IntPtr message, IntPtr userParam);

unsafe public static class Opengl {
    private const string opengl32 = nameof(opengl32) + ".dll";

    [DllImport(opengl32, EntryPoint = "glGetError", ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
    public static extern GlErrorCodes GetError ();
    [DllImport(opengl32, EntryPoint = "wglCreateContext", ExactSpelling = true, SetLastError = true)]
    private static extern IntPtr CreateContext (IntPtr dc);
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
    [DllImport(opengl32, EntryPoint = "glPointSize", ExactSpelling = true, SetLastError = true)]
    public static extern void PointSize (float size);
    [DllImport(opengl32, EntryPoint = "glGetString", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern IntPtr GetString (int name);
    [DllImport(opengl32, EntryPoint = "glFinish", ExactSpelling = true)]
    public static extern void Finish ();
    [DllImport(opengl32, EntryPoint = "glDeleteTextures", ExactSpelling = true)]
    private static     unsafe extern void DeleteTextures (int count, int* ints);
    [DllImport(opengl32, EntryPoint = "glScissor", ExactSpelling = true)]
    public static extern void Scissor (int x, int y, int width, int height);
    [DllImport(opengl32, EntryPoint = "glGetIntegerv", ExactSpelling = true)]
    unsafe public static extern void GetIntegerv (int count, int* ints);
    [DllImport(opengl32, ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool wglMakeCurrent (IntPtr dc, IntPtr hglrc);
    [DllImport(opengl32, ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private extern static bool wglDeleteContext (IntPtr hglrc);
    private static GlExtensions Extensions;

    //private class EarlierOpengl { 
    //        internal EarlierOpengl 
    //}

    private class GlExtensions {
        internal GlExtensions () {
            foreach (var f in typeof(GlExtensions).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                if (f.IsInitOnly && (IntPtr)f.GetValue(this) == IntPtr.Zero) {
                    var ptr = GetProcAddress(f.Name);
                    if (IntPtr.Zero != ptr)
                        f.SetValue(this, ptr);
                    else
                        Debug.WriteLine($"{f.Name} not available");
                }
        }
#pragma warning disable CS0649
        internal readonly delegate* unmanaged[Stdcall]<int, int*, void> glCreateFramebuffers;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, void*, void> glReadnPixels;
        internal readonly delegate* unmanaged[Stdcall]<int, DrawBuffer, void> glNamedFramebufferReadBuffer;
        internal readonly delegate* unmanaged[Stdcall]<int, int*, void> glCreateRenderbuffers;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int> glCheckNamedFramebufferStatus;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, int, void> glNamedFramebufferTexture;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, int, void> glNamedRenderbufferStorage;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, int, void> glNamedFramebufferRenderbuffer;
        internal readonly delegate* unmanaged[Stdcall]<int, DrawBuffer, void> glNamedFramebufferDrawBuffer;
        internal readonly delegate* unmanaged[Stdcall]<int, int, DrawBuffer[], void> glNamedFramebufferDrawBuffers;
        internal readonly delegate* unmanaged[Stdcall]<int, void> glActiveTexture;
        internal readonly delegate* unmanaged[Stdcall]<int, byte*, int> glGetAttribLocation;
        internal readonly delegate* unmanaged[Stdcall]<int, byte*, int> glGetUniformLocation;
        internal readonly delegate* unmanaged[Stdcall]<int> glCreateProgram;
        internal readonly delegate* unmanaged[Stdcall]<ShaderType, int> glCreateShader;
        internal readonly delegate* unmanaged[Stdcall]<int, int, void> glAttachShader;
        internal readonly delegate* unmanaged[Stdcall]<BufferTarget, int, void> glBindBuffer;
        internal readonly delegate* unmanaged[Stdcall]<int, int, void> glBindFramebuffer;
        internal readonly delegate* unmanaged[Stdcall]<int, void> glBindVertexArray;
        internal readonly delegate* unmanaged[Stdcall]<int, void> glCompileShader;
        internal readonly delegate* unmanaged[Stdcall]<int, int*, void> glCreateBuffers;
        internal readonly delegate* unmanaged[Stdcall]<int, long> glGetTextureHandleARB;
        internal readonly delegate* unmanaged[Stdcall]<long, void> glMakeTextureHandleResidentARB;
        internal readonly delegate* unmanaged[Stdcall]<int, int*, void> glCreateVertexArrays;
        internal readonly delegate* unmanaged[Stdcall]<int, int*, void> glDeleteBuffers;
        internal readonly delegate* unmanaged[Stdcall]<int, int*, void> glDeleteRenderbuffers;
        internal readonly delegate* unmanaged[Stdcall]<int, int*, void> glDeleteVertexArrays;
        internal readonly delegate* unmanaged[Stdcall]<int, int*, void> glDeleteFramebuffers;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int*, void> glCreateTextures;
        internal readonly delegate* unmanaged[Stdcall]<DebugProc, IntPtr, void> glDebugMessageCallback;
        internal readonly delegate* unmanaged[Stdcall]<int, void> glDeleteShader;
        internal readonly delegate* unmanaged[Stdcall]<int, void> glDeleteProgram;
        internal readonly delegate* unmanaged[Stdcall]<Primitive, int, int, int, void> glDrawArraysInstanced;
        internal readonly delegate* unmanaged[Stdcall]<int, int, void> glEnableVertexArrayAttrib;
        internal readonly delegate* unmanaged[Stdcall]<int, ProgramParameter, int*, void> glGetProgramiv;
        internal readonly delegate* unmanaged[Stdcall]<int, ShaderParameter, int*, void> glGetShaderiv;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int*, byte*, void> glGetProgramInfoLog;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int*, byte*, void> glGetShaderInfoLog;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, int*, int*, UniformType*, byte*, void> glGetActiveUniform;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, int*, int*, AttribType*, byte*, void> glGetActiveAttrib;
        internal readonly delegate* unmanaged[Stdcall]<int, void> glLinkProgram;
        internal readonly delegate* unmanaged[Stdcall]<int, long, IntPtr, int, void> glNamedBufferStorage;
        internal readonly delegate* unmanaged[Stdcall]<int, long, long, void*, void> glNamedBufferSubData;
        internal readonly delegate* unmanaged[Stdcall]<int, int, byte**, int*, void> glShaderSource;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, void> glTextureParameteri;
        internal readonly delegate* unmanaged[Stdcall]<int, int, TextureFormat, int, int, void> glTextureStorage2D;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, void*, void> glTextureSubImage2D;
        internal readonly delegate* unmanaged[Stdcall]<int, int, void> glUniform1i;
        internal readonly delegate* unmanaged[Stdcall]<int, float, void> glUniform1f;
        internal readonly delegate* unmanaged[Stdcall]<int, float, float, void> glUniform2f;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, void> glUniform2i;
        internal readonly delegate* unmanaged[Stdcall]<int, float, float, float, float, void> glUniform4f;
        internal readonly delegate* unmanaged[Stdcall]<int, long, bool, Matrix4x4, void> glUniformMatrix4fv;
        internal readonly delegate* unmanaged[Stdcall]<int, void> glUseProgram;
        internal readonly delegate* unmanaged[Stdcall]<int, int, void> glVertexAttribDivisor;
        internal readonly delegate* unmanaged[Stdcall]<int, int, AttribType, bool, int, long, void> glVertexAttribPointer;
        internal readonly delegate* unmanaged[Stdcall]<int, int, AttribType, int, long, void> glVertexAttribIPointer;
        internal readonly delegate* unmanaged[Stdcall]<IntPtr> wglGetExtensionsStringEXT;
        internal readonly delegate* unmanaged[Stdcall]<IntPtr, IntPtr> wglGetExtensionsStringARB;
        internal readonly delegate* unmanaged[Stdcall]<int, int> wglSwapIntervalEXT;
        internal readonly delegate* unmanaged[Stdcall]<int> wglGetSwapIntervalEXT;
        internal readonly delegate* unmanaged[Stdcall]<int, int, byte*> glGetStringi;
        //internal readonly delegate* unmanaged[Stdcall]<int, int> glCheckFramebufferStatus;
        //internal readonly delegate* unmanaged[Stdcall]<int, void> glDepthFunc;
        //internal readonly delegate* unmanaged[Stdcall]<Capability, bool> glIsEnabled;
        //internal readonly delegate* unmanaged[Stdcall]<int, int, void> glBindRenderbuffer;
        //internal readonly delegate* unmanaged[Stdcall]<int, int, void> glBindTexture;
        //internal readonly delegate* unmanaged[Stdcall]<BufferBit, void> glClear;
        //internal readonly delegate* unmanaged[Stdcall]<float, float, float, float, void> glClearColor;
        //internal readonly delegate* unmanaged[Stdcall]<DepthFunction, void> glDepthFunc;
        //internal readonly delegate* unmanaged[Stdcall]<Primitive, int, int, void> glDrawArrays;
        //internal readonly delegate* unmanaged[Stdcall]<Capability, void> glEnable;
        //internal readonly delegate* unmanaged[Stdcall]<Capability, void> glDisable;
        //internal readonly delegate* unmanaged[Stdcall]<bool, void> glDepthMask;
        //internal readonly delegate* unmanaged[Stdcall]<int, int*, void> glGetIntegerv;
        //internal readonly delegate* unmanaged[Stdcall]<int, float*, void> glGetFloatv;
        //internal readonly delegate* unmanaged[Stdcall]<int, int, int, int, void> glScissor;

#pragma warning restore CS0649
    }

    private static delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int*, IntPtr> wglCreateContextAttribsARB;
    private static delegate* unmanaged[Stdcall]<IntPtr, int, int, int, int*, int*, int> wglGetPixelFormatAttribivARB;

    public static int GetSwapIntervalEXT () => Extensions.wglGetSwapIntervalEXT();
    public static bool SwapIntervalEXT (int frames) => 0 != Extensions.wglSwapIntervalEXT(frames);

    public static int GetPixelFormatCount (IntPtr dc, int a, int b, int c) {
        int pixelFormatCount = (int)PixelFormatAttributes.PixelFormatCount;
        var count = 0;
        GetPixelFormatAttribivARB(dc, a, b, c, ref pixelFormatCount, ref count);
        return count;
    }

    private static void GetPixelFormatAttribivARB (IntPtr deviceContext, int pixelFormatIndex, int b, int c, ref int attributes, ref int values) {
        fixed (int* a = &attributes)
        fixed (int* v = &values)
            _ = wglGetPixelFormatAttribivARB(deviceContext, pixelFormatIndex, b, c, a, v);
    }

    public static void GetPixelFormatAttribivARB (IntPtr deviceContext, int pixelFormatIndex, int b, int c, int[] attributes, int[] values) {
        fixed (int* a = attributes)
        fixed (int* v = values)
            if (0 == wglGetPixelFormatAttribivARB(deviceContext, pixelFormatIndex, b, c, a, v))
                throw new WinApiException(nameof(wglGetPixelFormatAttribivARB));
    }

    public static IntPtr CreateContextAttribsARB (IntPtr dc, IntPtr sharedContext, ReadOnlySpan<int> attribs) {
        fixed (int* p = attribs) {
            var context = wglCreateContextAttribsARB(dc, sharedContext, p);
            return IntPtr.Zero != context ? context : throw new WinApiException(nameof(wglCreateContextAttribsARB));
        }
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
    //public static int CheckFramebufferStatus (FramebufferTarget target) => Extensions.glCheckFramebufferStatus((int)target);
    public static bool IsEnabled (Capability cap) => 0 != glIsEnabled(cap);
    public static void ActiveTexture (int i) => Extensions.glActiveTexture(i);
    public static void AttachShader (int p, int s) => Extensions.glAttachShader(p, s);
    public static void BindBuffer (BufferTarget target, int buffer) => Extensions.glBindBuffer(target, buffer);
    public static void BindFramebuffer (FramebufferTarget target, int buffer) => Extensions.glBindFramebuffer((int)target, buffer);
    //public static void BindRenderbuffer (int buffer) => Extensions.glBindRenderbuffer(Const.RENDERBUFFER, buffer);
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
    public static void Uniform (int uniform, Vector2i v) => Extensions.glUniform2i(uniform, v.X, v.Y);
    public static void Uniform (int uniform, Vector4 v) => Extensions.glUniform4f(uniform, v.X, v.Y, v.Z, v.W);
    public static void UseProgram (int p) => Extensions.glUseProgram(p);
    public static void VertexAttribDivisor (int index, int divisor) => Extensions.glVertexAttribDivisor(index, divisor);
    public static void VertexAttribPointer (int index, int size, AttribType type, bool normalized, int stride, long ptr) => Extensions.glVertexAttribPointer(index, size, type, normalized, stride, ptr);
    public static void VertexAttribIPointer (int index, int size, AttribType type, int stride, long ptr) => Extensions.glVertexAttribIPointer(index, size, type, stride, ptr);
    public static void Viewport (Vector2i position, Vector2i size) => Viewport(position.X, position.Y, size.X, size.Y);
    public static int GetAttribLocation (int program, string name) => GetLocation(program, name, Extensions.glGetAttribLocation);
    public static int GetUniformLocation (int program, string name) => GetLocation(program, name, Extensions.glGetUniformLocation);
    public static long GetTextureHandleARB (int texture) => Extensions.glGetTextureHandleARB(texture);
    public static void MakeTextureHandleResidentARB (long textureHandle) => Extensions.glMakeTextureHandleResidentARB(textureHandle);

    private static int GetLocation (int program, string name, delegate* unmanaged[Stdcall]<int, byte*, int> f) {
        Span<byte> bytes = name.Length < 1024 ? stackalloc byte[name.Length + 1] : new byte[name.Length + 1];
        var l = Encoding.ASCII.GetBytes(name, bytes);
        if (l != name.Length)
            throw new Exception($"expected {name.Length} characters, not {l}");
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
            throw new Exception($"expected {source.Length} characters, not {l}");
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
        var bufferLength = Maths.IntMin(1024, actualLogLength);
        Span<byte> bytes = stackalloc byte[bufferLength];
        fixed (byte* p = bytes)
            Extensions.glGetProgramInfoLog(id, bufferLength, null, p);
        return Encoding.ASCII.GetString(bytes);
    }

    public static string GetShaderInfoLog (int id) {
        int actualLogLength = GetShader(id, ShaderParameter.InfoLogLength);
        var bufferLength = Maths.IntMin(1024, actualLogLength);
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
    public static void ReleaseCurrent (IntPtr deviceContext) {
        if (IntPtr.Zero == GetCurrentContext())
            throw new Exception("no current context");
        if (!wglMakeCurrent(deviceContext, IntPtr.Zero))
            throw new WinApiException(nameof(wglMakeCurrent));
        Extensions = null;
        supportedExtensions.Clear();
    }

    private static string GetString (OpenglString name) {
        GlException.Assert();
        var ptr = GetString((int)name);
        GlException.Assert();
        if (IntPtr.Zero == ptr)
            throw new Exception("glGetString returned IntPtr.Zero");
        var str = Marshal.PtrToStringAnsi(ptr);
        if (str is null)
            throw new Exception("glGetString returned null string");
        return str;
    }

    public static void MakeCurrent (IntPtr deviceContext, IntPtr renderingContext) {
        if (IntPtr.Zero == renderingContext)
            throw new ArgumentException("may not be zero, use ReleaseCurrent to release a context", nameof(renderingContext));
        if (!wglMakeCurrent(deviceContext, renderingContext))
            throw new WinApiException(nameof(wglMakeCurrent));

        wglGetPixelFormatAttribivARB = (delegate* unmanaged[Stdcall]<IntPtr, int, int, int, int*, int*, int>)GetProcAddress(nameof(wglGetPixelFormatAttribivARB));
        if (wglGetPixelFormatAttribivARB is null)
            throw new Exception($"{nameof(wglGetPixelFormatAttribivARB)} is null");
        wglCreateContextAttribsARB = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int*, IntPtr>)GetProcAddress(nameof(wglCreateContextAttribsARB));
        if (wglCreateContextAttribsARB is null)
            throw new Exception($"{nameof(wglCreateContextAttribsARB)} is null");
        VersionString = GetString(OpenglString.Version);
        Renderer = GetString(OpenglString.Renderer);
        var m = Regex.Match(VersionString, @"^(\d\.\d\.\d+) ((Core|Compatibility) )?");
        if (!m.Success)
            throw new Exception($"'{VersionString}' not a version string");
        ShaderVersion = Version.Parse(m.Groups[1].Value);
        ShaderVersionString = $"{ShaderVersion.Major}{ShaderVersion.Minor}0";
        if (m.Groups[3].Success && Enum.TryParse<ProfileMask>(m.Groups[3].Value, out var profileMask))
            Profile = profileMask;
        else
            Profile = ProfileMask.Unknown;

        Extensions = new();
        if (ProfileMask.Core == Profile) {
            int count = GetIntegerv(IntParameter.NumExtensions);
            for (var i = 0; i < count; ++i) {
                var p = Extensions.glGetStringi((int)OpenglString.Extensions, i);
                if (null == p)
                    throw new Exception($"failed to get ptr to extension string at index {i}");
                supportedExtensions.Add(Marshal.PtrToStringAnsi((IntPtr)p));
            }
        } else {
            supportedExtensions.AddRange(GetString(OpenglString.Extensions).Split(' '));
        };
    }

    public static void DeleteContext (IntPtr renderingContext) {
        if (!wglDeleteContext(renderingContext))
            throw new WinApiException(nameof(wglDeleteContext));
    }

    public static int CreateTexture2D () {
        int i;
        Extensions.glCreateTextures(Const.TEXTURE_2D, 1, &i);
        return i;
    }

    public unsafe static IntPtr CreateSimpleContext (DeviceContext dc, PixelFlag required, PixelFlag rejected) {
        if (GetCurrentContext() != IntPtr.Zero)
            throw new WinApiException("context already exists");
        var descriptor = new PixelFormatDescriptor { size = PixelFormatDescriptor.Size, version = 1 };
        var pfIndex = FindPixelFormat(dc, required, rejected, ref descriptor);
        if (0 == pfIndex)
            throw new Exception("no pixelformat found");
        Gdi32.SetPixelFormat(dc, pfIndex, ref descriptor);
        var rc = CreateContext((IntPtr)dc);
        return rc != IntPtr.Zero ? rc : throw new WinApiException("failed wglCreateContext");
    }
    public static ProfileMask Profile { get; private set; } = ProfileMask.Unknown;
    public static Version ShaderVersion { get; private set; }
    public static string ShaderVersionString { get; private set; }
    public static string VersionString { get; private set; }
    public static string Renderer { get; private set; }

    private static readonly List<string> supportedExtensions = new();
    public static bool IsSupported (string extension) => supportedExtensions.Contains(extension);
    public static IReadOnlyCollection<string> SupportedExtensions { get; } = supportedExtensions;

    private static unsafe int FindPixelFormat (DeviceContext dc, PixelFlag required, PixelFlag rejected, ref PixelFormatDescriptor pfd) {
        var formatCount = Gdi32.GetPixelFormatCount(dc);
        if (formatCount == 0)
            throw new WinApiException("formatCount == 0");
        var x = 0;
        for (var i = 1; i <= formatCount; i++) {
            Gdi32.DescribePixelFormat(dc, i, ref pfd);
            if ((pfd.flags & required) == required && (pfd.flags & rejected) == 0 && x == 0)
                x = i;
        }

        return x;
    }
}
