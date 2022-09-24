namespace Gl;

using System.Runtime.InteropServices;
using Win32;
using Common;

internal static class Opengl {
    private const string opengl32 = nameof(opengl32) + ".dll";
    [DllImport(opengl32, SetLastError = true)]
    private static extern nint wglCreateContext (nint dc);

    internal static nint CreateContext (DeviceContext dc) {
        var ctx = wglCreateContext((nint)dc);
        return 0 != ctx ? ctx : throw new WinApiException(nameof(wglCreateContext));
    }

    [DllImport(opengl32, SetLastError = true)]
    private static extern nint wglGetProcAddress (nint name);

    internal static nint GetProcAddress (string name) {
        using Ascii n = new(name);
        return wglGetProcAddress(n.Handle);
    }

    [DllImport(opengl32)]
    private static extern nint glGetString (int name);

    internal static string GetString (OpenglString name) => 
        Marshal.PtrToStringAnsi(glGetString((int)name));

    [DllImport(opengl32)]
    internal static extern nint wglGetCurrentDC ();

    [DllImport(opengl32, SetLastError = true)]
    internal static extern nint wglGetCurrentContext ();

    [DllImport(opengl32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool wglMakeCurrent (nint dc, nint hglrc);

    internal static void MakeCurrent (nint dc, nint hglrc) {
        if (!wglMakeCurrent(dc, hglrc))
            throw new WinApiException(nameof(wglMakeCurrent));
    }

    [DllImport(opengl32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal extern static bool wglDeleteContext (nint hglrc);
}
