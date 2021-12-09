﻿namespace Engine;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Win32;

class Glfw {
    const string ClassName = "myclass";
    private static IntPtr MyProc (IntPtr hWnd, WinMessage msg, IntPtr w, IntPtr l) {
        Debug.WriteLine(msg.ToString());
        return Gl.User.DefWindowProcW(hWnd, msg, w, l);
    }
    public static ushort glfwInit () {
        var atom = glfwPlatformInit();
        if (0 == atom)
            return 0;
        glfwDefaultWindowHints();
        return atom;
    }
    public static object glfwCreateWindow () {
        var window = createNativeWindow();
        if (window == IntPtr.Zero)
            return null;
        var wgl = glfwInitWgl();
        if (wgl is null)
            return null;
        var ctx = glfwCreateContextWGL();
        return ctx;
    }
    public static object glfwInitWgl (IntPtr helperWindowHandle) {
        var pfd = PixelFormatDescriptor.Create();
        pfd.Flags = PixelFlags.DrawToWindow | PixelFlags.SupportOpengl | PixelFlags.DoubleBuffer;
        pfd.ColorBits = 24;
        Gl.Gdi.ChoosePixelFormat(
    }
    public static IntPtr createNativeWindow () { 
        var nullModuleHandle = Gl.Kernel.GetModuleHandleW(null);
        return Gl.User.CreateWindowExW(0x300, ClassName, "?", 0x6000000, 0, 0, 1, 1, IntPtr.Zero, IntPtr.Zero, nullModuleHandle, IntPtr.Zero);
    }

    public static ushort glfwPlatformInit () {
        var atom = glfwRegisterWindowClassWin32();
        if (0 == atom)
            return 0;
        if (!createHelperWindow())
            return 0;
        return atom;
    }
    public static bool createHelperWindow () {
        var nullModuleHandle = Gl.Kernel.GetModuleHandleW(null);
        var helperWindow = Gl.User.CreateWindowExW(0x300, ClassName, "?", 0x6000000, 0, 0, 1, 1, IntPtr.Zero, IntPtr.Zero, nullModuleHandle, IntPtr.Zero);
        if (helperWindow == IntPtr.Zero)
            return false;
        var m = new Message();
        while (Gl.User.PeekMessageW(ref m, helperWindow, 0, 0, 1)) {
            _ = Gl.User.TranslateMessage(ref m);
            _ = Gl.User.DispatchMessageW(ref m);
        }
        return true;
    }
    public static ushort glfwRegisterWindowClassWin32 () {
        var wc = WindowClassExW.Create();
        wc.style = ClassStyle.HRedraw | ClassStyle.VRedraw | ClassStyle.OwnDc;
        wc.wndProc = MyProc;
        wc.hInstance = Gl.Kernel.GetModuleHandleW(null);
        wc.hCursor = IntPtr.Zero;
        wc.classname = "myclass";
        return Gl.User.RegisterClassExW(ref wc);
    }
    public static void glfwDefaultWindowHints () { }
}

class Engine {
    private static Func<IntPtr, IntPtr> wglCreateContext;
    private static Func<IntPtr, int> wglDeleteContext;
    private static Func<string, IntPtr> wglGetProcAddress;
    private static Func<IntPtr> wglGetCurrentDC;
    private static Func<IntPtr> wglGetCurrentContext;
    private static Func<IntPtr, IntPtr, int> wglMakeCurrent;
    private static Func<IntPtr, IntPtr, int> wglShareLists;
    private static void GetProc<T> (ref T t, string name) where T : Delegate {
        var p = Gl.Kernel.GetProcAddress(wglInstance, name);
        Gl.Kernel.Win32Assert(p);
        t = Marshal.GetDelegateForFunctionPointer<T>(p);
        Debug.Assert(t is not null);

    }
    private static IntPtr MyProc (IntPtr hWnd, WinMessage msg, IntPtr w, IntPtr l) {
        Debug.WriteLine(msg.ToString());
        return Gl.User.DefWindowProcW(hWnd, msg, w, l);
    }
    static IntPtr wglInstance;
    [STAThread]
    static void Main () {
        var wc = WindowClassExW.Create();
        wc.style = ClassStyle.HRedraw | ClassStyle.VRedraw | ClassStyle.OwnDc;
        wc.wndProc = MyProc;
        wc.hInstance = Gl.Kernel.GetModuleHandleW(null);
        wc.hCursor = IntPtr.Zero;
        wc.classname = "myclass";
        var atom = Gl.User.RegisterClassExW(ref wc);
        Gl.Kernel.Win32Assert(atom != 0);

        var nullModuleHandle = Gl.Kernel.GetModuleHandleW(null);
        var helperWindow = Gl.User.CreateWindowExW(0x300, wc.classname, "?", 0x02000000 | 0x04000000, 0, 0, 1, 1, IntPtr.Zero, IntPtr.Zero, nullModuleHandle, IntPtr.Zero);
        Gl.Kernel.Win32Assert(helperWindow);
        _ = Gl.User.ShowWindow(helperWindow, 0);
        var m = new Message();
        while (Gl.User.PeekMessageW(ref m, helperWindow, 0, 0, 1)) {
            _ = Gl.User.TranslateMessage(ref m);
            _ = Gl.User.DispatchMessageW(ref m);
        }
        Gl.User.DestroyWindow(helperWindow);
        Gl.Kernel.Win32Assert(Gl.User.UnregisterClassW(new IntPtr(atom), IntPtr.Zero));

        //wglInstance = Gl.Kernel.LoadLibraryA("opengl32.dll");
        //Gl.Kernel.Win32Assert(wglInstance);
        //GetProc(ref wglCreateContext, nameof(wglCreateContext));
        //GetProc(ref wglDeleteContext, nameof(wglDeleteContext));
        //GetProc(ref wglGetProcAddress, nameof(wglGetProcAddress));
        //GetProc(ref wglGetCurrentDC, nameof(wglGetCurrentDC));
        //GetProc(ref wglGetCurrentContext, nameof(wglGetCurrentContext));
        //GetProc(ref wglMakeCurrent, nameof(wglMakeCurrent));
        //GetProc(ref wglShareLists, nameof(wglShareLists));

        //using var f = new TextureTest(PixelFormatDescriptor.Typical, 1024, 1024);
        //f.Run();
    }
}
