namespace Gl;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Win32;

public class BaseWindow:IDisposable {
    private readonly WndProc wndProc;
    protected int Width { get; }
    protected int Height { get; }
    protected ushort ClassAtom { get; }
    protected IntPtr WindowHandle { get; }
    protected static readonly IntPtr SelfHandle = Kernel.GetModuleHandleW(null);
    protected virtual string ClassName => "MYWINDOWCLASS";
    private bool disposed;
    private bool cursorTracked;
    protected bool IsForeground { get; private set; }

    public void Dispose () {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected void SetText (string text) => Demand(User.SetWindowText(WindowHandle, text));

    //protected virtual void WindowPosChanging (IntPtr w, IntPtr l) { }// => WriteLine(nameof(WindowPosChanging), w, l);
    //protected virtual void Moving (Rect r) { }// => Debug.WriteLine($"{nameof(Moving)} {r.left}, {r.top}, {r.right}, {r.bottom}");
    //protected virtual void WindowPosChanged (WindowPos p) { }// => Debug.WriteLine($"{nameof(WindowPosChanged)}: {p.x}, {p.y}, {p.cx}, {p.cy}");
    //protected virtual void EraseBkgnd () { }// => Debug.WriteLine(nameof(EraseBkgnd));
    //protected virtual void Move (short x, short y) { }// => WriteLine(nameof(Move), x, y);
    protected virtual void MouseMove (int x, int y) { }// => WriteLine(nameof(MouseMove), x, y);
    protected virtual void MouseEnter (bool entered) { }
    //protected virtual void NCMouseLeave (IntPtr w, IntPtr l) { }// => WriteLine(nameof(NCMouseLeave), w, l);
    protected virtual void CaptureChanged () { }// => Debug.WriteLine(nameof(CaptureChanged));
    //protected virtual void ExitSizeMove () { }// => Debug.WriteLine(nameof(ExitSizeMove));
    //protected virtual void EnterSizeMove () { }// => Debug.WriteLine(nameof(EnterSizeMove));
    protected virtual void SetFocus () { }// => Debug.WriteLine(nameof(SetFocus));
    protected virtual void KillFocus () { }// => Debug.WriteLine(nameof(KillFocus));
    //protected virtual void Size (SizeMessage m, int width, int height) { }// => Debug.WriteLine($"{nameof(Size)}, {m}, {width} x {height}");
    protected virtual void KeyUp (Keys k) { }
    protected virtual void KeyDown (Keys k) {
        switch (k) {
            case Keys.Escape:
                User.PostQuitMessage(0);
                break;
        }
    }
    protected virtual void Closing () { }
    protected virtual void Paint () { }
    protected virtual void Load () { }

    protected static (short x, short y) Split (IntPtr p) {
        var i = (int)(p.ToInt64() & int.MaxValue);
        return ((short)(i & ushort.MaxValue), (short)((i >> 16) & ushort.MaxValue));
    }

    unsafe public virtual void Run () {
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
    }

    private int CaptureMouse (bool capture) {
        return capture ? CaptureMouse(0, WindowHandle) : CaptureMouse(RawInputDevice.RemoveDevice, IntPtr.Zero);
    }

    private static unsafe int CaptureMouse (uint flags, IntPtr ptr) {
        RawInputDevice rid = new() { usagePage = 1, usage = 2, flags = flags, windowHandle = ptr };
        var structSize = System.Runtime.InteropServices.Marshal.SizeOf<RawInputDevice>();
        return User.RegisterRawInputDevices(ref rid, 1, (uint)structSize);
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
    public BaseWindow (Vector2i size) {
        if (size.X < 1 || size.Y < 1)
            throw new ArgumentOutOfRangeException(nameof(size));
        wndProc = new(WndProc);
        ClassAtom = User.RegisterWindowClass(wndProc, SelfHandle, ClassName);
        WindowHandle = User.CreateWindow(ClassAtom, size, SelfHandle);
        (Width, Height) = size;
    }


    private IntPtr WndProc (IntPtr hWnd, WinMessage msg, IntPtr w, IntPtr l) {
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
                    MouseMove(x, y);
                }
                return IntPtr.Zero;
            case WinMessage.SysCommand: {

                    var i = (SysCommand)(IntPtr.Size > 8 ? (int)(w.ToInt64() & int.MaxValue) : w.ToInt32());
                    if (i == SysCommand.Close) {
                        User.PostQuitMessage(0);
                        return IntPtr.Zero;
                    }
                    Debug.WriteLine(Enum.IsDefined(i) ? $"syscmd {i}" : $"syscmd {w}");
                }
                break;
            case WinMessage.MouseLeave:
                cursorTracked = false;
                MouseEnter(false);
                break;
            case WinMessage.CaptureChanged:
                CaptureChanged();
                break;
            case WinMessage.SetFocus:
                _ = CaptureMouse(IsForeground = true);
                SetFocus();
                break;
            case WinMessage.KillFocus:
                _ = CaptureMouse(IsForeground = false);
                KillFocus();
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
                return IntPtr.Zero;
            case WinMessage.Paint:
                var ps = new PaintStruct();
                _ = Demand(User.BeginPaint(WindowHandle, ref ps));
                Paint();
                _ = User.EndPaint(WindowHandle, ref ps);
                return IntPtr.Zero;
        }
        return User.DefWindowProcW(hWnd, msg, w, l);
    }
    protected static void Demand (bool condition, string message = null) {
        if (!condition) {
            var stackFrame = new StackFrame(1, true);
            var m = $">{stackFrame.GetFileName()}({stackFrame.GetFileLineNumber()},{stackFrame.GetFileColumnNumber()}): {message ?? "?"} ({Kernel.GetLastError():X})";
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

}
