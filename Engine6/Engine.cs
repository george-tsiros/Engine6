namespace Engine;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Win32;

class Glfw {
    public void Stop () {
        var deleted = wglDeleteContext(RenderingContext);
        if (0 == deleted)
            Gl.Kernel.Win32Assert(false);
        RenderingContext = IntPtr.Zero;
        if (!Gl.User.DestroyWindow(WindowHandle))
            Gl.Kernel.Win32Assert(false);
        WindowHandle = IntPtr.Zero;
    }

    const string defaultClassName = "GLFW";
    public string ClassName { get; }
    internal Glfw (string classname = null) {
        ClassName = classname ?? defaultClassName;
        glfwInit();
    }
    long frameCount = 0;
    IntPtr MyProc (IntPtr hWnd, WinMessage msg, IntPtr w, IntPtr l) {
        //Debug.WriteLine(msg.ToString());
        switch (msg) {
            case WinMessage.Paint when IntPtr.Zero != DeviceContext && IntPtr.Zero != RenderingContext:
                frameCount++;
                var e = Gl.Opengl.GetError();
                Debug.Assert(0 == e);
                Gl.Opengl.ClearColor(0.5f, 0.5f, 0.5f, 1f);
                e = Gl.Opengl.GetError();
                Debug.Assert(0 == e);

                Gl.Opengl.Clear(Gl.BufferBit.Color | Gl.BufferBit.Depth);
                e = Gl.Opengl.GetError();
                Debug.Assert(0 == e);
                var eh = Gl.Gdi.SwapBuffers(DeviceContext);
                e = Gl.Opengl.GetError();
                Debug.Assert(0 == e);
                Gl.Kernel.Win32Assert(eh);
                return IntPtr.Zero;
            case WinMessage.KeyDown:
                Gl.User.PostQuitMessage(0);
                return IntPtr.Zero;
        }
        return Gl.User.DefWindowProcW(hWnd, msg, w, l);
    }
    void glfwInit () {
        glfwPlatformInit();
        glfwDefaultWindowHints();
    }
    public void glfwCreateWindow () {
        createNativeWindow();
        glfwInitWgl();
        glfwCreateContextWGL();
    }
    public IntPtr RenderingContext { get; private set; }
    public IntPtr DeviceContext { get; private set; }
    unsafe void glfwCreateContextWGL () {
        var pfd = PixelFormatDescriptor.Create();
        DeviceContext = Gl.User.GetDC(WindowHandle);
        if (IntPtr.Zero == DeviceContext)
            throw new GlException();
        int pixelFormatIndex = choosePixelFormat();
        if (0 == pixelFormatIndex)
            throw new GlException();
        var described = Gl.Gdi.DescribePixelFormat(DeviceContext, pixelFormatIndex, pfd.Ss, &pfd);
        if (0 == described)
            throw new GlException();
        var formatSet = Gl.Gdi.SetPixelFormat(DeviceContext, pixelFormatIndex, ref pfd);
        if (!formatSet)
            throw new GlException();
        if (!extensionSupportedWGL(GlExtensions.WGL_ARB_create_context))
            throw new GlException();
        if (!extensionSupportedWGL(GlExtensions.WGL_ARB_create_context_profile))
            throw new GlException();
        RenderingContext = wglCreateContext(DeviceContext);
       var madeCurrent= wglMakeCurrent(DeviceContext, RenderingContext);
        Debug.Assert(0 != madeCurrent);
    }
    private bool extensionSupportedWGL (GlExtensions extension) {
        return Array.Exists(extensions, e => string.Equals(e, extension.ToString(), StringComparison.OrdinalIgnoreCase));
    }
    unsafe private int choosePixelFormat () {
        var nativeCount = 0;
        var attribs = new int[40];
        int attrib = (int)ExtensionFlags.WGL_NUMBER_PIXEL_FORMATS_ARB;
        var attribCount = 0;
        if (wglGetPixelFormatAttribivARB is not null && extensionSupportedWGL(GlExtensions.WGL_ARB_pixel_format)) {
            if (!wglGetPixelFormatAttribivARB(DeviceContext, 1, 0, 1, &attrib, &nativeCount))
                throw new GlException();
            attribs[attribCount++] = (int)ExtensionFlags.WGL_SUPPORT_OPENGL_ARB;
            attribs[attribCount++] = (int)ExtensionFlags.WGL_DRAW_TO_WINDOW_ARB;
            attribs[attribCount++] = (int)ExtensionFlags.WGL_PIXEL_TYPE_ARB;
            attribs[attribCount++] = (int)ExtensionFlags.WGL_ACCELERATION_ARB;
            attribs[attribCount++] = (int)ExtensionFlags.WGL_RED_BITS_ARB;
            attribs[attribCount++] = (int)ExtensionFlags.WGL_RED_SHIFT_ARB;
            attribs[attribCount++] = (int)ExtensionFlags.WGL_GREEN_BITS_ARB;
            attribs[attribCount++] = (int)ExtensionFlags.WGL_GREEN_SHIFT_ARB;
            attribs[attribCount++] = (int)ExtensionFlags.WGL_BLUE_BITS_ARB;
            attribs[attribCount++] = (int)ExtensionFlags.WGL_BLUE_SHIFT_ARB;
            attribs[attribCount++] = (int)ExtensionFlags.WGL_ALPHA_BITS_ARB;
            attribs[attribCount++] = (int)ExtensionFlags.WGL_ALPHA_SHIFT_ARB;
            attribs[attribCount++] = (int)ExtensionFlags.WGL_DEPTH_BITS_ARB;
            attribs[attribCount++] = (int)ExtensionFlags.WGL_STENCIL_BITS_ARB;
            attribs[attribCount++] = (int)ExtensionFlags.WGL_ACCUM_BITS_ARB;
            attribs[attribCount++] = (int)ExtensionFlags.WGL_ACCUM_RED_BITS_ARB;
            attribs[attribCount++] = (int)ExtensionFlags.WGL_ACCUM_GREEN_BITS_ARB;
            attribs[attribCount++] = (int)ExtensionFlags.WGL_ACCUM_BLUE_BITS_ARB;
            attribs[attribCount++] = (int)ExtensionFlags.WGL_ACCUM_ALPHA_BITS_ARB;
            attribs[attribCount++] = (int)ExtensionFlags.WGL_AUX_BUFFERS_ARB;
            attribs[attribCount++] = (int)ExtensionFlags.WGL_STEREO_ARB;
            attribs[attribCount++] = (int)ExtensionFlags.WGL_DOUBLE_BUFFER_ARB;
            if (extensionSupportedWGL(GlExtensions.WGL_ARB_multisample))
                attribs[attribCount++] = (int)ExtensionFlags.WGL_SAMPLES_ARB;
            if (extensionSupportedWGL(GlExtensions.WGL_ARB_framebuffer_sRGB) || extensionSupportedWGL(GlExtensions.WGL_EXT_framebuffer_sRGB))
                attribs[attribCount++] = (int)ExtensionFlags.WGL_FRAMEBUFFER_SRGB_CAPABLE_ARB;
        } else {
            nativeCount = Gl.Gdi.DescribePixelFormat(DeviceContext, 1, PixelFormatDescriptor.Size, null);
        }
        //var pfds = new PixelFormatDescriptor[nativeCount];
        //for (var i = 0; i < nativeCount; i++) {
        //    pfds[i] = PixelFormatDescriptor.Create();
        //}
        //if (wglGetPixelFormatAttribivARB is not null && extensionSupportedWGL(GlExtensions.WGL_ARB_pixel_format)) {
        //    for (var i = 0; i < nativeCount; i++) {
        //        var pixelFormat = i + 1;
        //    }
        //} else { 
        var lastPixelFormatIndex = 0;
        for (var i = 0; i < nativeCount; i++) {
            var pixelFormat = i + 1;
            var pfd = PixelFormatDescriptor.Create();

            var eh = Gl.Gdi.DescribePixelFormat(DeviceContext, pixelFormat, PixelFormatDescriptor.Size, &pfd);
            if (0 == eh)
                continue;
            if (pfd.Flags.HasFlag(PixelFlags.GenericAccelerated) || pfd.Flags.HasFlag(PixelFlags.GenericFormat) && !pfd.Flags.HasFlag(PixelFlags.Stereo) && PixelFormatDescriptor.Typical(pfd)) { 
                Debug.WriteLine($"{pixelFormat}: {pfd}");
                lastPixelFormatIndex = pixelFormat;
            }
        }
        return lastPixelFormatIndex;
    }

    string[] extensions;
    void GetExtensionsString () {
        var ptr = GetExtensionsStringPtr();
        if (IntPtr.Zero == ptr)
            throw new GlException();
        if (Marshal.PtrToStringAnsi(ptr) is string str)
            extensions = str.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    static IntPtr GetExtensionsStringPtr () {
        if (wglGetExtensionsStringARB is not null)
            return wglGetExtensionsStringARB(wglGetCurrentDC());
        else if (wglGetExtensionsStringEXT is not null)
            return wglGetExtensionsStringEXT();
        else throw new GlException();
    }

    void glfwInitWgl () {
        wglInstance = Gl.Kernel.LoadLibraryA("opengl32.dll");
        Debug.Assert(wglInstance == new IntPtr(0x00007ffd73e50000));
        //0x00007ffd73e50000
        GetProc(ref wglCreateContext, nameof(wglCreateContext));
        GetProc(ref wglDeleteContext, nameof(wglDeleteContext));
        GetProc(ref wglGetProcAddress, nameof(wglGetProcAddress));
        GetProc(ref wglGetCurrentDC, nameof(wglGetCurrentDC));
        GetProc(ref wglGetCurrentContext, nameof(wglGetCurrentContext));
        GetProc(ref wglMakeCurrent, nameof(wglMakeCurrent));
        GetProc(ref wglShareLists, nameof(wglShareLists));
        var dc = Gl.User.GetDC(helperWindow);
        var pfd = PixelFormatDescriptor.Create();
        pfd.Flags = PixelFlags.DrawToWindow | PixelFlags.SupportOpengl | PixelFlags.DoubleBuffer;
        pfd.ClrBts = 24;
        var formatIndex = Gl.Gdi.ChoosePixelFormat(dc, ref pfd);
        var pfSet = Gl.Gdi.SetPixelFormat(dc, formatIndex, ref pfd);
        if (!pfSet)
            throw new GlException();
        var rc = wglCreateContext(dc);
        if (IntPtr.Zero == rc)
            throw new GlException();
        pdc = wglGetCurrentDC();
        prc = wglGetCurrentContext();
        var madeCurrent = wglMakeCurrent(dc, rc);
        if (0 == madeCurrent)
            throw new GlException();
        wglGetProc(ref wglGetExtensionsStringEXT, nameof(wglGetExtensionsStringEXT));
        wglGetProc(ref wglGetExtensionsStringARB, nameof(wglGetExtensionsStringARB));
        wglGetProc(ref wglCreateContextAttribsARB, nameof(wglCreateContextAttribsARB));
        wglGetProc(ref wglSwapIntervalEXT, nameof(wglSwapIntervalEXT));
        wglGetProc(ref wglGetPixelFormatAttribivARB, nameof(wglGetPixelFormatAttribivARB));
        GetExtensionsString();
        var madePCurrent = wglMakeCurrent(pdc, prc);
        var deleted = wglDeleteContext(rc);
    }
    IntPtr pdc;
    IntPtr prc;

    unsafe private static void wglGetProc<T> (ref T t, string name) where T : Delegate {
        Span<byte> bytes = stackalloc byte[name.Length + 1];
        var length = System.Text.Encoding.ASCII.GetBytes(name, bytes);
        Debug.Assert(length + 1 == bytes.Length);
        bytes[length] = 0;
        fixed (byte* p = bytes) {
            var ptr = wglGetProcAddress(p);
            t = IntPtr.Zero != ptr ? Marshal.GetDelegateForFunctionPointer<T>(ptr) : null;
        }
    }
    public IntPtr WindowHandle { get; private set; }
    void createNativeWindow () {
        var nullModuleHandle = Gl.Kernel.GetModuleHandleW(null);
        WindowHandle = Gl.User.CreateWindowExW(0x300, ClassName, "?", 0x6000000, 0, 0, 320, 240, IntPtr.Zero, IntPtr.Zero, nullModuleHandle, IntPtr.Zero);
        if (IntPtr.Zero == WindowHandle)
            throw new GlException();
    }
    ushort atom;
    void glfwPlatformInit () {
        glfwRegisterWindowClassWin32();
        createHelperWindow();
    }
    IntPtr helperWindow;
    void createHelperWindow () {
        var nullModuleHandle = Gl.Kernel.GetModuleHandleW(null);
        helperWindow = Gl.User.CreateWindowExW(0x300, ClassName, "?", 0x6000000, 0, 0, 1, 1, IntPtr.Zero, IntPtr.Zero, nullModuleHandle, IntPtr.Zero);
        if (IntPtr.Zero == helperWindow)
            throw new GlException();
        var m = new Message();
        while (Gl.User.PeekMessageW(ref m, helperWindow, 0, 0, 1)) {
            _ = Gl.User.TranslateMessage(ref m);
            _ = Gl.User.DispatchMessageW(ref m);
        }
    }
    void glfwRegisterWindowClassWin32 () {
        var wc = WindowClassExW.Create();
        wc.style = ClassStyle.HRedraw | ClassStyle.VRedraw | ClassStyle.OwnDc;
        wc.wndProc = MyProc;
        wc.hInstance = Gl.Kernel.GetModuleHandleW(null);
        wc.hCursor = IntPtr.Zero;
        wc.classname = ClassName;
        atom = Gl.User.RegisterClassExW(ref wc);
        if (0 == atom)
            throw new GlException();
    }
    void glfwDefaultWindowHints () { }
    delegate IntPtr IntPtr_IntPtr__IntPtr (IntPtr a);
    delegate int IntPtr__Int32 (IntPtr a);
    unsafe delegate IntPtr ByteP__IntPtr (byte* a);
    delegate IntPtr Void__IntPtr ();
    delegate int IntPtr_IntPtr__Int32 (IntPtr a, IntPtr b);
    delegate IntPtr IntPtr_IntPtr_ArrayInt32__IntPtr (IntPtr a, IntPtr b, int[] c);
    delegate IntPtr Int32__IntPtr (int a);
    private static IntPtr_IntPtr__IntPtr wglCreateContext;
    private static IntPtr__Int32 wglDeleteContext;
    private static ByteP__IntPtr wglGetProcAddress;
    private static Void__IntPtr wglGetCurrentDC;
    private static Void__IntPtr wglGetCurrentContext;
    private static IntPtr_IntPtr__Int32 wglMakeCurrent;
    private static IntPtr_IntPtr__Int32 wglShareLists;

    private static Void__IntPtr wglGetExtensionsStringEXT;
    private static IntPtr_IntPtr__IntPtr wglGetExtensionsStringARB;
    private static IntPtr_IntPtr_ArrayInt32__IntPtr wglCreateContextAttribsARB;
    private static Int32__IntPtr wglSwapIntervalEXT;
    [return: MarshalAs(UnmanagedType.Bool)]
    unsafe delegate bool IntPtr_Int32_Int32_UInt32_Int32P_Int32P__Boolean (IntPtr a, int b, int c, uint d, int* e, int* f);
    private static IntPtr_Int32_Int32_UInt32_Int32P_Int32P__Boolean wglGetPixelFormatAttribivARB;
    private static void GetProc<T> (ref T t, string name) where T : Delegate {
        var p = Gl.Kernel.GetProcAddress(wglInstance, name);
        Gl.Kernel.Win32Assert(p);
        t = Marshal.GetDelegateForFunctionPointer<T>(p);
        Debug.Assert(t is not null);
    }
    static IntPtr wglInstance;
}

class Engine {
    [STAThread]
    static void Main () {
        var glfw = new Glfw();
        glfw.glfwCreateWindow();
        _ = Gl.User.ShowWindow(glfw.WindowHandle, 10);
        Gl.Kernel.Win32Assert(Gl.User.UpdateWindow(glfw.WindowHandle));
        Message m = new();
        for (; ; ) {
            if (Gl.User.PeekMessageW(ref m, IntPtr.Zero, 0, 0, 0)) {
                var eh = Gl.User.GetMessageW(ref m, IntPtr.Zero, 0, 0);
                if (eh == new IntPtr(-1))
                    Environment.FailFast("?");
                if (eh == IntPtr.Zero)
                    break;
                _ = Gl.User.DispatchMessageW(ref m);
            }
        }
        glfw.Stop();
        //using var f = new TextureTest(PixelFormatDescriptor.Typical, 1024, 1024);
        //f.Run();
    }
}
