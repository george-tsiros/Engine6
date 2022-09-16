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


//public static unsafe class RenderingContext {

//    private const BindingFlags NonPublicStatic = BindingFlags.NonPublic | BindingFlags.Static;
//    private const PixelFlag RequiredFlags = PixelFlag.SupportOpengl | PixelFlag.DrawToWindow;
//    private const PixelFlag RejectedFlags = PixelFlag.GenericAccelerated | PixelFlag.GenericFormat;

//    private static DeviceContext Dc;
//    private static nint handle;


//    public static void Create (DeviceContext dc) =>
//        Create(dc, ContextConfiguration.Default);

//    public static void Close () {
//        NotDisposed();
//    }
//    delegate nint CreateContextARB (nint a, nint b, int* c);

//    private static CreateContextARB wglCreateContextAttribsARB;
//    //private readonly delegate* unmanaged[Stdcall]<nint, int, int, int, int*, int*, int> wglGetPixelFormatAttribivARB;
//    //public static ProfileMask Profile { get; private set; } = ProfileMask.Undefined;
//    public static (Version, ProfileMask) GetCurrentContextVersion () {
//        NotDisposed();
//        var str = Opengl.GetString(OpenglString.Version);
//        var m = Regex.Match(str, @"^(\d+\.\d+(\.\d+)?) ((Core|Compatibility) )?");
//        if (!m.Success)
//            throw new Exception($"'{str}' does not begin with a valid version string");
//        var version = Version.Parse(m.Groups[1].Value);
//        var profile = m.Groups[4].Success && Enum.TryParse<ProfileMask>(m.Groups[4].Value, out var p) ? p : ProfileMask.Undefined;
//        return (version, profile);
//    }

//    //public static string ShaderVersionString { get; private set; }
//    // yes, it is quite janky for the time being. For all the reasons you can see and many more
//    public static void Create (DeviceContext dc, ContextConfiguration configuration) {

//        if (0 != handle || 0 != Opengl.wglGetCurrentContext())
//            throw new Exception("context already exists");

//        PixelFormatDescriptor descriptor = new();
//        var pfIndex = FindPixelFormat(dc, ref descriptor, configuration);
//        Gdi32.SetPixelFormat(dc, pfIndex, ref descriptor);

//        var rc = Opengl.CreateContext(dc);
//        Opengl.MakeCurrent((nint)dc, rc);

//        var requestedVersion = configuration.Version ?? GetCurrentContextVersion().Item1;

//        //var m = Regex.Match(VersionString, @"^(\d+\.\d+(\.\d+)?) ((Core|Compatibility) )?");
//        //if (!m.Success)
//        //    throw new Exception($"'{VersionString}' not a version string");


//        //ShaderVersionString = $"{ContextVersion.Major}{ContextVersion.Minor}0";

//        //if (m.Groups[4].Success && Enum.TryParse<ProfileMask>(m.Groups[3].Value, out var profileMask))
//        //    Profile = profileMask;
//        //else
//        //    Profile = ProfileMask.Undefined;
//        //wglGetPixelFormatAttribivARB = (delegate* unmanaged[Stdcall]<nint, int, int, int, int*, int*, int>)wglGetProcAddress((AnsiString)nameof(wglGetPixelFormatAttribivARB));
//        //if (wglGetPixelFormatAttribivARB is null)
//        //    throw new Exception($"failed to get {nameof(wglGetPixelFormatAttribivARB)}");
//        wglCreateContextAttribsARB = Marshal.GetDelegateForFunctionPointer<CreateContextARB>(Opengl.GetProcAddress(nameof(wglCreateContextAttribsARB)));

//        List<int> attributes = new() {
//            (int)ContextAttrib.MajorVersion,
//            requestedVersion.Major,
//            (int)ContextAttrib.MinorVersion,
//            requestedVersion.Minor
//        };

//        if (configuration.Flags is ContextFlag flags) {
//            attributes.Add((int)ContextAttrib.ContextFlags);
//            attributes.Add((int)flags);
//        }
//        if (configuration.Profile is ProfileMask mask) {
//            attributes.Add((int)ContextAttrib.ProfileMask);
//            attributes.Add((int)mask);
//        }
//        attributes.Add(0);

//        var asArray = attributes.ToArray();

//        fixed (int* p = asArray)
//            handle = wglCreateContextAttribsARB((nint)dc, 0, p);

//        try {
//            if (0 == handle)
//                throw new Exception(nameof(wglCreateContextAttribsARB));
//            Opengl.MakeCurrent((nint)dc, handle);

//        } catch (WinApiException) {
//            if (!Opengl.wglDeleteContext(handle))
//                Debug.WriteLine($"failed to make ARB context current, also failed to delete it");
//            handle = 0;
//            throw;
//        } finally {
//            if (!Opengl.wglDeleteContext(rc))
//                Debug.WriteLine($"failed to delete temporary context");
//        }

//        Dc = dc;

//        var (contextVersion, profile) = GetCurrentContextVersion();
//        if (contextVersion.Major != requestedVersion.Major || contextVersion.Minor != requestedVersion.Minor)
//            throw new Exception($"requested {requestedVersion} got {contextVersion}");
//        Debug.WriteLine($"{contextVersion}, {profile} ({Opengl.GetString(OpenglString.Version)}, {Opengl.GetString(OpenglString.Vendor)}, {Opengl.GetString(OpenglString.Renderer)})");
//        //Profile = profile;
//        const string opengl32 = "opengl32.dll";
//        foreach (var f in typeof(RenderingContext).GetFields(NonPublicStatic)) {
//            if (f.GetCustomAttribute<GlVersionAttribute>() is GlVersionAttribute attr) {
//                if (attr.MinimumVersion.Major <= contextVersion.Major && attr.MinimumVersion.Minor <= contextVersion.Minor) {
//                    var extPtr = Opengl.GetProcAddress(f.Name);
//                    if (0 != extPtr) {
//                        f.SetValue(null, extPtr);
//                        continue;
//                    }
//                    if (0 == opengl32dll)
//                        if (!Kernel32.GetModuleHandleEx(2, opengl32, ref opengl32dll) || 0 == opengl32dll)
//                            throw new WinApiException($"failed to get handle of {opengl32}");

//                    var glPtr = Kernel32.GetProcAddress(opengl32dll, f.Name);
//                    if (0 != glPtr)
//                        f.SetValue(null, glPtr);
//                    else
//                        Debug.WriteLine($"WARNING: driver is missing {f.Name}");
//                }
//            }
//        }

//        supportedExtensions = new();
//        if (LegacyOpenglVersion < contextVersion) {
//            int count = 0;
//            glGetIntegerv((int)IntParameter.NumExtensions, &count);
//            for (var i = 0; i < count; ++i) {
//                var p = glGetStringi((int)OpenglString.Extensions, i);
//                if (0 == p)
//                    throw new Exception($"failed to get ptr to extension string at index {i}");
//                supportedExtensions.Add(Marshal.PtrToStringAnsi(p));
//            }
//        } else {
//            supportedExtensions.AddRange(Opengl.GetString(OpenglString.Extensions).Split(' '));
//        }
//    }

//    private static List<string> supportedExtensions;

//    public static bool IsSupported (string extension) {
//        NotDisposed();
//        return supportedExtensions.Contains(extension);
//    }

//    public static IReadOnlyCollection<string> SupportedExtensions =>
//        supportedExtensions;

//    private static readonly Version LegacyOpenglVersion = new(3, 0, 0);
//    private static nint opengl32dll;

//    private static int FindPixelFormat (DeviceContext dc, ref PixelFormatDescriptor pfd, ContextConfiguration configuration) {
//        var formatCount = Gdi32.GetPixelFormatCount(dc);

//        var requireDoubleBuffer = configuration.DoubleBuffer is bool _0 && _0 ? PixelFlag.DoubleBuffer : PixelFlag.None;
//        var rejectDoubleBuffer = configuration.DoubleBuffer is bool _1 && !_1 ? PixelFlag.DoubleBuffer : PixelFlag.None;
//        var requireComposited = configuration.Composited is bool _2 && _2 ? PixelFlag.SupportComposition : PixelFlag.None;
//        var rejectComposited = configuration.Composited is bool _3 && !_3 ? PixelFlag.SupportComposition : PixelFlag.None;
//        var (requireSwapMethod, rejectSwapMethod) = configuration.SwapMethod switch {
//            SwapMethod.Copy => (PixelFlag.SwapCopy, PixelFlag.SwapExchange),
//            SwapMethod.Swap => (PixelFlag.SwapExchange, PixelFlag.SwapCopy),
//            SwapMethod.Undefined => (PixelFlag.None, PixelFlag.SwapExchange | PixelFlag.SwapCopy),
//            _ => (PixelFlag.None, PixelFlag.None)
//        };
//        var required = RequiredFlags | requireDoubleBuffer | requireComposited | requireSwapMethod;
//        var rejected = RejectedFlags | rejectDoubleBuffer | rejectComposited | rejectSwapMethod;
//        var colorBits = configuration.ColorBits ?? 32;
//        var depthBits = configuration.DepthBits ?? 24;
//        for (var i = 1; i <= formatCount; i++) {
//            Gdi32.DescribePixelFormat(dc, i, ref pfd);
//            if (colorBits == pfd.colorBits && depthBits <= pfd.depthBits && required == (pfd.flags & required) && 0 == (pfd.flags & rejected))
//                return i;
//        }

//        throw new Exception("no pixelformat found");
//    }

//    public static int CreateBuffer () =>
//        Create(glCreateBuffers);

//    public static int CreateVertexArray () =>
//        Create(glCreateVertexArrays);

//    public static int CreateFramebuffer () =>
//        Create(glCreateFramebuffers);

//    public static int CreateRenderbuffer () =>
//        Create(glCreateRenderbuffers);

//    private static int Create (delegate* unmanaged[Stdcall]<int, int*, void> f) {
//        NotDisposed();
//        int i;
//        f(1, &i);
//        return i;
//    }

//    private static void NotDisposed () {
//        if (0 == handle)
//            throw new InvalidOperationException();
//    }

//    public static void DeleteFramebuffer (int id) {
//        NotDisposed();
//        glDeleteFramebuffers(1, &id);
//    }

//    public static FramebufferStatus CheckNamedFramebufferStatus (int id, FramebufferTarget target) {
//        NotDisposed();
//        return (FramebufferStatus)glCheckNamedFramebufferStatus(id, (int)target);
//    }

//    public static void NamedFramebufferTexture (int id, FramebufferAttachment attachment, Sampler2D texture) {
//        NotDisposed();
//        glNamedFramebufferTexture(id, (int)attachment, (int)texture, 0);
//    }

//    public static void NamedFramebufferRenderbuffer (int framebuffer, FramebufferAttachment attachment, int renderbuffer) {
//        NotDisposed();
//        glNamedFramebufferRenderbuffer(framebuffer, (int)attachment, Const.RENDERBUFFER, renderbuffer);
//    }

//    public static GlErrorCode GetError () {
//        NotDisposed();
//        return (GlErrorCode)glGetError();
//    }

//    public static void NamedBufferStorage (int buffer, int size, nint data, int flags) {
//        NotDisposed();
//        glNamedBufferStorage(buffer, size, (void*)data, flags);
//    }

//    public static void NamedBufferSubData (int buffer, int offset, int size, void* data) {
//        NotDisposed();
//        glNamedBufferSubData(buffer, offset, size, data);
//    }

//    public static void DeleteBuffer (int id) {
//        NotDisposed();
//        glDeleteBuffers(1, &id);
//    }

//    public static void CompileShader (int s) {
//        NotDisposed();
//        glCompileShader(s);
//    }

//    public static int CreateProgram () {
//        NotDisposed();
//        return glCreateProgram();
//    }

//    public static int CreateShader (ShaderType shaderType) {
//        NotDisposed();
//        return glCreateShader((int)shaderType);
//    }

//    public static void DebugMessageCallback (DebugProc proc, void* userParam) {
//        NotDisposed();
//        glDebugMessageCallback(proc, userParam);
//    }

//    public static void DeleteProgram (int program) {
//        NotDisposed();
//        glDeleteProgram(program);
//    }

//    public static void DeleteShader (int shader) {
//        NotDisposed();
//        glDeleteShader(shader);
//    }

//    public static void DeleteTexture (int texture) {
//        NotDisposed();
//        glDeleteTextures(1, &texture);
//    }

//    public static void DeleteRenderbuffer (int i) {
//        NotDisposed();
//        glDeleteRenderbuffers(1, &i);
//    }

//    public static void DeleteVertexArray (int vao) {
//        NotDisposed();
//        glDeleteVertexArrays(1, &vao);
//    }

//    public static void DrawArrays (Primitive mode, int first, int count) {
//        NotDisposed();
//        glDrawArrays((int)mode, first, count);
//    }

//    public static void DrawArraysInstanced (Primitive mode, int firstIndex, int indicesPerInstance, int instancesCount) {
//        NotDisposed();
//        glDrawArraysInstanced((int)mode, firstIndex, indicesPerInstance, instancesCount);
//    }

//    public static void EnableVertexArrayAttrib (int id, int i) {
//        NotDisposed();
//        glEnableVertexArrayAttrib(id, i);
//    }

//    public static void LinkProgram (int p) {
//        NotDisposed();
//        glLinkProgram(p);
//    }

//    public static void NamedRenderbufferStorage (int renderbuffer, RenderbufferFormat format, int width, int height) {
//        NotDisposed();
//        glNamedRenderbufferStorage(renderbuffer, (int)format, width, height);
//    }
//    //public static void Scissor (int x, int y, int width, int height) => Extensions.glScissor(x, y, width, height);
//    public static void TextureBaseLevel (int texture, int level) {
//        NotDisposed();
//        glTextureParameteri(texture, Const.TEXTURE_BASE_LEVEL, level);
//    }

//    public static void TextureFilter (int texture, MagFilter filter) {
//        NotDisposed();
//        glTextureParameteri(texture, Const.TEXTURE_MAG_FILTER, (int)filter);
//    }

//    public static void TextureFilter (int texture, MinFilter filter) {
//        NotDisposed();
//        glTextureParameteri(texture, Const.TEXTURE_MIN_FILTER, (int)filter);
//    }

//    public static void TextureMaxLevel (int texture, int level) {
//        NotDisposed();
//        glTextureParameteri(texture, Const.TEXTURE_MAX_LEVEL, level);
//    }

//    public static void TextureStorage2D (int texture, int levels, TextureFormat sizedFormat, int width, int height) {
//        NotDisposed();
//        glTextureStorage2D(texture, levels, (int)sizedFormat, width, height);
//    }

//    public static void TextureSubImage2D (int texture, int level, int xOffset, int yOffset, int width, int height, PixelFormat format, int type, void* pixels) {
//        NotDisposed();
//        glTextureSubImage2D(texture, level, xOffset, yOffset, width, height, (int)format, type, pixels);
//    }

//    public static void TextureWrap (int texture, WrapCoordinate c, Wrap w) {
//        NotDisposed();
//        glTextureParameteri(texture, (int)c, (int)w);
//    }

//    public static void Uniform (int uniform, float f) {
//        NotDisposed();
//        glUniform1f(uniform, f);
//    }

//    public static void Uniform (int uniform, int i) {
//        NotDisposed();
//        glUniform1i(uniform, i);
//    }

//    public static void Uniform (int uniform, Matrix4x4 m) {
//        NotDisposed();
//        var p = &m;
//        // god have mercy on our souls
//        glUniformMatrix4fv(uniform, 1, 0, (float*)p);
//    }

//    public static void Uniform (int uniform, Vector2 v) {
//        NotDisposed();
//        glUniform2f(uniform, v.X, v.Y);
//    }

//    public static void Uniform (int uniform, Vector2i v) {
//        NotDisposed();
//        glUniform2i(uniform, v.X, v.Y);
//    }

//    public static void Uniform (int uniform, Vector4 v) {
//        NotDisposed();
//        glUniform4f(uniform, v.X, v.Y, v.Z, v.W);
//    }

//    public static void UseProgram (Program p) {
//        NotDisposed();
//        glUseProgram((int)p);
//    }

//    public static void VertexAttribDivisor (int index, int divisor) {
//        NotDisposed();
//        glVertexAttribDivisor(index, divisor);
//    }

//    public static void VertexAttribPointer (int index, int size, AttribType type, bool normalized, int stride, long ptr) {
//        NotDisposed();
//        glVertexAttribPointer(index, size, (int)type, normalized ? (byte)1 : (byte)0, stride, (void*)ptr);
//    }

//    public static void VertexAttribIPointer (int index, int size, AttribType type, int stride, long ptr) {
//        NotDisposed();
//        glVertexAttribIPointer(index, size, (int)type, stride, (void*)ptr);
//    }

//    //public static long GetTextureHandleARB (int texture) {
//    //NotDisposed();
//    //return glGetTextureHandleARB(texture);
//    //}
//    //public static void MakeTextureHandleResidentARB (long textureHandle) {
//    //NotDisposed();
//    //return glMakeTextureHandleResidentARB(textureHandle);
//    //}

//    public static void ClearColor (float r, float g, float b, float a) {
//        NotDisposed();
//        glClearColor(r, g, b, a);
//    }

//    public static void Clear (BufferBit what) {
//        NotDisposed();
//        glClear((int)what);
//    }

//    public static void BindTexture (int type, int id) {
//        NotDisposed();
//        glBindTexture(type, id);
//    }

//    public static void Enable (Capability cap) {
//        NotDisposed();
//        glEnable((int)cap);
//    }

//    public static void Disable (Capability cap) {
//        NotDisposed();
//        glDisable((int)cap);
//    }

//    public static bool IsEnabled (Capability cap) {
//        NotDisposed();
//        return 0 != glIsEnabled((int)cap);
//    }

//    public static void Viewport (Vector2i location, Vector2i size) =>
//        Viewport(location.X, location.Y, size.X, size.Y);

//    public static void Viewport (int x, int y, int w, int h) {
//        NotDisposed();
//        glViewport(x, y, w, h);
//    }

//    public static void Flush () {
//        NotDisposed();
//        glFlush();
//    }

//    public static void ShaderSource (int id, string source) {
//        NotDisposed();
//        var bytes = new byte[source.Length + 1];
//        var l = Encoding.ASCII.GetBytes(source, bytes);
//        if (source.Length != l)
//            throw new Exception($"expected {source.Length} characters, not {l}");
//        bytes[source.Length] = 0;
//        fixed (byte* strPtr = bytes)
//            glShaderSource(id, 1, &strPtr, null);
//    }

//    public static string GetShaderInfoLog (int id) {
//        NotDisposed();
//        int actualLogLength = 0;
//        glGetShaderiv(id, (int)ShaderParameter.InfoLogLength, &actualLogLength);
//        var bufferLength = Maths.IntMin(1024, actualLogLength);
//        Span<byte> bytes = stackalloc byte[bufferLength];
//        fixed (byte* p = bytes)
//            glGetShaderInfoLog(id, bufferLength, null, p);
//        return Encoding.ASCII.GetString(bytes);
//    }

//    public static string GetProgramInfoLog (int id) {
//        NotDisposed();
//        int actualLogLength = 0;
//        glGetProgramiv(id, (int)ProgramParameter.InfoLogLength, &actualLogLength);
//        var bufferLength = Maths.IntMin(1024, actualLogLength);
//        Span<byte> bytes = stackalloc byte[bufferLength];
//        fixed (byte* p = bytes)
//            glGetProgramInfoLog(id, bufferLength, null, p);
//        return Encoding.ASCII.GetString(bytes);
//    }

//    public static void AttachShader (int program, int shader) {
//        NotDisposed();
//        glAttachShader(program, shader);
//    }

//    public static void BindVertexArray (VertexArray value) {
//        NotDisposed();
//        if (value != GetIntegerv(IntParameter.VertexArrayBinding)) {
//            glBindVertexArray(value);
//            if (value != GetIntegerv(IntParameter.VertexArrayBinding))
//                throw new GlException(SetInt32Failed(nameof(IntParameter.VertexArrayBinding), value));
//        }
//    }

//    private static int GetIntegerv (IntParameter p) {
//        int i;
//        glGetIntegerv((int)p, &i);
//        return i;
//    }

//    private static string SetBoolFailed (string name, bool value) =>
//        $"failed to turn {name} {(value ? "on" : "off")}";

//    private static string SetInt32Failed (string name, int value) =>
//        $"failed to set {name} to {value}";

//    private static string SetEnumFailed<T> (T value) where T : Enum =>
//        $"failed to set {typeof(T)} to {value}";

//    public static void BindBuffer (BufferTarget target, int buffer) {
//        NotDisposed();
//        glBindBuffer((int)target, buffer);
//    }

//    public static void ActiveTexture (int i) {
//        NotDisposed();
//        if (i != GetIntegerv(IntParameter.ActiveTexture) - Const.TEXTURE0)
//            glActiveTexture(Const.TEXTURE0 + i);
//    }

//    public static int CreateTexture2D () {
//        NotDisposed();
//        int i;
//        glCreateTextures(Const.TEXTURE_2D, 1, &i);
//        return i;
//    }
//    private static int GetLocation (int program, string name, delegate* unmanaged[Stdcall]<int, byte*, int> f) {
//        Span<byte> bytes = name.Length < 1024 ? stackalloc byte[name.Length + 1] : new byte[name.Length + 1];
//        var l = Encoding.ASCII.GetBytes(name, bytes);
//        if (l != name.Length)
//            throw new Exception($"expected {name.Length} characters, not {l}");
//        bytes[name.Length] = 0;
//        fixed (byte* p = bytes)
//            return f(program, p);
//    }


//    public static int GetAttribLocation (int program, string name) {
//        NotDisposed();
//        return GetLocation(program, name, glGetAttribLocation);
//    }

//    public static int GetUniformLocation (int program, string name) {
//        NotDisposed();
//        return GetLocation(program, name, glGetUniformLocation);
//    }
//    public static int GetSwapInterval () {
//        NotDisposed();
//        return wglGetSwapIntervalEXT();
//    }

//    public static void SetSwapInterval (int value) {
//        NotDisposed();
//        if (value != wglGetSwapIntervalEXT()) {
//            if (0 == wglSwapIntervalEXT(value))
//                throw new GlException(SetInt32Failed(nameof(wglSwapIntervalEXT), value));
//            if (value != wglGetSwapIntervalEXT())
//                throw new GlException(SetInt32Failed(nameof(wglSwapIntervalEXT), value));
//        }
//    }
//    public static void NamedFramebufferDrawBuffer (int framebuffer, DrawBuffer attachment) {
//        NotDisposed();
//        glNamedFramebufferDrawBuffer(framebuffer, (int)attachment);
//    }

//    public static void NamedFramebufferDrawBuffers (int framebuffer, params DrawBuffer[] attachments) {
//        NotDisposed();
//        if (0 == attachments.Length)
//            throw new InvalidOperationException("must have at least one attachment");
//        fixed (DrawBuffer* db = attachments) {
//            int* p = (int*)db;
//            glNamedFramebufferDrawBuffers(framebuffer, attachments.Length, p);
//        }
//    }


//    public static void DepthFunc (DepthFunction function) {
//        NotDisposed();
//        glDepthFunc((int)function);
//    }

//    public static void BindDefaultFramebuffer () {
//        NotDisposed();
//        if (0 != GetIntegerv(IntParameter.FramebufferBinding)) {
//            glBindFramebuffer((int)FramebufferTarget.Framebuffer, 0);
//            if (0 != GetIntegerv(IntParameter.FramebufferBinding))
//                throw new GlException(SetInt32Failed(nameof(FramebufferTarget.Framebuffer), 0));
//        }
//    }

//    public static void BindFramebuffer (Framebuffer buffer) {
//        NotDisposed();
//        if (buffer != GetIntegerv(IntParameter.FramebufferBinding)) {
//            glBindFramebuffer((int)FramebufferTarget.Framebuffer, buffer);
//            if (buffer != GetIntegerv(IntParameter.FramebufferBinding))
//                throw new GlException(SetInt32Failed(nameof(FramebufferTarget.Framebuffer), buffer));
//        }
//    }

//    public static void NamedFramebufferReadBuffer (int framebuffer, DrawBuffer mode) {
//        NotDisposed();
//        glNamedFramebufferReadBuffer(framebuffer, (int)mode);
//    }

//    public static void ReadOnePixel (int x, int y, int width, int height, out uint pixel) {
//        NotDisposed();
//        fixed (uint* p = &pixel)
//            glReadnPixels(x, y, width, height, Const.RED_INTEGER, Const.INT, sizeof(uint), p);
//    }
//    public static int GetProgram (int id, ProgramParameter p) { // I wish I could join this with GetShader
//        NotDisposed();
//        int i;
//        glGetProgramiv(id, (int)p, &i);
//        return i;
//    }

//    public static int GetShader (int id, ShaderParameter p) {
//        NotDisposed();
//        int i;
//        glGetShaderiv(id, (int)p, &i);
//        return i;
//    }
//    public static (int size, AttribType type, string name) GetActiveAttrib (int id, int index) {
//        var maxLength = GetProgram(id, ProgramParameter.ActiveAttributeMaxLength);
//        int length, size, type;
//        Span<byte> bytes = stackalloc byte[maxLength];
//        fixed (byte* p = bytes)
//            glGetActiveAttrib(id, index, maxLength, &length, &size, &type, p);
//        var n = length > 0 ? Encoding.ASCII.GetString(bytes.Slice(0, length)) : "";
//        return (size, (AttribType)type, n);
//    }

//    public static (int size, UniformType type, string name) GetActiveUniform (int id, int index) {
//        var maxLength = GetProgram(id, ProgramParameter.ActiveUniformMaxLength);
//        int length, size, type;
//        Span<byte> bytes = stackalloc byte[maxLength];
//        fixed (byte* p = bytes)
//            glGetActiveUniform(id, index, maxLength, &length, &size, &type, p);
//        var n = length > 0 ? Encoding.ASCII.GetString(bytes.Slice(0, length)) : "";
//        return (size, (UniformType)type, n);
//    }



//#pragma warning disable IDE0044 // Make fields readonly
//#pragma warning disable IDE0051 // Remove unused private members
//#pragma warning disable CS0649
//#pragma warning disable CS0169 // Remove unused private members
//#pragma warning restore IDE0044 // Make fields readonly
//#pragma warning restore CS0169// Remove unused private members
//#pragma warning restore CS0649 
//#pragma warning restore IDE0051 // Remove unused private members
//}
