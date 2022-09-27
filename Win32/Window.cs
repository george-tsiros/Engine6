namespace Win32;

using Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

public delegate void Handler<T> (in T t) where T : struct;

public class Window:IDisposable {

    private const string DefaultFontFilepath = "data/ibm3270.txt";
    private static Window Instance;
    public Window (WindowStyle style = WindowStyle.OverlappedWindow, WindowStyleEx styleEx = WindowStyleEx.None) {
        Debug.Assert(Instance is null);
        Instance = this;
        Handle = User32.CreateWindow(Atom, style, styleEx);
        Dc = new(this);
    }

    public nint Handle { get; private set; }
    public Vector2i CursorLocation { get; private set; } = new(-1, -1);
    public DeviceContext Dc { get; private set; }
    public bool IsFocused { get; private set; }
    public MouseButton Buttons { get; private set; }
    public PixelFont PixelFont {
        get =>
            font ??= new(DefaultFontFilepath);
        set =>
            font = value;
    }
    protected List<IDisposable> Disposables { get; } = new();

    protected Rectangle Rect {
        get =>
            User32.GetWindowRect(this);
        set =>
            throw new NotImplementedException();
    }

    protected Vector2i ClientSize {
        get =>
            User32.GetClientAreaSize(this);
        set {
            var clientSize = ClientSize;
            if (value != clientSize) {
                var r = Rect;
                var (w, h) = value + r.Size - clientSize;
                User32.MoveWindow(this, r.Left, r.Top, w, h, false);
            }
        }
    }

    public bool IsKeyDown (Key key) {
        var (h, l) = FindIndex(key);
        return (KeyState[h] & l) != 0;
    }

    public void Run (CmdShow show = CmdShow.ShowNormal) {
        OnLoad();
        User32.UpdateWindow(this);
        _ = User32.ShowWindow(Handle, show);
        Message m = new();

        while (WinMessage.Quit != m.msg) {
            OnIdle();
            while (User32.PeekMessage(ref m, 0, 0, 0, PeekRemove.NoRemove)) {
                var x = User32.GetMessage(ref m);
                Debug.Assert(-1 != x);
                _ = User32.DispatchMessage(ref m);
                if (0 == x) {
                    Debug.Assert(WinMessage.Quit == m.msg);
                    break;
                }
            }
        }
        OnClosed();
        foreach (var disposable in Disposables)
            disposable.Dispose();
    }

    private const string ClassName = nameof(Window);

    static Window () {
        WindowClassW wc = new() {
            style = ClassStyle.None,
            wndProc = staticWndProc,
            hCursor = User32.LoadCursor(SystemCursor.Arrow),
            classname = nameof(Window),
        };
        Atom = User32.RegisterClass(ref wc);
    }

    private static readonly ushort Atom;
    private static readonly WndProc staticWndProc = StaticWndProc;
    //private static Window creating;
    private readonly long[] KeyState = { 0, 0, 0, 0 };
    private bool disposed;
    private PixelFont font;

    private static nint StaticWndProc (nint h, WinMessage m, nuint w, nint l) {
        if (WinMessage.Create == m) {
            return h;
        }

        return Instance.WndProc(h/*wat*/, m, w, l);

    }

    protected virtual void OnLoad () { }
    protected virtual void OnIdle () { }
    protected virtual void OnClosed () { }
    protected virtual void OnSize (SizeType sizeType, in Vector2i size) { }
    protected virtual void OnMove (in Vector2i clientRelativePosition) { }
    protected virtual void OnShowWindow (bool shown, ShowWindowReason reason) { }
    protected virtual void OnButtonDown (MouseButton button, in PointShort location) { }
    protected virtual void OnButtonUp (MouseButton button, in PointShort location) { }
    protected virtual void OnFocusChanged () { }
    protected virtual void OnKeyDown (Key key, bool repeat) { }
    protected virtual void OnKeyUp (Key key) { }
    protected virtual void OnInput (int dx, int dy) { }
    protected virtual void OnPaint (nint dc, in PaintStruct ps) { }
    //public event EventHandler<SizingArgs> Sizing;
    //public event EventHandler<MovingArgs> Moving;
    //private void OnActivate (bool activated, ActivateKind kind) { }
    //private void OnActivateApp (bool activated) { }
    //private void OnButtonDown (MouseButton justDepressed, PointShort p) { }
    //private void OnButtonUp (MouseButton justReleased, PointShort p) { }
    //private void OnCaptureChanged (nint windowOwningMouse) { }
    //private void OnClosed () { }
    //private void OnCreate (ref CreateStructW cs) { }
    //private void OnEnterSizeMove () { }
    //private void OnExitSizeMove () { }
    //private void OnFocusChanged (bool isFocused) { }
    //private void OnGetMinMaxInfo (ref MinMaxInfo x) { }
    //private void OnIdle () { }
    //private void OnKeyDown (Key k) { }
    //private void OnKeyUp (Key k) { }
    //private void OnLoad () { }
    //private void OnMouseLeave () { }
    //private void OnMouseMove (in Vector2i currentPosition) { }
    //private void OnMove (in Vector2i clientRelativePosition) { }
    //private void OnMoving (ref Rectangle topLeft) { }
    //private void OnPaint () { }
    //private void OnShowWindow (bool shown, ShowWindow reason) { }
    //private void OnSize (SizeType type, Vector2i size) { }
    //private void OnSizing (SizingEdge edge, ref Rectangle r) { }
    //private void OnWindowPosChanged (ref WindowPos p) { }
    //private void OnWindowPosChanging (ref WindowPos p) { }

    protected unsafe nint WndProc (nint h, WinMessage m, nuint w, nint l) {
        switch (m) {
            case WinMessage.Close:
                User32.PostQuitMessage(0);
                return 0;
            case WinMessage.Size:
                OnSize((SizeType)(int)(w & int.MaxValue), Split(l));
                return 0;
            case WinMessage.Move:
                OnMove(Split(l));
                return 0;
            case WinMessage.ShowWindow:
                OnShowWindow(0 != w, (ShowWindowReason)(int)(l & int.MaxValue));
                return 0;
            case WinMessage.EraseBkgnd:
                return 1;
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
            case WinMessage.SetFocus:
                IsFocused = true;
                OnFocusChanged();
                return 0;
            case WinMessage.KillFocus:
                IsFocused = false;
                OnFocusChanged();
                return 0;
            case WinMessage.KeyDown: {
                    var key = (Key)(w & byte.MaxValue);
                    var (hi, lo) = FindIndex(key);
                    KeyState[hi] |= lo;
                    OnKeyDown(key, 0 != (l & 0x40000000));
                }
                return 0;
            case WinMessage.KeyUp: {
                    var key = (Key)(w & byte.MaxValue);
                    var (hi, lo) = FindIndex(key);
                    KeyState[hi] &= ~lo;
                    OnKeyUp(key);
                }
                return 0;
            case WinMessage.Paint:
                PaintStruct ps = new();
                var dc = User32.BeginPaint(Handle, ref ps);
                OnPaint(dc, in ps);
                User32.EndPaint(Handle, ref ps);
                return 0;
            case WinMessage.Input:
                RawMouse data = new();
                if (User32.GetRawInputData(l, ref data))
                    if (0 != data.lastX || 0 != data.lastY) {
                        var r = Rect.Center;
                        User32.SetCursorPos(r.X, r.Y);
                        OnInput(data.lastX, data.lastY);
                    }
                break;
        }
        return User32.DefWindowProc(h, m, w, l);
    }

    public virtual void Dispose () {
        if (!disposed) {
            disposed = true;
            Dc.Close();
            User32.DestroyWindow(this);
            Instance = null;
            GC.SuppressFinalize(this);
        }
    }

    private static (int h, long l) FindIndex (Key k) =>
        ((int)k >> 6, 1l << ((int)k & 63));

    private static Vector2i Split (nint l) {
        var i = (int)(l & int.MaxValue);
        return new(i & ushort.MaxValue, (i >> 16) & ushort.MaxValue);
    }
}
