namespace Gl;

using System;
using Win32;
using System.Runtime.InteropServices;

public sealed class RenderingContext:IDisposable {
    private const string opengl32 = nameof(opengl32) + ".dll";

    [DllImport(opengl32)]
    private static extern uint glGetError ();
    
    [DllImport(opengl32, SetLastError = true)]
    private static extern nint wglCreateContext (nint dc);
    
    [DllImport(opengl32, SetLastError = true)]
    private static extern nint wglGetProcAddress (nint name);
    
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

    private readonly nint handle;

    public RenderingContext (DeviceContext dc, ContextConfiguration? configuration = null) {

    }
    private bool disposed = false;
    public void Dispose () {
        if (!disposed) {
            disposed = true;
        }
    }
}
