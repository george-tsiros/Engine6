namespace Gl;

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

class ContextCreationException:Exception {
    public ContextCreationException (string message = null) : base(message ?? Kernel.GetLastError().ToString("x16")) { }
}

public class GlWindow:IDisposable {
    protected ulong FramesRendered { get; private set; }
    public IntPtr DeviceContext { get; private set; }
    public IntPtr RenderingContext { get; private set; }
    public IntPtr WindowHandle { get; private set; }
    protected ushort ClassAtom { get; }
    public int Width { get; }
    public int Height { get; }
    private DebugProc debugProc;
    private long lastTicks;
    private bool disposed;
    private void MyDebugProc (int sourceEnum, int typeEnum, int id, int severityEnum, int length, IntPtr message, IntPtr userParam) {
        Debugger.Break();
    }
    private Predicate<PixelFormatDescriptor> IsGood { get; }
    public GlWindow (Predicate<PixelFormatDescriptor> isGood, int width, int height) {
        var windowClass = WindowClassExW.Create();
        windowClass.classname = "PlainWindow";
        windowClass.style = ClassStyle.VRedraw | ClassStyle.HRedraw | ClassStyle.OwnDc;
        windowClass.wndProc = MyProc;
        ClassAtom = User.RegisterClassExW(ref windowClass);
        Kernel.Win32Assert(ClassAtom != 0);
        (Width, Height) = (width, height);
        var windowStyle = WindowStyle.Caption | WindowStyle.ClipChildren | WindowStyle.ClipSiblings;
        IsGood = isGood;
        var maybeMyHandle = User.CreateWindowExW(0, windowClass.classname, "egh", (int)windowStyle, 100, 1000, Width, Height, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        Debug.Assert(maybeMyHandle == WindowHandle);
    }
    private void Paint () {
        long t0 = Stopwatch.GetTimestamp();
        var dticks = t0 - lastTicks;
        lastTicks = t0;
        var dt = dticks > 0 ? (float)((double)dticks / Stopwatch.Frequency) : 0f;
        Render(dt);
        Kernel.Win32Assert(Gdi.SwapBuffers(DeviceContext));
        ++FramesRendered;
    }
    protected virtual void Render (float dt) {
        Opengl.ClearColor(0.5f, 0.5f, 0.5f, 1f);
        Opengl.Clear(BufferBit.Color | BufferBit.Depth);
    }

    static string Foo (IntPtr p) => IntPtr.Size == 8 ? Foo(p.ToInt64()) : Foo(p.ToInt32());
    static string Foo (int p) => $"{p:x8}, {(p >> 16) & ushort.MaxValue}, {p & ushort.MaxValue}";
    static string Foo (long p) => $"{p:x16}, {(p >> 48) & ushort.MaxValue}, {(p >> 32) & ushort.MaxValue}, {(p >> 16) & ushort.MaxValue}, {p & ushort.MaxValue}";

    protected virtual void WindowPosChanging (IntPtr w, IntPtr l) => WriteLine(nameof(WindowPosChanging), w, l);
    protected virtual void Moving (Rect r) => Debug.WriteLine($"{nameof(Moving)} {r.left}, {r.top}, {r.right}, {r.bottom}");
    protected virtual void WindowPosChanged (WindowPos p) => Debug.WriteLine($"{nameof(WindowPosChanged)}: {p.x}, {p.y}, {p.cx}, {p.cy}");
    protected virtual void EraseBkgnd () => Debug.WriteLine(nameof(EraseBkgnd));
    protected virtual void Move (short x, short y) => WriteLine(nameof(Move), x, y);
    protected virtual void MouseMove (short x, short y) => WriteLine(nameof(MouseMove), x, y);
    protected virtual void MouseLeave () => Debug.WriteLine(nameof(MouseLeave));
    protected virtual void NCMouseLeave (IntPtr w, IntPtr l) => WriteLine(nameof(NCMouseLeave), w, l);
    protected virtual void CaptureChanged () => Debug.WriteLine(nameof(CaptureChanged));
    protected virtual void ExitSizeMove () => Debug.WriteLine(nameof(ExitSizeMove));
    protected virtual void EnterSizeMove () => Debug.WriteLine(nameof(EnterSizeMove));
    protected virtual void SetFocus () => Debug.WriteLine(nameof(SetFocus));
    protected virtual void KillFocus () => Debug.WriteLine(nameof(KillFocus));
    protected virtual void Size (SizeMessage m, int width, int height) => Debug.WriteLine($"{nameof(Size)}, {m}, {width} x {height}");
    protected virtual void KeyUp (Keys k) => Debug.WriteLine($"{nameof(KeyUp)}, {k}");

    static void WriteLine (string name, IntPtr w, IntPtr l) => Debug.WriteLine($"{name}: w {Foo(w)}, l {Foo(l)}");
    static void WriteLine (string name, int a, int b) => Debug.WriteLine($"{name}: {a}, {b}");

    protected virtual void KeyDown (Keys k) {
        Debug.WriteLine($"{nameof(KeyDown)}, {k}");
        switch (k) {
            case Keys.Escape:
                User.PostQuitMessage(0);
                break;
        }
    }
    private static (short x, short y) Split (IntPtr p) {
        var i = p.ToInt32();
        return ((short)(i & ushort.MaxValue), (short)((i >> 16) & ushort.MaxValue));
    }
    protected virtual void Load () { }
    protected virtual void Closing () { }

    public void Run () {
        Load();
        _ = User.ShowWindow(WindowHandle, 10);
        Kernel.Win32Assert(User.UpdateWindow(WindowHandle));
        Message m = new();
        var run = true;
        while (run) {
            if (User.PeekMessageW(ref m, IntPtr.Zero, 0, 0, 0)) {
                var eh = User.GetMessageW(ref m, IntPtr.Zero, 0, 0);
                if (eh == new IntPtr(-1))
                    Environment.FailFast(eh.ToString());
                if (eh == IntPtr.Zero)
                    run = false;
                _ = User.DispatchMessageW(ref m);
            }
            Paint();
        }
        Kernel.Win32Assert(Opengl.MakeCurrent(IntPtr.Zero, IntPtr.Zero));
        Kernel.Win32Assert(Opengl.DeleteContext(RenderingContext));
        Kernel.Win32Assert(User.ReleaseDC(WindowHandle, DeviceContext));
    }

    public void Dispose () {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Dispose (bool dispose) {
        if (dispose && !disposed) {
            disposed = true;
            Closing();
            Kernel.Win32Assert(User.DestroyWindow(WindowHandle));
            Kernel.Win32Assert(User.UnregisterClassW((IntPtr)ClassAtom, IntPtr.Zero));
        }
    }
    private void CreateContext () {
        Kernel.Win32Assert(WindowHandle != IntPtr.Zero);
        DeviceContext = User.GetDC(WindowHandle);
        Kernel.Win32Assert(DeviceContext != IntPtr.Zero);
        RenderingContext = Gdi.CreateContext(DeviceContext, IsGood);
        Kernel.Win32Assert(Opengl.MakeCurrent(DeviceContext, RenderingContext));
        //debugProc = MyDebugProc;
        //Opengl.DebugMessageCallback(debugProc, IntPtr.Zero);
        //RenderingContext = Opengl.CreateContextAttribs(DeviceContext, IntPtr.Zero, 4, 6, ContextFlags.Debug | ContextFlags.ForwardCompatible, ProfileMask.Core);
        Kernel.Win32Assert(Opengl.MakeCurrent(DeviceContext, RenderingContext));
        //Kernel.Win32Assert(Opengl.DeleteContext(tempContext));
        Debug.WriteLine(Marshal.PtrToStringAnsi(Opengl.GetString(OpenglString.Extensions)));
        Debug.WriteLine(Marshal.PtrToStringAnsi(Opengl.GetString(OpenglString.Renderer)));
        Debug.WriteLine(Marshal.PtrToStringAnsi(Opengl.GetString(OpenglString.Vendor)));
        Debug.WriteLine(Marshal.PtrToStringAnsi(Opengl.GetString(OpenglString.Version)));
    }
    private IntPtr MyProc (IntPtr hWnd, WinMessage msg, IntPtr w, IntPtr l) {
        switch (msg) {
            case WinMessage.Create:
                WindowHandle = hWnd;
                CreateContext();
                return IntPtr.Zero;
            case WinMessage.Moving:
                Moving(Marshal.PtrToStructure<Rect>(l));
                break;
            case WinMessage.WindowPosChanged:
                WindowPosChanged(Marshal.PtrToStructure<WindowPos>(l));
                break;
            case WinMessage.EraseBkgnd:
                EraseBkgnd();
                return (IntPtr)1;
            case WinMessage.Move: {
                    var (x, y) = Split(l);
                    Move(x, y);
                }
                break;
            case WinMessage.MouseLeave:
                MouseLeave();
                break;
            case WinMessage.MouseMove: {
                    var (x, y) = Split(l);
                    MouseMove(x, y);
                }
                break;
            case WinMessage.CaptureChanged:
                CaptureChanged();
                break;
            case WinMessage.EnterSizeMove:
                EnterSizeMove();
                break;
            case WinMessage.ExitSizeMove:
                ExitSizeMove();
                break;
            case WinMessage.SetFocus:
                SetFocus();
                break;
            case WinMessage.KillFocus:
                KillFocus();
                break;
            case WinMessage.Size: {
                    var i = w.ToInt32();
                    if (Enum.IsDefined(typeof(SizeMessage), i)) {
                        var (width, height) = Split(l);
                        Size((SizeMessage)i, width, height);
                    }
                }
                break;
            case WinMessage.KeyDown: {
                    var k = w.ToInt32();
                    var wasUp = 0 == (l.ToInt32() & (1 << 30));
                    if (wasUp && Enum.IsDefined(typeof(Keys), k)) {
                        KeyDown((Keys)k);
                        return IntPtr.Zero;
                    }
                }
                break;
            case WinMessage.KeyUp: {
                    var k = w.ToInt32();
                    if (Enum.IsDefined(typeof(Keys), k)) {
                        KeyUp((Keys)k);
                        return IntPtr.Zero;
                    }
                }
                break;
            case WinMessage.Close:
                User.PostQuitMessage(0);
                break;
            case WinMessage.Paint:
                Paint();
                return IntPtr.Zero;
        }
        return User.DefWindowProcW(hWnd, msg, w, l);
    }
}