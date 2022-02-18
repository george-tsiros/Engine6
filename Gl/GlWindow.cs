namespace Gl;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Win32;

public class GlWindow:IDisposable {
    protected static void Demand (bool condition, string message = null) {
        if (!condition) {
            var stackFrame = new StackFrame(1, true);
            var m = $">{stackFrame.GetFileName()}({stackFrame.GetFileLineNumber()},{stackFrame.GetFileColumnNumber()}): {message ?? "?"}";
            if (Debugger.IsAttached)
                throw new Exception(m);
            else
                Console.WriteLine(m);
        }
    }

    protected static IntPtr Demand (IntPtr p) {
        Demand(IntPtr.Zero != p);
        return p;
    }

    protected static int Demand (int p) {
        Demand(0 != p);
        return p;
    }

    protected const string ClassName = "MYWINDOWCLASS";
    protected ulong FramesRendered { get; private set; }
    public IntPtr DeviceContext { get; private set; }
    public IntPtr RenderingContext { get; private set; }
    public IntPtr WindowHandle { get; private set; }
    protected ushort ClassAtom { get; }
    public int Width { get; }
    public int Height { get; }
    private long lastTicks = long.MaxValue;
    private bool disposed;

    private static readonly IntPtr SelfHandle = Kernel.GetModuleHandleW(null);

    public GlWindow (Vector2i size) {
        selectorProc = new(SelectorProc);
        wndProcActual = new(WndProcActual);
        ClassAtom = User.RegisterWindowClass(selectorProc, SelfHandle, ClassName);
        WindowHandle = User.CreateWindow(ClassAtom, size, SelfHandle);

        (Width, Height) = size;
        DeviceContext = User.GetDC(WindowHandle);
        RenderingContext = Opengl.CreateSimpleContext(DeviceContext);
        State.DebugOutput = true;
        State.SwapInterval = -1;
    }

    private void Paint () {
        long t0 = Stopwatch.GetTimestamp();
        var dticks = t0 - lastTicks;
        lastTicks = t0;
        var dt = dticks > 0 ? (float)((double)dticks / Stopwatch.Frequency) : 0f;
        Render(dt);
        Demand(Gdi.SwapBuffers(DeviceContext));
        ++FramesRendered;
    }

    protected virtual void Render (float dt) {
        Opengl.glClearColor(0.5f, 0.5f, 0.5f, 1f);
        Opengl.glClear(BufferBit.Color | BufferBit.Depth);
    }

    private int CaptureMouse (bool capture) {
        return capture ? CaptureMouse(0, WindowHandle) : CaptureMouse(RawInputDevice.RemoveDevice, IntPtr.Zero);
    }
    private static unsafe int CaptureMouse (uint flags, IntPtr ptr) {
        RawInputDevice rid = new() { usagePage = 1, usage = 2, flags = flags, windowHandle = ptr };
        var structSize = System.Runtime.InteropServices.Marshal.SizeOf<RawInputDevice>();
        return User.RegisterRawInputDevices(&rid, 1, (uint)structSize);
    }

    static string Foo (IntPtr p) => Foo(p.ToInt64());
    static string Foo (int p) => $"{p:x8}, {(p >> 16) & ushort.MaxValue}, {p & ushort.MaxValue}";
    static string Foo (long p) => $"{p:x16}, {(p >> 48) & ushort.MaxValue}, {(p >> 32) & ushort.MaxValue}, {(p >> 16) & ushort.MaxValue}, {p & ushort.MaxValue}";

    protected virtual void WindowPosChanging (IntPtr w, IntPtr l) { }// => WriteLine(nameof(WindowPosChanging), w, l);
    protected virtual void Moving (Rect r) { }// => Debug.WriteLine($"{nameof(Moving)} {r.left}, {r.top}, {r.right}, {r.bottom}");
    protected virtual void WindowPosChanged (WindowPos p) { }// => Debug.WriteLine($"{nameof(WindowPosChanged)}: {p.x}, {p.y}, {p.cx}, {p.cy}");
    protected virtual void EraseBkgnd () { }// => Debug.WriteLine(nameof(EraseBkgnd));
    protected virtual void Move (short x, short y) { }// => WriteLine(nameof(Move), x, y);
    protected virtual void MouseMove (int x, int y) { }// => WriteLine(nameof(MouseMove), x, y);
    protected virtual void MouseEnter (bool entered) { }
    protected virtual void NCMouseLeave (IntPtr w, IntPtr l) { }// => WriteLine(nameof(NCMouseLeave), w, l);
    protected virtual void CaptureChanged () { }// => Debug.WriteLine(nameof(CaptureChanged));
    protected virtual void ExitSizeMove () { }// => Debug.WriteLine(nameof(ExitSizeMove));
    protected virtual void EnterSizeMove () { }// => Debug.WriteLine(nameof(EnterSizeMove));
    protected virtual void SetFocus () { }// => Debug.WriteLine(nameof(SetFocus));
    protected virtual void KillFocus () { }// => Debug.WriteLine(nameof(KillFocus));
    protected virtual void Size (SizeMessage m, int width, int height) { }// => Debug.WriteLine($"{nameof(Size)}, {m}, {width} x {height}");
    protected virtual void KeyUp (Keys k) { }// => Debug.WriteLine($"{nameof(KeyUp)}, {k}");

    static void WriteLine (string name, IntPtr w, IntPtr l) => Debug.WriteLine($"{name}: w {Foo(w)}, l {Foo(l)}");
    static void WriteLine (string name, int a, int b) => Debug.WriteLine($"{name}: {a}, {b}");

    protected virtual void KeyDown (Keys k) {
        switch (k) {
            case Keys.Escape:
                User.PostQuitMessage(0);
                break;
        }
    }
    private static (short x, short y) Split (IntPtr p) {
        var i = (int)(p.ToInt64() & int.MaxValue);
        return ((short)(i & ushort.MaxValue), (short)((i >> 16) & ushort.MaxValue));
    }
    protected bool IsForeground { get; private set; }
    protected virtual void Load () { }
    protected virtual void Closing () { }
    protected void SetText (string text) => Demand(User.SetWindowText(WindowHandle, text));
    unsafe public void Run () {
        Load();
        _ = User.ShowWindow(WindowHandle, 10);
        Demand(User.UpdateWindow(WindowHandle));
        Message m = new();
        for (; ; ) {
            if (User.PeekMessageW(ref m, IntPtr.Zero, 0, 0, PeekRemove.NoRemove)) {
                var eh = User.GetMessageW(ref m, IntPtr.Zero, 0, 0);
                if (eh == new IntPtr(-1))
                    Environment.FailFast(eh.ToString());
                if (eh == IntPtr.Zero)
                    break;
                _ = User.DispatchMessageW(ref m);
            }
            Demand(User.InvalidateRect(WindowHandle, null, IntPtr.Zero));
        }
        Demand(Opengl.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero));
        Demand(Opengl.wglDeleteContext(RenderingContext));
        Demand(User.ReleaseDC(WindowHandle, DeviceContext));
    }

    public void Dispose () {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Dispose (bool dispose) {
        if (dispose && !disposed) {
            disposed = true;
            _ = Demand(CaptureMouse(false));
            Closing();
            Demand(User.DestroyWindow(WindowHandle));
            Demand(User.UnregisterClassW(new IntPtr(ClassAtom), IntPtr.Zero));
        }
    }
    private readonly WndProc selectorProc;
    private IntPtr SelectorProc (IntPtr hWnd, WinMessage msg, IntPtr w, IntPtr l) => IntPtr.Zero != RenderingContext ? wndProcActual(hWnd, msg, w, l) : User.DefWindowProcW(hWnd, msg, w, l);
    private readonly WndProc wndProcActual;
    int lastx, lasty;

    private int virtualCursorX;
    private int virtualCursorY;
    private void MouseMoveInternal (int x, int y) {
        if (x == virtualCursorX && y == virtualCursorY)
            return;
        virtualCursorX = x;
        virtualCursorY = y;
        Debug.WriteLine($"{FramesRendered}: {x}, {y}");
        MouseMove(x, y);
    }
    private Rect windowRect;
    private bool cursorTracked;
    private IntPtr WndProcActual (IntPtr hWnd, WinMessage msg, IntPtr w, IntPtr l) {
        //Debug.WriteLine(msg);
        switch (msg) {
            case WinMessage.MouseMove: {
                    if (!IsForeground)
                        break;
                    var (x, y) = Split(l);
                    if (!cursorTracked) {
                        var tme = TrackMouseEvent.Create();
                        tme.flags = 2;
                        tme.track = WindowHandle;
                        _ = Demand(User.TrackMouseEvent(ref tme));
                        cursorTracked = true;
                        MouseEnter(true);
                    }
                    var dx = x - lastx;
                    var dy = y - lasty;
                    _ = User.GetWindowRect(WindowHandle, ref windowRect);
                    MouseMoveInternal( dx, dy);
                    _ = User.SetCursorPos(windowRect.left + windowRect.Width / 2, windowRect.top + windowRect.Height / 2);
                    lastx = x;
                    lasty = y;
                }
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
                    _ = User.GetWindowRect(WindowHandle, ref windowRect);
                    Move(x, y);
                }
                break;
            case WinMessage.MouseLeave:
                cursorTracked = false;
                MouseEnter(false);
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
                _ = CaptureMouse(IsForeground =true);
                SetFocus();
                break;
            case WinMessage.KillFocus:
                _ = CaptureMouse(IsForeground= false);
                KillFocus();
                break;
            case WinMessage.Size: {
                    var i = (int)(w.ToInt64() & int.MaxValue);
                    if (Enum.IsDefined(typeof(SizeMessage), i)) {
                        var (width, height) = Split(l);
                        Size((SizeMessage)i, width, height);
                    }
                }
                break;
            case WinMessage.KeyDown: {
                    var m = new KeyMessage(w, l);
                    if (m.WasDown)
                        break;
                    KeyDown(m.Key);
                    return IntPtr.Zero;
                }
            case WinMessage.KeyUp: {
                    var m = new KeyMessage(w, l);
                    KeyUp(m.Key);
                    return IntPtr.Zero;
                }
            case WinMessage.Close:
                User.PostQuitMessage(0);
                break;
            case WinMessage.Paint:
                var ps = new PaintStruct();
                var someDc = User.BeginPaint(WindowHandle, ref ps);
                if (someDc == IntPtr.Zero)
                    throw new Exception();
                Paint();
                var wut = User.EndPaint(WindowHandle, ref ps);
                Debug.Assert(wut);
                return IntPtr.Zero;
        }
        return User.DefWindowProcW(hWnd, msg, w, l);
    }
}
