namespace Win32;

using Common;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

public delegate nint WndProc (nint hWnd, WinMessage msg, nuint wparam, nint lparam);

public abstract class Window:IDisposable {
    private const string ClassName = nameof(Window);
    private static readonly Dictionary<nint, Window> Windows = new();
    static readonly ushort Atom;
    protected readonly nint Handle;

    static Window () { 
            var wc = new WindowClassW() {
            style = ClassStyle.None,
            wndProc = staticWndProc,
            hCursor = User32.LoadCursor(SystemCursor.Arrow),
            classname = nameof(Window),
        };
    }
        private static nint StaticWndProc (IntPtr h, WinMessage m, nuint w, nint l) {
        if (WinMessage.Create == m)
            Windows.Add(creating.WindowHandle = h, creating);
        return creating.WndProc(h, m, w, l);
    }

    public DeviceContext Dc { get; private set; }

    public Window () {
    }

    public bool IsFocused { get; private set; }
    public MouseButton Buttons { get; private set; }

    protected Rectangle Rect => User32.GetClientRect(Handle);

    private readonly int[] KeyState = new int[256 / 32];
    private TrackMouseEvent trackMouseStruct;
    private bool tracking = false;

    protected virtual void OnLoad () { }
    protected virtual void OnIdle () { }
    protected virtual void OnClosed () { }

    protected virtual void OnActivateApp (bool activated) { }
    protected virtual void OnActivate (bool activated, ActivateKind kind) { }
    protected virtual void OnButtonDown (MouseButton justDepressed, PointShort p) { }
    protected virtual void OnButtonUp (MouseButton justReleased, PointShort p) { }
    protected virtual void OnCaptureChanged (nint windowOwningMouse) { }
    protected virtual void OnEnterSizeMove () { }
    protected virtual void OnExitSizeMove () { }
    protected virtual void OnFocusChanged (bool isFocused) { }
    protected virtual unsafe void OnGetMinMaxInfo (MinMaxInfo* x) { }
    protected virtual void OnKeyDown (Key k) { }
    protected virtual void OnKeyUp (Key k) { }
    //protected virtual void OnMouseLeave () { }
    protected virtual void OnMouseMove (in Vector2i currentPosition) { }
    protected virtual void OnMove (in Vector2i clientRelativePosition) { }
    protected virtual unsafe void OnMoving (Rectangle* topLeft) { }
    protected virtual void OnSize (SizeType type, Vector2i size) { }
    protected virtual unsafe void OnSizing (SizingEdge edge, Rectangle* r) { }
    //protected virtual unsafe void OnCreate (CreateStructW* cs) {    }
    protected virtual void OnPaint () { }
    protected virtual void OnShowWindow (bool shown, ShowWindow reason) { }
    protected virtual unsafe void OnWindowPosChanged (WindowPos* p) { }
    protected virtual unsafe void OnWindowPosChanging (WindowPos* p) { }

    public bool IsKeyDown (Key key) {
        var (h, l) = FindIndex(key);
        return (KeyState[h] & l) != 0;
    }

    protected List<IDisposable> Disposables { get; } = new();
    private bool invalidated = false;
    protected void Invalidate () {
        if (!invalidated) {
            invalidated = true;
            User32.InvalidateWindow(Handle);
        }
    }
    private bool idling = false;
    public void Run () {
        trackMouseStruct = new() {
            size = TrackMouseEvent.Size,
            flags = TrackMouseFlag.Leave,
            window = Handle,
        };
        OnLoad();
        User32.UpdateWindow(Handle);
        _ = User32.ShowWindow(Handle, CmdShow.ShowNormal);
        var m = new Message();
        while (WinMessage.Quit != m.msg) {
            OnIdle();
            while (User32.PeekMessage(ref m, 0, 0, 0, PeekRemove.NoRemove))
                if (User32.GetMessage(ref m))
                    _ = User32.DispatchMessage(ref m);
        }
        OnClosed();
        foreach (var disposable in Disposables)
            disposable.Dispose();
    }

    public Vector2i CursorLocation { get; private set; } = new(-1, -1);

    protected unsafe nint WndProc (nint h, WinMessage m, nuint w, nint l) {
        switch (m) {
            case WinMessage.Create:
                Dc = new(h);
                // yes, it is different from NcCreate
                return 0;
            case WinMessage.Size:
                OnSize((SizeType)(int)(w & int.MaxValue), Split(l));
                return 0;
            case WinMessage.Sizing:
                if (0 != l) {
                    OnSizing((SizingEdge)(w & int.MaxValue), (Rectangle*)l);
                    return 0;
                }
                break;
            case WinMessage.Move:
                OnMove(Split(l));
                return 0;
            case WinMessage.Moving:
                if (0 != l) {
                    OnMoving((Rectangle*)l);
                    return 0;
                }
                break;
            case WinMessage.ShowWindow:
                OnShowWindow(0 != w, (ShowWindow)(int)(l & int.MaxValue));
                return 0;
            case WinMessage.WindowPosChanging:
                if (0 != l) {
                    OnWindowPosChanging((WindowPos*)l);
                    return 0;
                }
                break;
            case WinMessage.ActivateApp:
                OnActivateApp(0 != w);
                return 0;
            case WinMessage.Activate:
                OnActivate(0 != (w & 0xffff0000), (ActivateKind)(0xffff & w));
                return 0;
            case WinMessage.CaptureChanged:
                OnCaptureChanged(h);
                return 0;
            case WinMessage.EnterSizeMove:
                OnEnterSizeMove();
                return 0;
            case WinMessage.ExitSizeMove:
                OnExitSizeMove();
                return 0;
            case WinMessage.EraseBkgnd:
                return 1;
            case WinMessage.WindowPosChanged:
                if (0 != l) {
                    OnWindowPosChanged((WindowPos*)l);
                    return 0;
                }
                break;
            case WinMessage.GetMinMaxInfo:
                if (0 != l) {
                    OnGetMinMaxInfo((MinMaxInfo*)l);
                    return 0;
                }
                break;
            case WinMessage.MouseMove: {
                    if (!IsFocused)
                        break;
                    if (!tracking) {
                        User32.TrackMouseEvent(ref trackMouseStruct);
                        tracking = true;
                    }
                    var p = Split(l);
                    if (p != CursorLocation)
                        OnMouseMove(CursorLocation = p);
                }
                return 0;
            //    case WinMessage.SysCommand: {
            //            if ((int)(w & int.MaxValue) == (int)SysCommand.Close) {
            //                User32.PostQuitMessage(0);
            //                return 0;
            //            }
            //        }
            //        break;
            case WinMessage.LButtonDown:
            case WinMessage.RButtonDown:
            case WinMessage.MButtonDown:
            case WinMessage.XButtonDown: {
                    var wAsShort = (MouseButton)(ushort.MaxValue & w);
                    var change = wAsShort ^ Buttons;
                    Buttons = wAsShort;
                    OnButtonDown(change, new(l));
                }
                break;
            case WinMessage.LButtonUp:
            case WinMessage.RButtonUp:
            case WinMessage.MButtonUp:
            case WinMessage.XButtonUp: {
                    var wAsShort = (MouseButton)(ushort.MaxValue & w);
                    var change = wAsShort ^ Buttons;
                    Buttons = wAsShort;
                    OnButtonUp(change, new(l));
                }
                break;
            //    case WinMessage.MouseLeave:
            //        //Debug.WriteLine($"{DateTime.Now:mm:ss.fff}MouseLeave");
            //        tracking = false;
            //        OnMouseLeave();
            //        return 0;
            case WinMessage.SetFocus:
                OnFocusChanged(IsFocused = true);
                return 0;
            case WinMessage.KillFocus:
                OnFocusChanged(IsFocused = false);
                return 0;
            case WinMessage.KeyDown:
                if (0 == (l & 0x40000000)) {
                    var key = (Key)(w & byte.MaxValue);
                    var (hi, lo) = FindIndex(key);
                    KeyState[hi] |= lo;
                    OnKeyDown(key);
                }
                return 0;
            case WinMessage.KeyUp: {
                    var key = (Key)(w & byte.MaxValue);
                    var (hi, lo) = FindIndex(key);
                    KeyState[hi] &= ~lo;
                    OnKeyUp(key);
                    return 0;
                }
            case WinMessage.Paint: {
                    var ps = new PaintStruct();
                    var dc = User32.BeginPaint(h, ref ps);
                    if (!ps.rect.IsEmpty)
                        OnPaint();
                    User32.EndPaint(h, ref ps);
                }
                return 0;
        }
        return User32.DefWindowProc(h, m, w, l);
    }



    /*
Activate => 0
ActivateApp => 0
EraseBkgnd => 0
GETICON => 0
ImeNotify => 0
ImeSetContext => 0
KillFocus => 0
MouseMove => 0
Move => 0
NcActivate => 1
NcCalcSize => 0
NcCreate => 1
NCHITTEST => 1
NcPaint => 0
Paint => 0
SETCURSOR => 0
SetFocus => 0
ShowWindow => 0
Size => 0
WindowPosChanged => 0
WindowPosChanging => 0
*/
    private Font font;
    public Font Font {
        get =>
            font ??= new("data/ubuntu_mono_ligaturized.txt");
        set =>
            font = value;
    }

    private static (int h, int l) FindIndex (Key k) =>
        ((int)k >> 5, 1 << ((int)k & 31));

    protected static Vector2i Split (nint l) {
        var i = (int)(l & int.MaxValue);
        return new(i & ushort.MaxValue, (i >> 16) & ushort.MaxValue);
    }

    private bool disposed;

    public void Dispose () {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual void Dispose (bool dispose) {
        if (dispose && !disposed) {
            disposed = true;
            Dc.Close();
        }
    }
}
