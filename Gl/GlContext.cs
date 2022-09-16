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

public sealed unsafe class GlContext:IDisposable {
    public static void ClearColor (float r, float g, float b, float a) =>
        glClearColor(r, g, b, a);

    public static void Clear (BufferBit mask) =>
        glClear((int)mask);

    public static void UseProgram (Program p) {
        glUseProgram((int)p);
    }

    public static void Enable (Capability cap) {
        glEnable((int)cap);
    }

    public static void Disable (Capability cap) {
        glDisable((int)cap);
    }
#pragma warning disable IDE0044 // Make fields readonly
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CS0649
#pragma warning disable CS0169 // Remove unused private members
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<float, float, float, float, void> glClearColor;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glClear;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glUseProgram;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glEnable;
    [GlVersion(2, 0)] private static delegate* unmanaged[Stdcall]<int, void> glDisable;
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
        const string opengl32 = "opengl32.dll";
        const BindingFlags NonPublicStatic = BindingFlags.NonPublic | BindingFlags.Static;
        foreach (var f in typeof(RenderingContext).GetFields(NonPublicStatic)) {
            if (f.GetCustomAttribute<GlVersionAttribute>() is GlVersionAttribute attr) {
                if (attr.MinimumVersion.Major <= actualVersion.Major && attr.MinimumVersion.Minor <= actualVersion.Minor) {
                    var extPtr = Opengl.GetProcAddress(f.Name);
                    if (0 != extPtr) {
                        f.SetValue(null, extPtr);
                    } else {
                        if (0 == opengl32dll)
                            if (!Kernel32.GetModuleHandleEx(2, opengl32, ref opengl32dll) || 0 == opengl32dll)
                                throw new WinApiException($"failed to get handle of {opengl32}");

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
    private static nint opengl32dll;
    private static nint CreateContextAttribs (DeviceContext dc, List<int> attributes) {
        var createContext = Marshal.GetDelegateForFunctionPointer<wglCreateContextAttribsARB>(Opengl.GetProcAddress(nameof(wglCreateContextAttribsARB)));
        var asArray = attributes.ToArray();
        fixed (int* p = asArray)
            return createContext((nint)dc, 0, p);
    }

    delegate nint wglCreateContextAttribsARB (nint a, nint b, int* c);

    //private static CreateContextARB wglCreateContextAttribsARB;

    private static (Version Version, ProfileMask Profile) GetCurrentContextVersion () {
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
