namespace Gl;

using System.Runtime.InteropServices;
using Win32;
using Common;

internal static class Opengl32 {
    private const string dll = nameof(Opengl32) + ".dll";
    [DllImport(dll, SetLastError = true)]
    private static extern nint wglCreateContext (nint dc);

    internal static nint CreateContext (DeviceContext dc) {
        var ctx = wglCreateContext((nint)dc);
        return 0 != ctx ? ctx : throw new WinApiException(nameof(wglCreateContext));
    }

    [DllImport(dll, SetLastError = true)]
    private static extern nint wglGetProcAddress (nint name);

    internal static nint GetProcAddress (string name) {
        using Ascii n = new(name);
        return wglGetProcAddress(n);
    }

    [DllImport(dll)]
    private static extern nint glGetString (int name);

    internal static string GetString (OpenglString name) => 
        Marshal.PtrToStringAnsi(glGetString((int)name));

    [DllImport(dll)]
    internal static extern nint wglGetCurrentDC ();

    [DllImport(dll, SetLastError = true)]
    internal static extern nint wglGetCurrentContext ();

    [DllImport(dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool wglMakeCurrent (nint dc, nint hglrc);

    internal static void MakeCurrent (nint dc, nint hglrc) {
        if (!wglMakeCurrent(dc, hglrc))
            throw new WinApiException(nameof(wglMakeCurrent));
    }

    [DllImport(dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal extern static bool wglDeleteContext (nint hglrc);
}
