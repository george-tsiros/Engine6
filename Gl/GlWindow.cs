namespace Gl;

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

class ContextCreationException:Exception {
    public ContextCreationException (string message = null) : base(message ?? Kernel.GetLastError().ToString("x16")) { }
}

public class GlWindow:IDisposable {
    protected ulong FramesRendered { get; private set; }
    public IntPtr DeviceContext { get; }
    public IntPtr RenderingContext { get; }
    public IntPtr WindowHandle { get; }
    protected ushort ClassAtom { get; }
    public int Width { get; }
    public int Height { get; }
    private readonly DebugProc debugProc;
    private void MyDebugProc (int sourceEnum, int typeEnum, int id, int severityEnum, int length, IntPtr message, IntPtr userParam) {
        Debugger.Break();
    }
    public GlWindow (Predicate<PixelFormatDescriptor> isGood, int width, int height) {
        var windowClass = WindowClassExW.Create();
        windowClass.classname = "PlainWindow";
        windowClass.style = 3;
        windowClass.wndProc = MyProc;
        ClassAtom = User.RegisterClassExW(ref windowClass);
        Win32Assert(ClassAtom != 0);
        (Width, Height) = (width, height);
        var windowStyle = WindowStyle.Caption | WindowStyle.ClipChildren | WindowStyle.ClipSiblings;
        WindowHandle = User.CreateWindowExW(0, windowClass.classname, "egh", (int)windowStyle, 100, 1000, Width, Height, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        Win32Assert(WindowHandle != IntPtr.Zero);
        DeviceContext = User.GetDC(WindowHandle);
        Win32Assert(DeviceContext != IntPtr.Zero);
        RenderingContext = CreateContext(DeviceContext, isGood);
        Win32Assert(Opengl.MakeCurrent(DeviceContext, RenderingContext));
        debugProc = MyDebugProc;
        Opengl.DebugMessageCallback(debugProc, IntPtr.Zero);
    }
    protected virtual void Render (float dt) {
        Opengl.ClearColor(0.5f, 0.5f, 0.5f, 1f);
        Opengl.Clear(BufferBit.Color | BufferBit.Depth);
        Win32Assert(Gdi.SwapBuffers(DeviceContext));
    }
    private IntPtr MyProc (IntPtr hWnd, WinMessage msg, IntPtr wparam, IntPtr lparam) {
        switch (msg) {
            case WinMessage.KEYDOWN:
                //Debug.WriteLine($"{wparam:x16} {lparam:x16} {msg}");
                KeyDown(wparam.ToInt32());
                break;
            case WinMessage.KEYUP:
                break;
            case WinMessage.CLOSE:
                User.PostQuitMessage(0);
                break;
            case WinMessage.PAINT:
                Render(1f / 72f);
                ++FramesRendered;
                goto default;
            default:
                return User.DefWindowProcW(hWnd, msg, wparam, lparam);
        }
        return IntPtr.Zero;
    }
    protected virtual void KeyDown (int key) {
        switch (key) {
            case 0x1b:
                User.PostQuitMessage(0);
                break;
            case 0x70:
                if (User.GetCapture() == IntPtr.Zero)
                    _ = User.SetCapture(WindowHandle);
                else
                    Debug.WriteLine("other window has capture");
                break;
            case 0x71:
                if (User.GetCapture() == WindowHandle)
                    User.ReleaseCapture();
                else
                    Debug.WriteLine("did not have capture");
                break;
        }
    }
    protected virtual void Init () { }
    public void Run () {
        Init();
        _ = User.ShowWindow(WindowHandle, 10);
        Win32Assert(User.UpdateWindow(WindowHandle));
        Message m = new();
        while (User.GetMessageW(ref m, IntPtr.Zero, 0, 0)) {
            //_ = User.TranslateMessage(ref m);
            _ = User.DispatchMessageW(ref m);
        }
        Win32Assert(Opengl.MakeCurrent(IntPtr.Zero, IntPtr.Zero));
        Win32Assert(Opengl.DeleteContext(RenderingContext));
        Win32Assert(User.ReleaseDC(WindowHandle, DeviceContext));
    }
    private bool disposed;
    public void Dispose () {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    public void Dispose (bool dispose) {
        if (dispose && !disposed) {
            disposed = true;
            Closing();
            Win32Assert(User.DestroyWindow(WindowHandle));
            Win32Assert(User.UnregisterClassW((IntPtr)ClassAtom, IntPtr.Zero));
        }
    }
    protected virtual void Closing () { }
    private static void Win32Assert (bool b) {
        if (!b) {
            _ = User.MessageBox(IntPtr.Zero, Kernel.GetLastError().ToString("x16"), null, 0);
            Environment.Exit(-1);
        }
    }
    private static IntPtr CreateContext (IntPtr dc, Predicate<PixelFormatDescriptor> isGood) {
        var pfd = PixelFormatDescriptor.Create();
        try {
            var i = GetPixelFormat(dc, isGood, ref pfd);
            if (!Gdi.SetPixelFormat(dc, i, ref pfd))
                throw new ContextCreationException();
            var rc = Opengl.CreateContext(dc);
            return rc != IntPtr.Zero ? rc : throw new ContextCreationException();
        }
        catch (ContextCreationException e) {
            Debug.WriteLine(e.Message);
            _ = User.MessageBox(IntPtr.Zero, e.Message, null, 0);
            throw;
        }
    }
    private static int GetPixelFormat (IntPtr dc, Predicate<PixelFormatDescriptor> isGood, ref PixelFormatDescriptor pfd) {
        var pixelFormatIndex = 1;
        while (0 != Gdi.DescribePixelFormat(dc, pixelFormatIndex, pfd.Size, ref pfd)) {
            if (isGood(pfd))
                return pixelFormatIndex;
            pixelFormatIndex++;
        }
        throw new ContextCreationException("no pfd");
    }
}
