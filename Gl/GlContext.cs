namespace Gl;

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System;
using System.Text;
using Win32;
using System.Numerics;
using Common;

public delegate void DebugProc (DebugSource sourceEnum, DebugType typeEnum, int id, DebugSeverity severityEnum, int length, nint message, nint userParam);

public sealed unsafe class GlContext:IDisposable {

    public static int GetSwapInterval () => wglGetSwapIntervalEXT();
    public static void AttachShader (int program, int shader) => glAttachShader(program, shader);
    public static void ClearColor (float r, float g, float b, float a) => glClearColor(r, g, b, a);
    public static void Clear (BufferBit mask) => glClear((int)mask);
    public static void UseProgram (Program p) => glUseProgram((int)p);
    public static void Enable (Capability cap) => glEnable((int)cap);
    public static void Disable (Capability cap) => glDisable((int)cap);
    public static int CreateBuffer () => Create(glCreateBuffers);
    public static int CreateVertexArray () => Create(glCreateVertexArrays);
    public static int CreateFramebuffer () => Create(glCreateFramebuffers);
    public static int CreateRenderbuffer () => Create(glCreateRenderbuffers);
    public static void DepthFunc (DepthFunction function) => glDepthFunc((int)function);
    public static void Viewport (Vector2i location, Vector2i size) => glViewport(location.X, location.Y, size.X, size.Y);
    public static void Viewport (int x, int y, int w, int h) => glViewport(x, y, w, h);
    public static void DrawArrays (Primitive mode, int first, int count) => glDrawArrays((int)mode, first, count);
    public static void DeleteProgram (int program) => glDeleteProgram(program);
    public static int GetAttribLocation (int program, string name) => GetLocation(program, name, glGetAttribLocation);
    public static int GetUniformLocation (int program, string name) => GetLocation(program, name, glGetUniformLocation);
    public static void CompileShader (int s) => glCompileShader(s);
    public static int CreateProgram () => glCreateProgram();
    public static int CreateShader (ShaderType shaderType) => glCreateShader((int)shaderType);
    public static void DeleteShader (int shader) => glDeleteShader(shader);
    public static void DeleteTexture (int texture) => glDeleteTextures(1, &texture);
    public static void DeleteRenderbuffer (int i) => glDeleteRenderbuffers(1, &i);
    public static void DeleteVertexArray (int vao) => glDeleteVertexArrays(1, &vao);
    public static void EnableVertexArrayAttrib (int id, int i) => glEnableVertexArrayAttrib(id, i);
    public static void LinkProgram (int p) => glLinkProgram(p);
    public static void NamedRenderbufferStorage (int renderbuffer, RenderbufferFormat format, int width, int height) => glNamedRenderbufferStorage(renderbuffer, (int)format, width, height);
    //public static void Scissor (int x, int y, int width, int height) => Extensions.glScissor(x, y, width, height);
    public static void TextureBaseLevel (int texture, int level) => glTextureParameteri(texture, Const.TEXTURE_BASE_LEVEL, level);
    public static void TextureFilter (int texture, MagFilter filter) => glTextureParameteri(texture, Const.TEXTURE_MAG_FILTER, (int)filter);
    public static void TextureFilter (int texture, MinFilter filter) => glTextureParameteri(texture, Const.TEXTURE_MIN_FILTER, (int)filter);
    public static void TextureMaxLevel (int texture, int level) => glTextureParameteri(texture, Const.TEXTURE_MAX_LEVEL, level);
    public static void TextureStorage2D (int texture, int levels, TextureFormat sizedFormat, int width, int height) => glTextureStorage2D(texture, levels, (int)sizedFormat, width, height);
    public static void TextureSubImage2D (int texture, int level, int xOffset, int yOffset, int width, int height, PixelFormat format, int type, void* pixels) => glTextureSubImage2D(texture, level, xOffset, yOffset, width, height, (int)format, type, pixels);
    public static void TextureWrap (int texture, WrapCoordinate c, Wrap w) => glTextureParameteri(texture, (int)c, (int)w);
    public static void Uniform (int uniform, float f) => glUniform1f(uniform, f);
    public static void Uniform (int uniform, int i) => glUniform1i(uniform, i);
    public static void Uniform (int uniform, Matrix4x4 m) {
        var p = &m;
        // god have mercy on our souls
        glUniformMatrix4fv(uniform, 1, 0, (float*)p);
    }

    public static void Uniform (int uniform, Vector2 v) => glUniform2f(uniform, v.X, v.Y);
    public static void Uniform (int uniform, Vector2i v) => glUniform2i(uniform, v.X, v.Y);
    public static void Uniform (int uniform, Vector4 v) => glUniform4f(uniform, v.X, v.Y, v.Z, v.W);
    public static void VertexAttribDivisor (int index, int divisor) => glVertexAttribDivisor(index, divisor);
    public static void VertexAttribPointer (int index, int size, AttribType type, bool normalized, int stride, long ptr) => glVertexAttribPointer(index, size, (int)type, normalized ? (byte)1 : (byte)0, stride, (void*)ptr);
    public static void DeleteFramebuffer (int id) => glDeleteFramebuffers(1, &id);
    public static void BindBuffer (BufferTarget target, int buffer) => glBindBuffer((int)target, buffer);
    public static void ActiveTexture (int i) {
        if (i != GetIntegerv(IntParameter.ActiveTexture) - Const.TEXTURE0)
            glActiveTexture(Const.TEXTURE0 + i);
    }

    public static void BindTexture (int type, int id) => glBindTexture(type, id);
    public static int CreateTexture2D () {
        int i;
        glCreateTextures(Const.TEXTURE_2D, 1, &i);
        return i;
    }

    public static void VertexAttribIPointer (int index, int size, AttribType type, int stride, long ptr) => glVertexAttribIPointer(index, size, (int)type, stride, (void*)ptr);
    public static FramebufferStatus CheckNamedFramebufferStatus (int id, FramebufferTarget target) => (FramebufferStatus)glCheckNamedFramebufferStatus(id, (int)target);
    public static void NamedFramebufferTexture (int id, FramebufferAttachment attachment, Sampler2D texture) => glNamedFramebufferTexture(id, (int)attachment, (int)texture, 0);
    public static void NamedFramebufferRenderbuffer (int framebuffer, FramebufferAttachment attachment, int renderbuffer) => glNamedFramebufferRenderbuffer(framebuffer, (int)attachment, Const.RENDERBUFFER, renderbuffer);
    public static GlErrorCode GetError () => (GlErrorCode)glGetError();
    public static void NamedBufferStorage (int buffer, int size, nint data, int flags) => glNamedBufferStorage(buffer, size, (void*)data, flags);
    public static void NamedBufferSubData (int buffer, int offset, int size, void* data) => glNamedBufferSubData(buffer, offset, size, data);
    public static void DeleteBuffer (int id) => glDeleteBuffers(1, &id);

    public static void ShaderSource (int id, string source) {
        var bytes = new byte[source.Length + 1];
        var l = Encoding.ASCII.GetBytes(source, bytes);
        if (source.Length != l)
            throw new Exception($"expected {source.Length} characters, not {l}");
        bytes[source.Length] = 0;
        fixed (byte* strPtr = bytes)
            glShaderSource(id, 1, &strPtr, null);
    }

    public static string GetShaderInfoLog (int id) {
        int actualLogLength = 0;
        glGetShaderiv(id, (int)ShaderParameter.InfoLogLength, &actualLogLength);
        var bufferLength = Maths.IntMin(1024, actualLogLength);
        Span<byte> bytes = stackalloc byte[bufferLength];
        fixed (byte* p = bytes)
            glGetShaderInfoLog(id, bufferLength, null, p);
        return Encoding.ASCII.GetString(bytes);
    }

    public static string GetProgramInfoLog (int id) {
        int actualLogLength = 0;
        glGetProgramiv(id, (int)ProgramParameter.InfoLogLength, &actualLogLength);
        var bufferLength = Maths.IntMin(1024, actualLogLength);
        Span<byte> bytes = stackalloc byte[bufferLength];
        fixed (byte* p = bytes)
            glGetProgramInfoLog(id, bufferLength, null, p);
        return Encoding.ASCII.GetString(bytes);
    }

    private static int GetLocation (int program, string name, delegate* unmanaged[Stdcall]<int, byte*, int> f) {
        Span<byte> bytes = name.Length < 1024 ? stackalloc byte[name.Length + 1] : new byte[name.Length + 1];
        var l = Encoding.ASCII.GetBytes(name, bytes);
        if (l != name.Length)
            throw new Exception($"expected {name.Length} characters, not {l}");
        bytes[name.Length] = 0;
        fixed (byte* p = bytes)
            return f(program, p);
    }

    public static (int size, AttribType type, string name) GetActiveAttrib (int id, int index) {
        var maxLength = GetProgram(id, ProgramParameter.ActiveAttributeMaxLength);
        int length, size, type;
        Span<byte> bytes = stackalloc byte[maxLength];
        fixed (byte* p = bytes)
            glGetActiveAttrib(id, index, maxLength, &length, &size, &type, p);
        var n = length > 0 ? Encoding.ASCII.GetString(bytes.Slice(0, length)) : "";
        return (size, (AttribType)type, n);
    }

    public static (int size, UniformType type, string name) GetActiveUniform (int id, int index) {
        var maxLength = GetProgram(id, ProgramParameter.ActiveUniformMaxLength);
        int length, size, type;
        Span<byte> bytes = stackalloc byte[maxLength];
        fixed (byte* p = bytes)
            glGetActiveUniform(id, index, maxLength, &length, &size, &type, p);
        var n = length > 0 ? Encoding.ASCII.GetString(bytes.Slice(0, length)) : "";
        return (size, (UniformType)type, n);
    }


    public static int GetProgram (int id, ProgramParameter p) {
        int i;
        glGetProgramiv(id, (int)p, &i);
        return i;
    }

    public static int GetShader (int id, ShaderParameter p) {
        int i;
        glGetShaderiv(id, (int)p, &i);
        return i;
    }

    public static void BindVertexArray (VertexArray value) {
        if (value != GetIntegerv(IntParameter.VertexArrayBinding)) {
            glBindVertexArray(value);
            if (value != GetIntegerv(IntParameter.VertexArrayBinding))
                throw new GlException(SetInt32Failed(nameof(IntParameter.VertexArrayBinding), value));
        }
    }

    private static string SetBoolFailed (string name, bool value) =>
        $"failed to turn {name} {(value ? "on" : "off")}";

    private static string SetInt32Failed (string name, int value) =>
        $"failed to set {name} to {value}";

    private static string SetEnumFailed<T> (T value) where T : Enum =>
        $"failed to set {typeof(T)} to {value}";

    private static int GetIntegerv (IntParameter p) {
        int i;
        glGetIntegerv((int)p, &i);
        return i;
    }

    private static int Create (delegate* unmanaged[Stdcall]<int, int*, void> f) {
        int i;
        f(1, &i);
        return i;
    }

    public static void SetSwapInterval (int value) {
        if (value != wglGetSwapIntervalEXT()) {
            if (0 == wglSwapIntervalEXT(value))
                throw new GlException(SetInt32Failed(nameof(wglSwapIntervalEXT), value));
            if (value != wglGetSwapIntervalEXT())
                throw new GlException(SetInt32Failed(nameof(wglSwapIntervalEXT), value));
        }
    }

#pragma warning disable IDE0044 // Make fields readonly
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CS0649
#pragma warning disable CS0169 // Remove unused private members
    //[GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, nint> glGetString;
    [GlVersion(1, 0)] private static delegate* unmanaged[Stdcall]<int, int> wglSwapIntervalEXT;
    [GlVersion(1, 0)] private static delegate* unmanaged[Stdcall]<int> wglGetSwapIntervalEXT;

    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<byte, byte, byte, byte, void> glColorMask;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<byte, void> glDepthMask;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<double, double, void> glDepthRange;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<double, void> glClearDepth;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<float, byte, void> glSampleCoverage;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<float, float, float, float, void> glBlendColor;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<float, float, float, float, void> glClearColor;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<float, float, void> glPolygonOffset;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<float, void> glLineWidth;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<float, void> glPointSize;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, byte*, int> glGetAttribLocation;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, byte*, int> glGetUniformLocation;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, byte*, void> glGetBooleanv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, byte*, void> glVertexAttrib4Nubv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, byte*, void> glVertexAttrib4ubv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, byte, byte, byte, byte, void> glVertexAttrib4Nub;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, byte> glIsBuffer;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, byte> glIsEnabled;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, byte> glIsProgram;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, byte> glIsQuery;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, byte> glIsShader;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, byte> glIsTexture;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, byte> glUnmapBuffer;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, double*, void> glGetDoublev;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, double*, void> glVertexAttrib1dv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, double*, void> glVertexAttrib2dv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, double*, void> glVertexAttrib3dv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, double*, void> glVertexAttrib4dv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, double, double, double, double, void> glVertexAttrib4d;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, double, double, double, void> glVertexAttrib3d;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, double, double, void> glVertexAttrib2d;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, double, void> glVertexAttrib1d;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, float*, void> glGetFloatv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, float*, void> glPointParameterfv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, float*, void> glVertexAttrib1fv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, float*, void> glVertexAttrib2fv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, float*, void> glVertexAttrib3fv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, float*, void> glVertexAttrib4fv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, float, float, float, float, void> glUniform4f;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, float, float, float, float, void> glVertexAttrib4f;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, float, float, float, void> glUniform3f;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, float, float, float, void> glVertexAttrib3f;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, float, float, void> glUniform2f;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, float, float, void> glVertexAttrib2f;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, float, void> glPixelStoref;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, float, void> glPointParameterf;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, float, void> glUniform1f;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, float, void> glVertexAttrib1f;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int*, int*, int, void> glMultiDrawArrays;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int*, int, void**, int, void> glMultiDrawElements;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glDeleteBuffers;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glDeleteQueries;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glDeleteTextures;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glDrawBuffers;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glGenBuffers;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glGenQueries;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glGenTextures;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glGetIntegerv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glPointParameteriv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glVertexAttrib4iv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glVertexAttrib4Niv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glVertexAttrib4Nuiv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glVertexAttrib4uiv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, byte**, int*, void> glShaderSource;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, byte*, void> glBindAttribLocation;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, byte, float*, void> glUniformMatrix2fv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, byte, float*, void> glUniformMatrix3fv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, byte, float*, void> glUniformMatrix4fv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, double*, void> glGetVertexAttribdv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, float*, void> glGetTexParameterfv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, float*, void> glGetUniformfv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, float*, void> glGetVertexAttribfv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, float*, void> glTexParameterfv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, float*, void> glUniform1fv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, float*, void> glUniform2fv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, float*, void> glUniform3fv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, float*, void> glUniform4fv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, float, void> glTexParameterf;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, byte*, void> glGetProgramInfoLog;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, byte*, void> glGetShaderInfoLog;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, byte*, void> glGetShaderSource;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, int*, void> glGetAttachedShaders;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetBufferParameteriv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetProgramiv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetQueryiv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetQueryObjectiv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetQueryObjectuiv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetShaderiv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetTexParameteriv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetUniformiv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetVertexAttribiv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glTexParameteriv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glUniform1iv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glUniform2iv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glUniform3iv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glUniform4iv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, byte, int, void*, void> glVertexAttribPointer;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, float*, void> glGetTexLevelParameterfv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, int*, int*, byte*, void> glGetActiveAttrib;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, int*, int*, byte*, void> glGetActiveUniform;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glGetTexLevelParameteriv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, int, int, void*, void> glCompressedTexSubImage3D;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, int, int, void*, void> glTexSubImage3D;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, int, void*, void> glTexImage3D;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, int, void> glCopyTexSubImage3D;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, void*, void> glCompressedTexImage3D;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, void*, void> glCompressedTexSubImage2D;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, void*, void> glTexImage2D;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, void*, void> glTexSubImage2D;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, void> glCopyTexImage2D;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, void> glCopyTexSubImage2D;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, void*, void> glCompressedTexImage2D;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, void*, void> glTexImage1D;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, void> glCopyTexImage1D;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, void*, void> glCompressedTexImage1D;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, void*, void> glCompressedTexSubImage1D;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, void*, void> glReadPixels;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, void*, void> glTexSubImage1D;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, void> glCopyTexSubImage1D;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void*, void> glDrawRangeElements;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void> glUniform4i;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void*, void> glGetTexImage;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glBlendFuncSeparate;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glScissor;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glStencilFuncSeparate;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glStencilOpSeparate;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glUniform3i;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glViewport;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, void*, void> glDrawElements;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glDrawArrays;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glStencilFunc;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glStencilOp;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glTexParameteri;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glUniform2i;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, void**, void> glGetBufferPointerv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, void**, void> glGetVertexAttribPointerv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, void*, void> glGetCompressedTexImage;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, void*> glMapBuffer;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glAttachShader;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glBeginQuery;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glBindBuffer;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glBindTexture;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glBlendEquationSeparate;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glBlendFunc;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glDetachShader;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glHint;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glPixelStorei;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glPointParameteri;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glPolygonMode;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glStencilMaskSeparate;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glUniform1i;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, int> glCreateShader;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, nint, nint, void*, void> glBufferSubData;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, nint, nint, void*, void> glGetBufferSubData;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, nint, void*, int, void> glBufferData;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, sbyte*, void> glVertexAttrib4bv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, sbyte*, void> glVertexAttrib4Nbv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, short*, void> glVertexAttrib1sv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, short*, void> glVertexAttrib2sv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, short*, void> glVertexAttrib3sv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, short*, void> glVertexAttrib4Nsv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, short*, void> glVertexAttrib4sv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, short, short, short, short, void> glVertexAttrib4s;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, short, short, short, void> glVertexAttrib3s;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, short, short, void> glVertexAttrib2s;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, short, void> glVertexAttrib1s;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, ushort*, void> glVertexAttrib4Nusv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, ushort*, void> glVertexAttrib4usv;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glActiveTexture;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glBlendEquation;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glClear;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glClearStencil;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glCompileShader;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glCullFace;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glDeleteProgram;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glDeleteShader;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glDepthFunc;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glDisable;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glDisableVertexAttribArray;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glDrawBuffer;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glEnable;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glEnableVertexAttribArray;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glEndQuery;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glFrontFace;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glLinkProgram;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glLogicOp;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glReadBuffer;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glStencilMask;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glUseProgram;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glValidateProgram;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int> glCreateProgram;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int> glGetError;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<void> glFinish;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<void> glFlush;

    [GlVersion(2, 1)] private static delegate* unmanaged[Stdcall]<int, int, byte, float*, void> glUniformMatrix2x3fv;
    [GlVersion(2, 1)] private static delegate* unmanaged[Stdcall]<int, int, byte, float*, void> glUniformMatrix2x4fv;
    [GlVersion(2, 1)] private static delegate* unmanaged[Stdcall]<int, int, byte, float*, void> glUniformMatrix3x2fv;
    [GlVersion(2, 1)] private static delegate* unmanaged[Stdcall]<int, int, byte, float*, void> glUniformMatrix3x4fv;
    [GlVersion(2, 1)] private static delegate* unmanaged[Stdcall]<int, int, byte, float*, void> glUniformMatrix4x2fv;
    [GlVersion(2, 1)] private static delegate* unmanaged[Stdcall]<int, int, byte, float*, void> glUniformMatrix4x3fv;

    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, byte*, int> glGetFragDataLocation;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, byte*, void> glVertexAttribI4ubv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, byte, byte, byte, byte, void> glColorMaski;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, byte> glIsFramebuffer;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, byte> glIsRenderbuffer;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, byte> glIsVertexArray;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glDeleteFramebuffers;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glDeleteRenderbuffers;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glDeleteVertexArrays;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glGenFramebuffers;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glGenRenderbuffers;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glGenVertexArrays;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glVertexAttribI1iv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glVertexAttribI1uiv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glVertexAttribI2iv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glVertexAttribI2uiv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glVertexAttribI3iv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glVertexAttribI3uiv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glVertexAttribI4iv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glVertexAttribI4uiv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, byte**, int, void> glTransformFeedbackVaryings;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, byte*, void> glBindFragDataLocation;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, byte*, void> glGetBooleani_v;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, byte> glIsEnabledi;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, float*, void> glClearBufferfv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, float, int, void> glClearBufferfi;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glClearBufferiv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glClearBufferuiv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetIntegeri_v;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetRenderbufferParameteriv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetTexParameterIiv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetTexParameterIuiv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetUniformuiv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetVertexAttribIiv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetVertexAttribIuiv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glTexParameterIiv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glTexParameterIuiv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glUniform1uiv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glUniform2uiv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glUniform3uiv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glUniform4uiv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, int*, int*, byte*, void> glGetTransformFeedbackVarying;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glGetFramebufferAttachmentParameteriv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, int, int, void> glBlitFramebuffer;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, void> glFramebufferTexture3D;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void> glFramebufferTexture1D;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void> glFramebufferTexture2D;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void> glFramebufferTextureLayer;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void> glRenderbufferStorageMultisample;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void> glUniform4ui;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void> glVertexAttribI4i;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void> glVertexAttribI4ui;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void*, void> glVertexAttribIPointer;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glFramebufferRenderbuffer;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glRenderbufferStorage;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glUniform3ui;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glVertexAttribI3i;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glVertexAttribI3ui;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, nint, nint, void> glBindBufferRange;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glBindBufferBase;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glUniform2ui;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glVertexAttribI2i;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glVertexAttribI2ui;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, nint> glGetStringi;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glBeginConditionalRender;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glBindFramebuffer;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glBindRenderbuffer;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glClampColor;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glDisablei;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glEnablei;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glUniform1ui;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glVertexAttribI1i;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glVertexAttribI1ui;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, int> glCheckFramebufferStatus;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, nint, nint, int, void*> glMapBufferRange;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, nint, nint, void> glFlushMappedBufferRange;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, sbyte*, void> glVertexAttribI4bv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, short*, void> glVertexAttribI4sv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, ushort*, void> glVertexAttribI4usv;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, void> glBeginTransformFeedback;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, void> glBindVertexArray;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<int, void> glGenerateMipmap;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<void> glEndConditionalRender;
    [GlVersion(3, 0)] private static delegate* unmanaged[Stdcall]<void> glEndTransformFeedback;

    [GlVersion(3, 1)] private static delegate* unmanaged[Stdcall]<int, byte*, int> glGetUniformBlockIndex;
    [GlVersion(3, 1)] private static delegate* unmanaged[Stdcall]<int, int, byte**, int*, void> glGetUniformIndices;
    [GlVersion(3, 1)] private static delegate* unmanaged[Stdcall]<int, int, int*, int, int*, void> glGetActiveUniformsiv;
    [GlVersion(3, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, byte*, void> glGetActiveUniformBlockName;
    [GlVersion(3, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, byte*, void> glGetActiveUniformName;
    [GlVersion(3, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glGetActiveUniformBlockiv;
    [GlVersion(3, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glDrawArraysInstanced;
    [GlVersion(3, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, void*, int, void> glDrawElementsInstanced;
    [GlVersion(3, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glTexBuffer;
    [GlVersion(3, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glUniformBlockBinding;
    [GlVersion(3, 1)] private static delegate* unmanaged[Stdcall]<int, int, nint, nint, nint, void> glCopyBufferSubData;
    [GlVersion(3, 1)] private static delegate* unmanaged[Stdcall]<int, void> glPrimitiveRestartIndex;

    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, byte*, int> glGetFragDataIndex;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int*, int, void**, int, int*, void> glMultiDrawElementsBaseVertex;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, float*, void> glGetMultisamplefv;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, float*, void> glGetSamplerParameterfv;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, float*, void> glSamplerParameterfv;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, float, void> glSamplerParameterf;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetSamplerParameterIiv;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetSamplerParameterIuiv;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetSamplerParameteriv;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glSamplerParameterIiv;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glSamplerParameterIuiv;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glSamplerParameteriv;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, int, byte*, void> glBindFragDataLocationIndexed;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, byte, void> glTexImage2DMultisample;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, byte, void> glTexImage3DMultisample;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void*, int, void> glDrawRangeElementsBaseVertex;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glFramebufferTexture;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, int, void*, int, int, void> glDrawElementsInstancedBaseVertex;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, int, void*, int, void> glDrawElementsBaseVertex;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glSamplerParameteri;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, long*, void> glGetBufferParameteri64v;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, long*, void> glGetInteger64i_v;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, long*, void> glGetQueryObjecti64v;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, nint> glFenceSync;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, ulong*, void> glGetQueryObjectui64v;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, void> glQueryCounter;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, int, void> glSampleMaski;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, long*, void> glGetInteger64v;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<int, void> glProvokingVertex;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<nint, byte> glIsSync;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<nint, int, int, int*, int*, void> glGetSynciv;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<nint, int, ulong, int> glClientWaitSync;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<nint, int, ulong, void> glWaitSync;
    [GlVersion(3, 2)] private static delegate* unmanaged[Stdcall]<nint, void> glDeleteSync;

    [GlVersion(3, 3)] private static delegate* unmanaged[Stdcall]<int, byte> glIsSampler;
    [GlVersion(3, 3)] private static delegate* unmanaged[Stdcall]<int, int*, void> glDeleteSamplers;
    [GlVersion(3, 3)] private static delegate* unmanaged[Stdcall]<int, int*, void> glGenSamplers;
    [GlVersion(3, 3)] private static delegate* unmanaged[Stdcall]<int, int, byte, int, void> glVertexAttribP1ui;
    [GlVersion(3, 3)] private static delegate* unmanaged[Stdcall]<int, int, byte, int, void> glVertexAttribP2ui;
    [GlVersion(3, 3)] private static delegate* unmanaged[Stdcall]<int, int, byte, int, void> glVertexAttribP3ui;
    [GlVersion(3, 3)] private static delegate* unmanaged[Stdcall]<int, int, byte, int, void> glVertexAttribP4ui;
    [GlVersion(3, 3)] private static delegate* unmanaged[Stdcall]<int, int, void> glBindSampler;
    [GlVersion(3, 3)] private static delegate* unmanaged[Stdcall]<int, int, void> glVertexAttribDivisor;

    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<float, void> glMinSampleShading;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, byte> glIsTransformFeedback;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, float*, void> glPatchParameterfv;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glDeleteTransformFeedbacks;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int*, void> glGenTransformFeedbacks;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int, byte*, int> glGetSubroutineIndex;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int, byte*, int> glGetSubroutineUniformLocation;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int, double*, void> glGetUniformdv;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetUniformSubroutineuiv;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glUniformSubroutinesuiv;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glGetProgramStageiv;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glGetQueryIndexediv;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int*, byte*, void> glGetActiveSubroutineName;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int*, byte*, void> glGetActiveSubroutineUniformName;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int*, void> glGetActiveSubroutineUniformiv;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void> glBlendFuncSeparatei;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glBeginQueryIndexed;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glBlendEquationSeparatei;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glBlendFunci;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glDrawTransformFeedbackStream;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int, void*, void> glDrawElementsIndirect;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glBindTransformFeedback;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glBlendEquationi;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glDrawTransformFeedback;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glEndQueryIndexed;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, int, void> glPatchParameteri;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<int, void*, void> glDrawArraysIndirect;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<void> glPauseTransformFeedback;
    [GlVersion(4, 0)] private static delegate* unmanaged[Stdcall]<void> glResumeTransformFeedback;

    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<float, float, void> glDepthRangef;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<float, void> glClearDepthf;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, byte> glIsProgramPipeline;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, double*, void> glVertexAttribL1dv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, double*, void> glVertexAttribL2dv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, double*, void> glVertexAttribL3dv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, double*, void> glVertexAttribL4dv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, double, double, double, double, void> glVertexAttribL4d;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, double, double, double, void> glVertexAttribL3d;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, double, double, void> glDepthRangeIndexed;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, double, double, void> glVertexAttribL2d;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, double, void> glVertexAttribL1d;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, float*, void> glViewportIndexedfv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, float, float, float, float, void> glViewportIndexedf;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int*, int, void*, int, void> glShaderBinary;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int*, void> glDeleteProgramPipelines;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int*, void> glGenProgramPipelines;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int*, void> glScissorIndexedv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, byte**, int> glCreateShaderProgramv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, double*, void> glDepthRangeArrayv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, double*, void> glGetDoublei_v;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, double*, void> glGetVertexAttribLdv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, float*, void> glGetFloati_v;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, float*, void> glViewportArrayv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, float, float, float, float, void> glProgramUniform4f;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, float, float, float, void> glProgramUniform3f;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, float, float, void> glProgramUniform2f;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, float, void> glProgramUniform1f;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int*, byte*, void> glGetProgramPipelineInfoLog;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int*, int*, void*, void> glGetProgramBinary;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int*, int*, void> glGetShaderPrecisionFormat;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetProgramPipelineiv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glScissorArrayv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, byte, float*, void> glProgramUniformMatrix2fv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, byte, float*, void> glProgramUniformMatrix2x3fv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, byte, float*, void> glProgramUniformMatrix2x4fv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, byte, float*, void> glProgramUniformMatrix3fv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, byte, float*, void> glProgramUniformMatrix3x2fv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, byte, float*, void> glProgramUniformMatrix3x4fv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, byte, float*, void> glProgramUniformMatrix4fv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, byte, float*, void> glProgramUniformMatrix4x2fv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, byte, float*, void> glProgramUniformMatrix4x3fv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, float*, void> glProgramUniform1fv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, float*, void> glProgramUniform2fv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, float*, void> glProgramUniform3fv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, float*, void> glProgramUniform4fv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glProgramUniform1iv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glProgramUniform1uiv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glProgramUniform2iv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glProgramUniform2uiv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glProgramUniform3iv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glProgramUniform3uiv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glProgramUniform4iv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glProgramUniform4uiv;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, void> glProgramUniform4i;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, void> glProgramUniform4ui;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void> glProgramUniform3i;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void> glProgramUniform3ui;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void> glScissorIndexed;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void*, void> glVertexAttribLPointer;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glProgramUniform2i;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glProgramUniform2ui;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glProgramParameteri;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glProgramUniform1i;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glProgramUniform1ui;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glUseProgramStages;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, void*, int, void> glProgramBinary;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, int, void> glActiveShaderProgram;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, void> glBindProgramPipeline;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<int, void> glValidateProgramPipeline;
    [GlVersion(4, 1)] private static delegate* unmanaged[Stdcall]<void> glReleaseShaderCompiler;

    [GlVersion(4, 2)] private static delegate* unmanaged[Stdcall]<int, int, int, byte, int, int, int, void> glBindImageTexture;
    [GlVersion(4, 2)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glGetActiveAtomicCounterBufferiv;
    [GlVersion(4, 2)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int*, void> glGetInternalformativ;
    [GlVersion(4, 2)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, void> glTexStorage3D;
    [GlVersion(4, 2)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void> glDrawArraysInstancedBaseInstance;
    [GlVersion(4, 2)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void> glTexStorage2D;
    [GlVersion(4, 2)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glDrawTransformFeedbackStreamInstanced;
    [GlVersion(4, 2)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glTexStorage1D;
    [GlVersion(4, 2)] private static delegate* unmanaged[Stdcall]<int, int, int, void*, int, int, int, void> glDrawElementsInstancedBaseVertexBaseInstance;
    [GlVersion(4, 2)] private static delegate* unmanaged[Stdcall]<int, int, int, void*, int, int, void> glDrawElementsInstancedBaseInstance;
    [GlVersion(4, 2)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glDrawTransformFeedbackInstanced;
    [GlVersion(4, 2)] private static delegate* unmanaged[Stdcall]<int, void> glMemoryBarrier;

    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<DebugProc, void*, void> glDebugMessageCallback;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, byte*, int> glGetProgramResourceIndex;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, byte*, int> glGetProgramResourceLocation;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, byte*, int> glGetProgramResourceLocationIndex;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int*, int*, int*, int*, int*, byte*, int> glGetDebugMessageLog;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int*, int, int, int, int, void> glInvalidateSubFramebuffer;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetFramebufferParameteriv;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glInvalidateFramebuffer;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int, byte*, void> glObjectLabel;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int, byte*, void> glPushDebugGroup;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int, byte, int, void> glVertexAttribFormat;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, byte*, void> glGetObjectLabel;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glGetProgramInterfaceiv;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int*, byte*, void> glGetProgramResourceName;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int*, byte, void> glDebugMessageControl;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int*, int, int*, int*, void> glGetProgramResourceiv;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, byte*, void> glDebugMessageInsert;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, byte, void> glTexStorage2DMultisample;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, byte, void> glTexStorage3DMultisample;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, void> glCopyImageSubData;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, void> glInvalidateTexSubImage;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, void> glTextureView;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int, int, long*, void> glGetInternalformati64v;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void*, void> glClearBufferData;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glVertexAttribIFormat;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glVertexAttribLFormat;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int, nint, nint, void> glTexBufferRange;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glDispatchCompute;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glFramebufferParameteri;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glShaderStorageBlockBinding;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, nint, int, void> glBindVertexBuffer;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, nint, nint, int, int, void*, void> glClearBufferSubData;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, void*, int, int, void> glMultiDrawElementsIndirect;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, void> glInvalidateTexImage;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, void> glVertexAttribBinding;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, int, void> glVertexBindingDivisor;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, nint, nint, void> glInvalidateBufferSubData;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, void**, void> glGetPointerv;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, void*, int, int, void> glMultiDrawArraysIndirect;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<int, void> glInvalidateBufferData;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<nint, void> glDispatchComputeIndirect;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<void*, int, byte*, void> glObjectPtrLabel;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<void*, int, int*, byte*, void> glGetObjectPtrLabel;
    [GlVersion(4, 3)] private static delegate* unmanaged[Stdcall]<void> glPopDebugGroup;
    [GlVersion(4, 4)] private static delegate* unmanaged[Stdcall]<int, int, int*, nint*, int*, void> glBindVertexBuffers;
    [GlVersion(4, 4)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glBindImageTextures;
    [GlVersion(4, 4)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glBindSamplers;
    [GlVersion(4, 4)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glBindTextures;
    [GlVersion(4, 4)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, nint*, nint*, void> glBindBuffersRange;
    [GlVersion(4, 4)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glBindBuffersBase;
    [GlVersion(4, 4)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, int, int, void*, void> glClearTexSubImage;
    [GlVersion(4, 4)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void*, void> glClearTexImage;
    [GlVersion(4, 4)] private static delegate* unmanaged[Stdcall]<int, nint, void*, int, void> glBufferStorage;

    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, byte> glUnmapNamedBuffer;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int*, void> glCreateBuffers;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int*, void> glCreateFramebuffers;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int*, void> glCreateProgramPipelines;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int*, void> glCreateRenderbuffers;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int*, void> glCreateSamplers;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int*, void> glCreateTransformFeedbacks;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int*, void> glCreateVertexArrays;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, float*, void> glGetTextureParameterfv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, float*, void> glTextureParameterfv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, float, void> glTextureParameterf;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int*, int, int, int, int, void> glInvalidateNamedFramebufferSubData;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glCreateQueries;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glCreateTextures;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetNamedBufferParameteriv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetNamedFramebufferParameteriv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetNamedRenderbufferParameteriv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetTextureParameterIiv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetTextureParameterIuiv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetTextureParameteriv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetTransformFeedbackiv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glGetVertexArrayiv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glInvalidateNamedFramebufferData;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glNamedFramebufferDrawBuffers;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glTextureParameterIiv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glTextureParameterIuiv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int*, void> glTextureParameteriv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, double*, void> glGetnUniformdv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, float*, void> glClearNamedFramebufferfv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, float*, void> glGetnUniformfv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, float*, void> glGetTextureLevelParameterfv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, float, int, void> glClearNamedFramebufferfi;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, nint*, int*, void> glVertexArrayVertexBuffers;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glClearNamedFramebufferiv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glClearNamedFramebufferuiv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glGetNamedFramebufferAttachmentParameteriv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glGetnUniformiv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glGetnUniformuiv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glGetTextureLevelParameteriv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glGetTransformFeedbacki_v;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int*, void> glGetVertexArrayIndexediv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, byte, int, void> glVertexArrayAttribFormat;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, byte, void> glTextureStorage2DMultisample;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, byte, void> glTextureStorage3DMultisample;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, int, int, int, int, void> glBlitNamedFramebuffer;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, int, int, int, void*, void> glGetTextureSubImage;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, int, int, void*, void> glCompressedTextureSubImage3D;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, int, int, void*, void> glTextureSubImage3D;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, int, void*, void> glGetCompressedTextureSubImage;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, int, void> glCopyTextureSubImage3D;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, void*, void> glCompressedTextureSubImage2D;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, void*, void> glTextureSubImage2D;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, int, void> glCopyTextureSubImage2D;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, int, void*, void> glReadnPixels;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, void*, void> glCompressedTextureSubImage1D;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, void*, void> glTextureSubImage1D;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, void> glCopyTextureSubImage1D;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, int, void> glTextureStorage3D;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void*, void> glGetnTexImage;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void*, void> glGetTextureImage;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void> glNamedFramebufferTextureLayer;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void> glNamedRenderbufferStorageMultisample;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void> glTextureStorage2D;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void> glVertexArrayAttribIFormat;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, int, void> glVertexArrayAttribLFormat;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void*, void> glClearNamedBufferData;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glNamedFramebufferRenderbuffer;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glNamedFramebufferTexture;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glNamedRenderbufferStorage;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, int, void> glTextureStorage1D;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, long*, void> glGetVertexArrayIndexed64iv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, nint, int, void> glTextureBufferRange;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, nint, int, void> glTransformFeedbackBufferRange;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, nint, int, void> glVertexArrayVertexBuffer;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, nint, void> glGetQueryBufferObjecti64v;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, nint, void> glGetQueryBufferObjectiv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, nint, void> glGetQueryBufferObjectui64v;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, nint, void> glGetQueryBufferObjectuiv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, void*, void> glGetCompressedTextureImage;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, void*, void> glGetnCompressedTexImage;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glNamedFramebufferParameteri;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glTextureBuffer;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glTextureParameteri;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glTransformFeedbackBufferBase;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glVertexArrayAttribBinding;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int, void> glVertexArrayBindingDivisor;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, int> glCheckNamedFramebufferStatus;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, long*, void> glGetNamedBufferParameteri64v;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, nint, nint, int, int, void*, void> glClearNamedBufferSubData;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, nint, nint, nint, void> glCopyNamedBufferSubData;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, void**, void> glGetNamedBufferPointerv;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, void*> glMapNamedBuffer;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, void> glBindTextureUnit;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, void> glClipControl;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, void> glDisableVertexArrayAttrib;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, void> glEnableVertexArrayAttrib;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, void> glNamedFramebufferDrawBuffer;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, void> glNamedFramebufferReadBuffer;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, int, void> glVertexArrayElementBuffer;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, nint, nint, int, void*> glMapNamedBufferRange;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, nint, nint, void*, void> glGetNamedBufferSubData;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, nint, nint, void*, void> glNamedBufferSubData;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, nint, nint, void> glFlushMappedNamedBufferRange;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, nint, void*, int, void> glNamedBufferData;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, nint, void*, int, void> glNamedBufferStorage;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, void> glGenerateTextureMipmap;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int, void> glMemoryBarrierByRegion;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<int> glGetGraphicsResetStatus;
    [GlVersion(4, 5)] private static delegate* unmanaged[Stdcall]<void> glTextureBarrier;
#pragma warning restore IDE0044 // Make fields readonly
#pragma warning restore CS0169// Remove unused private members
#pragma warning restore CS0649
#pragma warning restore IDE0051 // Remove unused private members

    public GlContext (DeviceContext dc) : this(dc, ContextConfiguration.Default) { }

    public GlContext (DeviceContext dc, ContextConfiguration configuration) {
        if (0 != Opengl.wglGetCurrentContext())
            throw new InvalidOperationException("context already exists");
        SetPixelFormat(dc, configuration);
        var rc = Opengl.CreateContext(dc);
        Opengl.MakeCurrent((nint)dc, rc);
        var requestedVersion = configuration.Version ?? GetCurrentContextVersion().Version;
        List<int> attributes = new() {
            (int)ContextAttrib.MajorVersion,
            requestedVersion.Major,
            (int)ContextAttrib.MinorVersion,
            requestedVersion.Minor
        };

        if (configuration.Flags is ContextFlag flags) {
            attributes.Add((int)ContextAttrib.ContextFlags);
            attributes.Add((int)flags);
        }
        if (configuration.Profile is ProfileMask mask) {
            attributes.Add((int)ContextAttrib.ProfileMask);
            attributes.Add((int)mask);
        }
        attributes.Add(0);
        var handle = CreateContextAttribs(dc, attributes);
        try {
            if (0 == handle)
                throw new Exception(nameof(wglCreateContextAttribsARB));
            Opengl.MakeCurrent((nint)dc, handle);
        } catch (WinApiException) {
            if (!Opengl.wglDeleteContext(handle))
                Debug.WriteLine($"failed to make ARB context current, also failed to delete it");
            throw;
        } finally {
            if (!Opengl.wglDeleteContext(rc))
                Debug.WriteLine($"failed to delete temporary context");
        }

        var (actualVersion, profile) = GetCurrentContextVersion();
        if (actualVersion.Major != requestedVersion.Major || actualVersion.Minor != requestedVersion.Minor)
            throw new Exception($"requested {requestedVersion} got {actualVersion}");
        Debug.WriteLine($"{actualVersion}, {profile} ({Opengl.GetString(OpenglString.Version)}, {Opengl.GetString(OpenglString.Vendor)}, {Opengl.GetString(OpenglString.Renderer)})");

        const BindingFlags NonPublicStatic = BindingFlags.NonPublic | BindingFlags.Static;

        nint opengl32dll = 0;
        const string opengl32 = "opengl32.dll";
        if (!Kernel32.GetModuleHandleEx(2, opengl32, ref opengl32dll) || 0 == opengl32dll)
            throw new WinApiException($"failed to get handle of {opengl32}");

        foreach (var f in typeof(GlContext).GetFields(NonPublicStatic)) {
            if (f.GetCustomAttribute<GlVersionAttribute>() is GlVersionAttribute attr) {
                if (attr.MinimumVersion.Major <= actualVersion.Major && attr.MinimumVersion.Minor <= actualVersion.Minor) {
                    var extPtr = Opengl.GetProcAddress(f.Name);
                    if (0 != extPtr) {
                        f.SetValue(null, extPtr);
                    } else {
                        var glPtr = Kernel32.GetProcAddress(opengl32dll, f.Name);
                        if (0 != glPtr)
                            f.SetValue(null, glPtr);
                        else
                            Debug.WriteLine($"WARNING: driver is missing {f.Name}");
                    }
                }
            }
        }
    }

    private static nint CreateContextAttribs (DeviceContext dc, List<int> attributes) {
        var createContext = Marshal.GetDelegateForFunctionPointer<wglCreateContextAttribsARB>(Opengl.GetProcAddress(nameof(wglCreateContextAttribsARB)));
        var asArray = attributes.ToArray();
        fixed (int* p = asArray)
            return createContext((nint)dc, 0, p);
    }

    delegate nint wglCreateContextAttribsARB (nint a, nint b, int* c);

    public static (Version Version, ProfileMask Profile) GetCurrentContextVersion () {
        var str = Opengl.GetString(OpenglString.Version);
        var m = Regex.Match(str, @"^(\d+\.\d+(\.\d+)?) ((Core|Compatibility) )?");
        if (!m.Success)
            throw new Exception($"'{str}' does not begin with a valid version string");
        var version = Version.Parse(m.Groups[1].Value);
        var profile = m.Groups[4].Success && Enum.TryParse<ProfileMask>(m.Groups[4].Value, out var p) ? p : ProfileMask.Undefined;
        return (version, profile);
    }

    private static void SetPixelFormat (DeviceContext dc, ContextConfiguration configuration) {
        const PixelFlag RequiredFlags = PixelFlag.SupportOpengl | PixelFlag.DrawToWindow;
        const PixelFlag RejectedFlags = PixelFlag.GenericAccelerated | PixelFlag.GenericFormat;
        var requireDoubleBuffer = configuration.DoubleBuffer is bool _0 && _0 ? PixelFlag.DoubleBuffer : PixelFlag.None;
        var rejectDoubleBuffer = configuration.DoubleBuffer is bool _1 && !_1 ? PixelFlag.DoubleBuffer : PixelFlag.None;
        var requireComposited = configuration.Composited is bool _2 && _2 ? PixelFlag.SupportComposition : PixelFlag.None;
        var rejectComposited = configuration.Composited is bool _3 && !_3 ? PixelFlag.SupportComposition : PixelFlag.None;
        var (requireSwapMethod, rejectSwapMethod) = ForSwapMethod(configuration.SwapMethod);
        var required = RequiredFlags | requireDoubleBuffer | requireComposited | requireSwapMethod;
        var rejected = RejectedFlags | rejectDoubleBuffer | rejectComposited | rejectSwapMethod;
        var colorBits = configuration.ColorBits ?? 32;
        var depthBits = configuration.DepthBits ?? 24;
        PixelFormatDescriptor p = new();
        var count = Gdi32.GetPixelFormatCount(dc);
        for (var i = 1; i <= count; i++) {
            Gdi32.DescribePixelFormat(dc, i, ref p);
            if (colorBits == p.colorBits && depthBits <= p.depthBits && required == (p.flags & required) && 0 == (p.flags & rejected)) {
                Gdi32.SetPixelFormat(dc, i, ref p);
                return;
            }
        }
        throw new Exception("no pixel format found");
    }

    private static (PixelFlag require, PixelFlag reject) ForSwapMethod (SwapMethod? m) => m switch {
        SwapMethod.Copy => (PixelFlag.SwapCopy, PixelFlag.SwapExchange),
        SwapMethod.Swap => (PixelFlag.SwapExchange, PixelFlag.SwapCopy),
        SwapMethod.Undefined => (PixelFlag.None, PixelFlag.SwapExchange | PixelFlag.SwapCopy),
        _ => (PixelFlag.None, PixelFlag.None)
    };

    private bool disposed = false;
    public void Dispose () {
        if (disposed)
            return;
        disposed = true;
        var ctx = Opengl.wglGetCurrentContext();
        if (0 == ctx)
            throw new InvalidOperationException();
        if (!Opengl.wglDeleteContext(ctx))
            throw new WinApiException(nameof(Opengl.wglDeleteContext));
    }
}
