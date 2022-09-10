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

#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
public delegate void DebugProc (DebugSource sourceEnum, DebugType typeEnum, int id, DebugSeverity severityEnum, int length, nint message, nint userParam);

unsafe public static class Opengl {
    private const string opengl32 = nameof(opengl32) + ".dll";

    [DllImport(opengl32)]
    private static extern uint glGetError ();
    [DllImport(opengl32, SetLastError = true)]
    private static extern nint wglCreateContext (nint dc);
    [DllImport(opengl32, SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern nint wglGetProcAddress (string name);
    [DllImport(opengl32)]
    private static extern nint wglGetCurrentDC ();
    [DllImport(opengl32, SetLastError = true)]
    private static extern nint wglGetCurrentContext ();
    [DllImport(opengl32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool wglMakeCurrent (nint dc, nint hglrc);
    [DllImport(opengl32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private extern static bool wglDeleteContext (nint hglrc);

    private static GlFunctions functions;

    private class GlFunctions {
        private static nint opengl32dll;

        internal GlFunctions () {
            foreach (var f in typeof(GlFunctions).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                if (f.IsInitOnly && 0 == (nint)f.GetValue(this)) {
                    var extPtr = wglGetProcAddress(f.Name);
                    if (0 != extPtr) {
                        f.SetValue(this, extPtr);
                        continue;
                    }
                    if (0 == opengl32dll)
                        if (!Kernel32.GetModuleHandleEx(2, opengl32, ref opengl32dll) || 0 == opengl32dll)
                            throw new WinApiException($"failed to get handle of {opengl32}");

                    var glPtr = Kernel32.GetProcAddress(opengl32dll, f.Name);
                    if (0 == glPtr)
                        throw new WinApiException($"failed to get address of {f.Name}");
                    f.SetValue(this, glPtr);
                }
        }
#pragma warning disable CS0649
        internal readonly delegate* unmanaged[Stdcall]<int, int*, void> glCreateFramebuffers;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, void*, void> glReadnPixels;
        internal readonly delegate* unmanaged[Stdcall]<int, int, void> glNamedFramebufferReadBuffer;
        internal readonly delegate* unmanaged[Stdcall]<int, int*, void> glCreateRenderbuffers;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int> glCheckNamedFramebufferStatus;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, int, void> glViewport;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, int, void> glNamedFramebufferTexture;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, int, void> glNamedRenderbufferStorage;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, int, void> glNamedFramebufferRenderbuffer;
        internal readonly delegate* unmanaged[Stdcall]<int, int, void> glNamedFramebufferDrawBuffer;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int*, void> glNamedFramebufferDrawBuffers;
        internal readonly delegate* unmanaged[Stdcall]<int, void> glActiveTexture;
        internal readonly delegate* unmanaged[Stdcall]<int, byte*, int> glGetAttribLocation;
        internal readonly delegate* unmanaged[Stdcall]<int, byte*, int> glGetUniformLocation;
        internal readonly delegate* unmanaged[Stdcall]<int> glCreateProgram;
        internal readonly delegate* unmanaged[Stdcall]<int, int> glCreateShader;
        internal readonly delegate* unmanaged[Stdcall]<int, int, void> glAttachShader;
        internal readonly delegate* unmanaged[Stdcall]<int, nint> glGetString;
        internal readonly delegate* unmanaged[Stdcall]<int, int, void> glBindBuffer;
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
        internal readonly delegate* unmanaged[Stdcall]<DebugProc, nint, void> glDebugMessageCallback;
        internal readonly delegate* unmanaged[Stdcall]<int, void> glDeleteShader;
        internal readonly delegate* unmanaged[Stdcall]<int, void> glDeleteProgram;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, void> glDrawArrays;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, int, void> glDrawArraysInstanced;
        internal readonly delegate* unmanaged[Stdcall]<int, int, void> glEnableVertexArrayAttrib;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int*, void> glGetProgramiv;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int*, void> glGetShaderiv;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int*, byte*, void> glGetProgramInfoLog;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int*, byte*, void> glGetShaderInfoLog;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, int*, int*, int*, byte*, void> glGetActiveUniform;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, int*, int*, int*, byte*, void> glGetActiveAttrib;
        internal readonly delegate* unmanaged[Stdcall]<int, void> glLinkProgram;
        internal readonly delegate* unmanaged[Stdcall]<int, long, nint, int, void> glNamedBufferStorage;
        internal readonly delegate* unmanaged[Stdcall]<int, long, long, void*, void> glNamedBufferSubData;
        internal readonly delegate* unmanaged[Stdcall]<int, int, byte**, int*, void> glShaderSource;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, void> glTextureParameteri;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, int, int, void> glTextureStorage2D;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, void*, void> glTextureSubImage2D;
        internal readonly delegate* unmanaged[Stdcall]<int, int, void> glUniform1i;
        internal readonly delegate* unmanaged[Stdcall]<int, float, void> glUniform1f;
        internal readonly delegate* unmanaged[Stdcall]<int, float, float, void> glUniform2f;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, void> glUniform2i;
        internal readonly delegate* unmanaged[Stdcall]<int, float, float, float, float, void> glUniform4f;
        internal readonly delegate* unmanaged[Stdcall]<int, long, byte, Matrix4x4, void> glUniformMatrix4fv;
        internal readonly delegate* unmanaged[Stdcall]<int, void> glUseProgram;
        internal readonly delegate* unmanaged[Stdcall]<int, int, void> glVertexAttribDivisor;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, byte, int, long, void> glVertexAttribPointer;
        internal readonly delegate* unmanaged[Stdcall]<int, int, int, int, long, void> glVertexAttribIPointer;
        internal readonly delegate* unmanaged[Stdcall]<nint> wglGetExtensionsStringEXT;
        internal readonly delegate* unmanaged[Stdcall]<nint, nint> wglGetExtensionsStringARB;
        internal readonly delegate* unmanaged[Stdcall]<int, int> wglSwapIntervalEXT;
        internal readonly delegate* unmanaged[Stdcall]<int> wglGetSwapIntervalEXT;
        internal readonly delegate* unmanaged[Stdcall]<int, int, byte*> glGetStringi;
        //internal readonly delegate* unmanaged[Stdcall]<int, int> glCheckFramebufferStatus;
        //internal readonly delegate* unmanaged[Stdcall]<int, void> glDepthFunc;
        internal readonly delegate* unmanaged[Stdcall]<int, byte> glIsEnabled;
        //internal readonly delegate* unmanaged[Stdcall]<int, int, void> glBindRenderbuffer;
        internal readonly delegate* unmanaged[Stdcall]<int, int, void> glBindTexture;
        internal readonly delegate* unmanaged[Stdcall]<int, void> glClear;
        internal readonly delegate* unmanaged[Stdcall]<float, float, float, float, void> glClearColor;
        internal readonly delegate* unmanaged[Stdcall]<int, void> glDepthFunc;
        internal readonly delegate* unmanaged[Stdcall]<int, void> glEnable;
        internal readonly delegate* unmanaged[Stdcall]<int, void> glDisable;
        //internal readonly delegate* unmanaged[Stdcall]<bool, void> glDepthMask;
        internal readonly delegate* unmanaged[Stdcall]<int, int*, void> glDeleteTextures;
        internal readonly delegate* unmanaged[Stdcall]<int, int*, void> glGetIntegerv;
        internal readonly delegate* unmanaged[Stdcall]<int, float*, void> glGetFloatv;
        //internal readonly delegate* unmanaged[Stdcall]<int, int, int, int, void> glScissor;
        internal readonly delegate* unmanaged[Stdcall]<uint, uint, nint> glFenceSync;
        internal readonly delegate* unmanaged[Stdcall]<nint, void> glDeleteSync;
        internal readonly delegate* unmanaged[Stdcall]<nint, uint, int, int*, int*, void> glGetSynciv;
        internal readonly delegate* unmanaged[Stdcall]<void> glFlush;
#pragma warning restore CS0649
    }

    private static delegate* unmanaged[Stdcall]<nint, nint, int*, nint> wglCreateContextAttribsARB;
    private static delegate* unmanaged[Stdcall]<nint, int, int, int, int*, int*, int> wglGetPixelFormatAttribivARB;

    private static int GetSwapIntervalEXT () => functions.wglGetSwapIntervalEXT();
    private static bool SwapIntervalEXT (int frames) => 0 != functions.wglSwapIntervalEXT(frames);

    public static void SetSwapInterval (int value) {
        if (value != GetSwapIntervalEXT()) {
            if (!SwapIntervalEXT(value))
                throw new GlException(SetInt32Failed(nameof(SwapIntervalEXT), value));
            if (value != GetSwapIntervalEXT())
                throw new GlException(SetInt32Failed(nameof(SwapIntervalEXT), value));
        }
    }

    public static int GetPixelFormatCountARB (DeviceContext dc) {
        int pixelFormatCount = (int)PixelFormatAttrib.PixelFormatCount;
        var count = 0;
        GetPixelFormatAttribivARB((nint)dc, 1, 0, 1, ref pixelFormatCount, ref count);
        return count;
    }

    private static void GetPixelFormatAttribivARB (nint deviceContext, int pixelFormatIndex, int layerPlane, int attributeCount, ref int attributes, ref int values) {
        fixed (int* a = &attributes)
        fixed (int* v = &values)
            _ = wglGetPixelFormatAttribivARB(deviceContext, pixelFormatIndex, layerPlane, attributeCount, a, v);
    }

    public static void GetPixelFormatAttribivARB (nint deviceContext, int pixelFormatIndex, int[] attributes, int[] values) {
        if (attributes.Length != values.Length)
            throw new ArgumentException("unequal array lengths", nameof(attributes));
        if (0 == attributes.Length)
            throw new ArgumentException("arrays may not be empty", nameof(attributes));
        fixed (int* a = attributes)
        fixed (int* v = values)
            if (0 == wglGetPixelFormatAttribivARB(deviceContext, pixelFormatIndex, 0, attributes.Length, a, v))
                throw new WinApiException(nameof(wglGetPixelFormatAttribivARB));
    }

    public static nint CreateContextAttribsARB (nint dc, nint sharedContext, ReadOnlySpan<int> attribs) {
        fixed (int* p = attribs) {
            var context = wglCreateContextAttribsARB(dc, sharedContext, p);
            return 0 != context ? context : throw new WinApiException(nameof(wglCreateContextAttribsARB));
        }
    }

    public static void ReadOnePixel (int x, int y, int width, int height, out uint pixel) {
        fixed (uint* p = &pixel)
            functions.glReadnPixels(x, y, width, height, Const.RED_INTEGER, Const.INT, sizeof(uint), p);
    }
    public static void DepthFunc (DepthFunction function) => functions.glDepthFunc((int)function);
    public static GlErrorCodes GetError () => (GlErrorCodes)glGetError();
    public static nint GetCurrentContext () => wglGetCurrentContext();
    public static void Viewport (Vector2i position, Vector2i size) => functions.glViewport(position.X, position.Y, size.X, size.Y);
    public static int GetAttribLocation (int program, string name) => GetLocation(program, name, functions.glGetAttribLocation);
    public static int GetUniformLocation (int program, string name) => GetLocation(program, name, functions.glGetUniformLocation);
    //public static void ReadnPixels (int x, int y, int width, int height, int format, int type, int bufSize, void* data) => Extensions.glReadnPixels(x, y, width, height, format, type, bufSize, data);
    public static void NamedFramebufferReadBuffer (int framebuffer, DrawBuffer mode) => functions.glNamedFramebufferReadBuffer(framebuffer, (int)mode);
    public static void NamedFramebufferDrawBuffer (int framebuffer, DrawBuffer attachment) => functions.glNamedFramebufferDrawBuffer(framebuffer, (int)attachment);
    public static void NamedFramebufferDrawBuffers (int framebuffer, params DrawBuffer[] attachments) {
        if (0 == attachments.Length)
            throw new InvalidOperationException("must have at least one attachment");
        fixed (DrawBuffer* db = attachments) {
            int* p = (int*)db;
            functions.glNamedFramebufferDrawBuffers(framebuffer, attachments.Length, p);
        }
    }
    public static void NamedFramebufferRenderbuffer (int framebuffer, FramebufferAttachment attachment, int renderbuffer) => functions.glNamedFramebufferRenderbuffer(framebuffer, (int)attachment, Const.RENDERBUFFER, renderbuffer);
    public static void NamedFramebufferTexture (int framebuffer, FramebufferAttachment attachment, int texture) => functions.glNamedFramebufferTexture(framebuffer, (int)attachment, texture, 0);
    public static FramebufferStatus CheckNamedFramebufferStatus (int framebuffer, FramebufferTarget target) => (FramebufferStatus)functions.glCheckNamedFramebufferStatus(framebuffer, (int)target);
    //public static int CheckFramebufferStatus (FramebufferTarget target) => Extensions.glCheckFramebufferStatus((int)target);
    public static void Enable (Capability cap) => functions.glEnable((int)cap);
    public static void Disable (Capability cap) => functions.glDisable((int)cap);
    public static bool IsEnabled (Capability cap) => 0 != functions.glIsEnabled((int)cap);
    public static void ActiveTexture (int i) {
        if (i != GetIntegerv(IntParameter.ActiveTexture) - Const.TEXTURE0)
            functions.glActiveTexture(Const.TEXTURE0 + i);
    }
    public static void AttachShader (int p, int s) => functions.glAttachShader(p, s);
    public static void BindBuffer (BufferTarget target, int buffer) => functions.glBindBuffer((int)target, buffer);
    public static void BindDefaultFramebuffer () {
        if (0 != GetIntegerv(IntParameter.FramebufferBinding)) {
            functions.glBindFramebuffer((int)FramebufferTarget.Framebuffer, 0);
            if (0 != GetIntegerv(IntParameter.FramebufferBinding))
                throw new GlException(SetInt32Failed(nameof(FramebufferTarget.Framebuffer), 0));
        }

    }
    public static void BindFramebuffer (Framebuffer buffer) {
        if (buffer != GetIntegerv(IntParameter.FramebufferBinding)) {
            functions.glBindFramebuffer((int)FramebufferTarget.Framebuffer, buffer);
            if (buffer != GetIntegerv(IntParameter.FramebufferBinding))
                throw new GlException(SetInt32Failed(nameof(FramebufferTarget.Framebuffer), buffer));
        }
    }

    //public static void BindRenderbuffer (int buffer) => Extensions.glBindRenderbuffer(Const.RENDERBUFFER, buffer);
    public static void BindVertexArray (VertexArray value) {
        if (value != GetIntegerv(IntParameter.VertexArrayBinding)) {
            functions.glBindVertexArray(value);
            if (value != GetIntegerv(IntParameter.VertexArrayBinding))
                throw new GlException(SetInt32Failed(nameof(IntParameter.VertexArrayBinding), value));
        }
    }

    public static void CompileShader (int s) => functions.glCompileShader(s);
    public static int CreateProgram () => functions.glCreateProgram();
    public static int CreateShader (ShaderType shaderType) => functions.glCreateShader((int)shaderType);
    public static void DebugMessageCallback (DebugProc proc, nint userParam) => functions.glDebugMessageCallback(proc, userParam);
    public static void DeleteProgram (int program) => functions.glDeleteProgram(program);
    public static void DeleteShader (int shader) => functions.glDeleteShader(shader);
    public static void DeleteTexture (int texture) => functions.glDeleteTextures(1, &texture);
    public static void DeleteBuffer (int i) => functions.glDeleteBuffers(1, &i);
    public static void DeleteRenderbuffer (int i) => functions.glDeleteRenderbuffers(1, &i);
    public static void DeleteFramebuffer (int i) => functions.glDeleteFramebuffers(1, &i);
    public static void DeleteVertexArray (int vao) => functions.glDeleteVertexArrays(1, &vao);
    public static void DrawArrays (Primitive mode, int first, int count) => functions.glDrawArrays((int)mode, first, count);
    public static void DrawArraysInstanced (Primitive mode, int firstIndex, int indicesPerInstance, int instancesCount) => functions.glDrawArraysInstanced((int)mode, firstIndex, indicesPerInstance, instancesCount);
    public static void EnableVertexArrayAttrib (int id, int i) => functions.glEnableVertexArrayAttrib(id, i);
    public static void LinkProgram (int p) => functions.glLinkProgram(p);
    public static void NamedRenderbufferStorage (int renderbuffer, RenderbufferFormat format, int width, int height) => functions.glNamedRenderbufferStorage(renderbuffer, (int)format, width, height);
    public static void NamedBufferStorage (int buffer, long size, nint data, int flags) => functions.glNamedBufferStorage(buffer, size, data, flags);
    public static void NamedBufferSubData (int buffer, long offset, long size, void* data) => functions.glNamedBufferSubData(buffer, offset, size, data);
    //public static void Scissor (int x, int y, int width, int height) => Extensions.glScissor(x, y, width, height);
    public static void TextureBaseLevel (int texture, int level) => functions.glTextureParameteri(texture, Const.TEXTURE_BASE_LEVEL, level);
    public static void TextureFilter (int texture, MagFilter filter) => functions.glTextureParameteri(texture, Const.TEXTURE_MAG_FILTER, (int)filter);
    public static void TextureFilter (int texture, MinFilter filter) => functions.glTextureParameteri(texture, Const.TEXTURE_MIN_FILTER, (int)filter);
    public static void TextureMaxLevel (int texture, int level) => functions.glTextureParameteri(texture, Const.TEXTURE_MAX_LEVEL, level);
    public static void TextureStorage2D (int texture, int levels, TextureFormat sizedFormat, int width, int height) => functions.glTextureStorage2D(texture, levels, (int)sizedFormat, width, height);
    public static void TextureSubImage2D (int texture, int level, int xOffset, int yOffset, int width, int height, PixelFormat format, int type, void* pixels) => functions.glTextureSubImage2D(texture, level, xOffset, yOffset, width, height, (int)format, type, pixels);
    public static void TextureWrap (int texture, WrapCoordinate c, Wrap w) => functions.glTextureParameteri(texture, (int)c, (int)w);
    public static void Uniform (int uniform, float f) => functions.glUniform1f(uniform, f);
    public static void Uniform (int uniform, int i) => functions.glUniform1i(uniform, i);
    public static void Uniform (int uniform, Matrix4x4 m) => functions.glUniformMatrix4fv(uniform, 1, 0, m);
    public static void Uniform (int uniform, Vector2 v) => functions.glUniform2f(uniform, v.X, v.Y);
    public static void Uniform (int uniform, Vector2i v) => functions.glUniform2i(uniform, v.X, v.Y);
    public static void Uniform (int uniform, Vector4 v) => functions.glUniform4f(uniform, v.X, v.Y, v.Z, v.W);
    public static void UseProgram (Program p) => functions.glUseProgram((int)p);
    public static void VertexAttribDivisor (int index, int divisor) => functions.glVertexAttribDivisor(index, divisor);
    public static void VertexAttribPointer (int index, int size, AttribType type, bool normalized, int stride, long ptr) => functions.glVertexAttribPointer(index, size, (int)type, normalized ? (byte)1 : (byte)0, stride, ptr);
    public static void VertexAttribIPointer (int index, int size, AttribType type, int stride, long ptr) => functions.glVertexAttribIPointer(index, size, (int)type, stride, ptr);
    public static long GetTextureHandleARB (int texture) => functions.glGetTextureHandleARB(texture);
    public static void MakeTextureHandleResidentARB (long textureHandle) => functions.glMakeTextureHandleResidentARB(textureHandle);
    public static void ClearColor (float r, float g, float b, float a) => functions.glClearColor(r, g, b, a);
    public static void Clear (BufferBit what) => functions.glClear((int)what);
    public static void BindTexture (int type, int id) => functions.glBindTexture(type, id);
    public static void Viewport (int x, int y, int w, int h) => functions.glViewport(x, y, w, h);
    public static void Flush () => functions.glFlush();
    private static int GetLocation (int program, string name, delegate* unmanaged[Stdcall]<int, byte*, int> f) {
        Span<byte> bytes = name.Length < 1024 ? stackalloc byte[name.Length + 1] : new byte[name.Length + 1];
        var l = Encoding.ASCII.GetBytes(name, bytes);
        if (l != name.Length)
            throw new Exception($"expected {name.Length} characters, not {l}");
        bytes[name.Length] = 0;
        fixed (byte* p = bytes)
            return f(program, p);
    }

    public static int CreateBuffer () => Create(functions.glCreateBuffers);
    public static int CreateVertexArray () => Create(functions.glCreateVertexArrays);
    public static int CreateFramebuffer () => Create(functions.glCreateFramebuffers);
    public static int CreateRenderbuffer () => Create(functions.glCreateRenderbuffers);

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
            functions.glShaderSource(id, 1, &strPtr, null);
    }

    public static int GetProgram (int id, ProgramParameter p) { // I wish I could join this with GetShader
        int i;
        functions.glGetProgramiv(id, (int)p, &i);
        return i;
    }

    public static int GetShader (int id, ShaderParameter p) {
        int i;
        functions.glGetShaderiv(id, (int)p, &i);
        return i;
    }

    public static (int size, AttribType type, string name) GetActiveAttrib (int id, int index) {
        var maxLength = GetProgram(id, ProgramParameter.ActiveAttributeMaxLength);
        int length, size;
        int type;
        Span<byte> bytes = stackalloc byte[maxLength];
        fixed (byte* p = bytes)
            functions.glGetActiveAttrib(id, index, maxLength, &length, &size, &type, p);
        var n = length > 0 ? Encoding.ASCII.GetString(bytes.Slice(0, length)) : "";
        return (size, (AttribType)type, n);
    }

    public static (int size, UniformType type, string name) GetActiveUniform (int id, int index) {
        var maxLength = GetProgram(id, ProgramParameter.ActiveUniformMaxLength);
        int length, size;
        int type;
        Span<byte> bytes = stackalloc byte[maxLength];
        fixed (byte* p = bytes)
            functions.glGetActiveUniform(id, index, maxLength, &length, &size, &type, p);
        var n = length > 0 ? Encoding.ASCII.GetString(bytes.Slice(0, length)) : "";
        return (size, (UniformType)type, n);
    }

    public static string GetProgramInfoLog (int id) {
        int actualLogLength = GetProgram(id, ProgramParameter.InfoLogLength);
        var bufferLength = Maths.IntMin(1024, actualLogLength);
        Span<byte> bytes = stackalloc byte[bufferLength];
        fixed (byte* p = bytes)
            functions.glGetProgramInfoLog(id, bufferLength, null, p);
        return Encoding.ASCII.GetString(bytes);
    }

    public static string GetShaderInfoLog (int id) {
        int actualLogLength = GetShader(id, ShaderParameter.InfoLogLength);
        var bufferLength = Maths.IntMin(1024, actualLogLength);
        Span<byte> bytes = stackalloc byte[bufferLength];
        fixed (byte* p = bytes)
            functions.glGetShaderInfoLog(id, bufferLength, null, p);
        return Encoding.ASCII.GetString(bytes);
    }

    public static int GetIntegerv (IntParameter p) {
        int i;
        functions.glGetIntegerv((int)p, &i);
        return i;
    }
    public static void ReleaseCurrent (DeviceContext deviceContext) {
        if (0 == wglGetCurrentContext())
            throw new Exception("no current context");
        if (!wglMakeCurrent((nint)deviceContext, 0))
            throw new WinApiException(nameof(wglMakeCurrent));
        functions = null;
        supportedExtensions.Clear();
    }

    private static string GetString (OpenglString name) {
        GlException.Assert();
        var ptr = functions.glGetString((int)name);
        GlException.Assert();
        if (0 == ptr)
            throw new Exception("glGetString returned 0");
        var str = Marshal.PtrToStringAnsi(ptr);
        if (str is null)
            throw new Exception("glGetString returned null string");
        return str;
    }

    private static void MakeCurrent (DeviceContext deviceContext, nint renderingContext) {
        if (!wglMakeCurrent((nint)deviceContext, renderingContext))
            throw new WinApiException(nameof(wglMakeCurrent));

        wglGetPixelFormatAttribivARB = (delegate* unmanaged[Stdcall]<nint, int, int, int, int*, int*, int>)wglGetProcAddress(nameof(wglGetPixelFormatAttribivARB));
        if (wglGetPixelFormatAttribivARB is null)
            throw new Exception($"{nameof(wglGetPixelFormatAttribivARB)} is null");
        wglCreateContextAttribsARB = (delegate* unmanaged[Stdcall]<nint, nint, int*, nint>)wglGetProcAddress(nameof(wglCreateContextAttribsARB));
        if (wglCreateContextAttribsARB is null)
            throw new Exception($"{nameof(wglCreateContextAttribsARB)} is null");

        functions = new();

        VersionString = GetString(OpenglString.Version);
        Renderer = GetString(OpenglString.Renderer);
        var m = Regex.Match(VersionString, @"^(\d\.\d\.\d+) ((Core|Compatibility) )?");
        if (!m.Success)
            throw new Exception($"'{VersionString}' not a version string");
        ContextVersion = Version.Parse(m.Groups[1].Value);
        ShaderVersionString = $"{ContextVersion.Major}{ContextVersion.Minor}0";
        if (m.Groups[3].Success && Enum.TryParse<ProfileMask>(m.Groups[3].Value, out var profileMask))
            Profile = profileMask;
        else
            Profile = ProfileMask.Undefined;

        if (LegacyOpenglVersion < ContextVersion) {
            int count = GetIntegerv(IntParameter.NumExtensions);
            for (var i = 0; i < count; ++i) {
                var p = functions.glGetStringi((int)OpenglString.Extensions, i);
                if (null == p)
                    throw new Exception($"failed to get ptr to extension string at index {i}");
                supportedExtensions.Add(Marshal.PtrToStringAnsi((nint)p));
            }
        } else {
            supportedExtensions.AddRange(GetString(OpenglString.Extensions).Split(' '));
        };
    }
    static readonly Version LegacyOpenglVersion = new(3, 0, 0);
    public static void DeleteContext (nint renderingContext) {
        if (!wglDeleteContext(renderingContext))
            throw new WinApiException(nameof(wglDeleteContext));
    }

    public static int CreateTexture2D () {
        int i;
        functions.glCreateTextures(Const.TEXTURE_2D, 1, &i);
        return i;
    }

    public static nint CreateContextARB (DeviceContext dc, nint ctx, ContextConfigurationARB configuration) {
        var version = configuration.Version ?? ContextVersion;
        var contextFlags = configuration.Flags ?? ContextFlag.Debug;
        var profileMask = configuration.Profile ?? ProfileMask.Core;
        var nameValuePairs = new int[] {
            (int)ContextAttrib.MajorVersion, version.Major,
            (int)ContextAttrib.MinorVersion, version.Minor,
            (int)ContextAttrib.ContextFlags, (int) contextFlags,
            (int)ContextAttrib.ProfileMask, (int)profileMask,
            0, 0,
        };
        var ctxARB = CreateContextAttribsARB(dc, nameValuePairs);
        MakeCurrent(dc, ctxARB);
        DeleteContext(ctx);
        return ctxARB;
    }

    private static nint CreateContextAttribsARB (DeviceContext dc, int[] attribs) {
        if (attribs.Length < 2 || attribs[^1] != 0 || attribs[^2] != 0)
            throw new ArgumentException("must have at least 2 arguments", nameof(attribs));
        fixed (int* p = attribs) {
            var ctxARB = wglCreateContextAttribsARB((nint)dc, 0, p);
            return 0 != ctxARB ? ctxARB : throw new WinApiException(nameof(wglCreateContextAttribsARB));
        }
    }

    public static nint CreateSimpleContext (DeviceContext dc, ContextConfiguration configuration) {
        if (0 != wglGetCurrentContext())
            throw new WinApiException("context already exists");
        var descriptor = new PixelFormatDescriptor();
        var pfIndex = FindPixelFormat(dc, ref descriptor, configuration);
        if (0 == pfIndex)
            throw new Exception("no pixelformat found");
        Gdi32.SetPixelFormat(dc, pfIndex, ref descriptor);
        var rc = wglCreateContext((nint)dc);
        if (0 == rc)
            throw new WinApiException("failed wglCreateContext");
        MakeCurrent(dc, rc);
        return rc;
    }
    public static ProfileMask Profile { get; private set; } = ProfileMask.Undefined;
    public static Version ContextVersion { get; private set; }
    public static string ShaderVersionString { get; private set; }
    public static string VersionString { get; private set; }
    public static string Renderer { get; private set; }

    private static readonly List<string> supportedExtensions = new();
    public static bool IsSupported (string extension) => supportedExtensions.Contains(extension);
    public static IReadOnlyCollection<string> SupportedExtensions { get; } = supportedExtensions;

    const PixelFlag RequiredFlags = PixelFlag.SupportOpengl | PixelFlag.DrawToWindow;
    const PixelFlag RejectedFlags = PixelFlag.GenericAccelerated | PixelFlag.GenericFormat;

    private static int FindPixelFormat (DeviceContext dc, ref PixelFormatDescriptor pfd, ContextConfiguration configuration) {
        var formatCount = Gdi32.GetPixelFormatCount(dc);
        if (formatCount == 0)
            throw new WinApiException("formatCount == 0");

        var requireDoubleBuffer = configuration.DoubleBuffer is bool _0 && _0 ? PixelFlag.DoubleBuffer : PixelFlag.None;
        var rejectDoubleBuffer = configuration.DoubleBuffer is bool _1 && !_1 ? PixelFlag.DoubleBuffer : PixelFlag.None;
        var requireComposited = configuration.Composited is bool _2 && _2 ? PixelFlag.SupportComposition : PixelFlag.None;
        var rejectComposited = configuration.Composited is bool _3 && !_3 ? PixelFlag.SupportComposition : PixelFlag.None;
        var (requireSwapMethod, rejectSwapMethod) = configuration.SwapMethod switch {
            SwapMethod.Copy => (PixelFlag.SwapCopy, PixelFlag.SwapExchange),
            SwapMethod.Swap => (PixelFlag.SwapExchange, PixelFlag.SwapCopy),
            SwapMethod.Undefined => (PixelFlag.None, PixelFlag.SwapExchange | PixelFlag.SwapCopy),
            _ => (PixelFlag.None, PixelFlag.None)
        };
        var required = RequiredFlags | requireDoubleBuffer | requireComposited | requireSwapMethod;
        var rejected = RejectedFlags | rejectDoubleBuffer | rejectComposited | rejectSwapMethod;
        var colorBits = configuration.ColorBits ?? 32;
        var depthBits = configuration.DepthBits ?? 24;
        var x = 0;
        for (var i = 1; i <= formatCount && 0 == x; i++) {
            Gdi32.DescribePixelFormat(dc, i, ref pfd);
            if (colorBits == pfd.colorBits && depthBits <= pfd.depthBits && required == (pfd.flags & required) && 0 == (pfd.flags & rejected))
                x = i;
        }

        return x;
    }
    private static string SetBoolFailed (string name, bool value) => $"failed to turn {name} {(value ? "on" : "off")}";
    private static string SetInt32Failed (string name, int value) => $"failed to set {name} to {value}";
    private static string SetEnumFailed<T> (T value) where T : Enum => $"failed to set {typeof(T)} to {value}";

    internal static nint FenceSync () {
        var fence = functions.glFenceSync(GL_SYNC_GPU_COMMANDS_COMPLETE, 0);
        return 0 != fence ? fence : throw new GlException();
    }

    internal static void DeleteSync (nint sync) {
        functions.glDeleteSync(sync);
    }

    internal const uint GL_OBJECT_TYPE = 0x9112;
    internal const uint GL_SYNC_CONDITION = 0x9113;
    internal const uint GL_SYNC_STATUS = 0x9114;
    internal const uint GL_SYNC_FLAGS = 0x9115;
    internal const uint GL_SYNC_FENCE = 0x9116;
    internal const uint GL_SYNC_GPU_COMMANDS_COMPLETE = 0x9117;
    internal const uint GL_UNSIGNALED = 0x9118;
    internal const uint GL_SIGNALED = 0x9119;
    internal const uint GL_ALREADY_SIGNALED = 0x911A;
    internal const uint GL_TIMEOUT_EXPIRED = 0x911B;
    internal static int GetSynci (nint sync, uint name) {
        var value = 0;
        var length = 0;
        functions.glGetSynciv(sync, name, 1, &length, &value);
        if (1 != length)
            throw new GlException();
        return value;
    }
}
#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments