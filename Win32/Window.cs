namespace Win32;

using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public delegate nint WndProc (nint hWnd, WinMessage msg, nuint wparam, nint lparam);

public abstract class Window:IDisposable {

    static Window () {
        var wc = new WindowClassW() {
            style = ClassStyle.None,
            wndProc = staticWndProc,
            hCursor = User32.LoadCursor(SystemCursor.Arrow),
            classname = nameof(Window),
        };
        Atom = User32.RegisterClass(ref wc);
    }

    private const string ClassName = nameof(Window);
    private static readonly Dictionary<nint, Window> Windows = new();
    private static readonly ushort Atom;
    private static readonly WndProc staticWndProc = StaticWndProc;
    private static Window creating;

    protected nint Handle { get; private set; }

    private static nint StaticWndProc (IntPtr h, WinMessage m, nuint w, nint l) {
        if (WinMessage.Create == m) {
            Windows.Add(creating.Handle = h, creating);
            return creating.WndProc(h, m, w, l);
        } else if (Windows.TryGetValue(h, out var window))
            return window.WndProc(h/*wat*/, m, w, l);
        else
            return User32.DefWindowProc(h, m, w, l);
    }

    public DeviceContext Dc { get; private set; }

    public Window (WindowStyle style = WindowStyle.OverlappedWindow) {
        creating = this;
        var h = User32.CreateWindow(Atom, style, WindowStyleEx.None);
        Debug.Assert(h == Handle);
    }

    public bool IsFocused { get; private set; }
    public MouseButton Buttons { get; private set; }

    protected Vector2i Size => User32.GetClientAreaSize(Handle);

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
    protected virtual void OnPaint (in Rectangle r) { }
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
                        OnPaint(ps.rect);
                    User32.EndPaint(h, ref ps);
                }
                return 0;
        }
        return User32.DefWindowProc(h, m, w, l);
    }

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

    public virtual void Dispose () {
        if (!disposed) {
            disposed = true;
            Dc.Close();
            _ = Windows.Remove(Handle);
            User32.DestroyWindow(Handle);
        }
    }
}
