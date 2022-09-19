namespace Win32;

using Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

public delegate nint WndProc (nint hWnd, WinMessage msg, nuint wparam, nint lparam);

public abstract class Window:IDisposable {

    public Window (WindowStyle style = WindowStyle.OverlappedWindow, WindowStyleEx styleEx = WindowStyleEx.None) {
        Handle = User32.CreateWindow(Atom, style, styleEx);
        Dc = new(this);
        if (!Windows.TryAdd(Handle, this))
            throw new ApplicationException();
    }

    public nint Handle { get; private set; }
    public Vector2i CursorLocation { get; private set; } = new(-1, -1);
    public DeviceContext Dc { get; private set; }
    public bool IsFocused { get; private set; }
    public MouseButton Buttons { get; private set; }
    public PixelFont PixelFont {
        get =>
            font ??= new("data/ibm3270.txt");
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
        Load?.Invoke(this, EventArgs.Empty);
        User32.UpdateWindow(this);
        _ = User32.ShowWindow(Handle, show);
        Message m = new();

        while (WinMessage.Quit != m.msg) {
            Idle?.Invoke(this, EventArgs.Empty);
            while (User32.PeekMessage(ref m, 0, 0, 0, PeekRemove.NoRemove)) {
                var x = User32.GetMessage(ref m);
                Debug.Assert(-1 != x);
                if (0 == x) {
                    Debug.Assert(WinMessage.Quit == m.msg);
                    break;
                }
                _ = User32.DispatchMessage(ref m);
            }
        }
        Closed?.Invoke(this, EventArgs.Empty);
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

    private static readonly ConcurrentDictionary<nint, Window> Windows = new();
    private static readonly ushort Atom;
    private static readonly WndProc staticWndProc = StaticWndProc;
    //private static Window creating;
    private readonly int[] KeyState = new int[256 / 32];
    private bool disposed;
    private PixelFont font;

    private static nint StaticWndProc (nint h, WinMessage m, nuint w, nint l) {
        if (WinMessage.Create == m) {
            return h;
        }

        if (Windows.TryGetValue(h, out var window))
            return window.WndProc(h/*wat*/, m, w, l);

        return User32.DefWindowProc(h, m, w, l);
    }

    public event EventHandler Load;
    public event EventHandler Idle;
    public event EventHandler Closed;
    //public event EventHandler<SizingEventArgs> Sizing;
    public event EventHandler<SizeEventArgs> Size;
    public event EventHandler<MoveEventArgs> Move;
    //public event EventHandler<MovingEventArgs> Moving;
    public event EventHandler<ShowWindowEventArgs> ShowWindow;
    public event EventHandler<ButtonEventArgs> ButtonDown, ButtonUp;
    public event EventHandler<FocusChangedEventArgs> FocusChanged;
    public event EventHandler<KeyEventArgs> KeyDown, KeyUp;
    public event EventHandler<InputEventArgs> Input;
    public event EventHandler<PaintEventArgs> Paint;
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
    //private void OnInput (int dx, int dy) { }
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
            //case WinMessage.Create:
            //    Dc = new(h);
            //    return 0;
            case WinMessage.Size:
                Size?.Invoke(this, new((SizeType)(int)(w & int.MaxValue), Split(l)));
                return 0;
            case WinMessage.Sizing:
                //if (0 != l) {
                //    var r = (Rectangle*)l;
                //    Sizing?.Invoke(this, new((SizingEdge)(w & int.MaxValue), ref *r));
                //    return 0;
                //}
                break;
            case WinMessage.Move:
                Move?.Invoke(this, new(Split(l)));
                return 0;
            case WinMessage.Moving:
                //if (0 != l) {
                //    var r = (Rectangle*)l;
                //    Moving?.Invoke(this, new(ref *r));
                //    return 0;
                //}
                break;
            case WinMessage.ShowWindow:
                ShowWindow?.Invoke(this, new(0 != w, (ShowWindowReason)(int)(l & int.MaxValue)));
                return 0;
            case WinMessage.ActivateApp:
                //OnActivateApp(0 != w);
                return 0;
            case WinMessage.Activate:
                //OnActivate(0 != (w & 0xffff0000), (ActivateKind)(0xffff & w));
                return 0;
            case WinMessage.CaptureChanged:
                //OnCaptureChanged(h);
                return 0;
            case WinMessage.EnterSizeMove:
                //OnEnterSizeMove();
                return 0;
            case WinMessage.ExitSizeMove:
                //OnExitSizeMove();
                return 0;
            case WinMessage.EraseBkgnd:
                return 1;
            case WinMessage.WindowPosChanging:
                //if (0 != l) {
                //    var p = (WindowPos*)l;
                //    OnWindowPosChanging(ref *p);
                //    return 0;
                //}
                break;
            case WinMessage.WindowPosChanged:
                //if (0 != l) {
                //    var p = (WindowPos*)l;
                //    OnWindowPosChanged(ref *p);
                //    return 0;
                //}
                break;
            case WinMessage.GetMinMaxInfo:
                //if (0 != l) {
                //    var p = (MinMaxInfo*)l;
                //    OnGetMinMaxInfo(ref *p);
                //    return 0;
                //}
                break;
            case WinMessage.LButtonDown:
            case WinMessage.RButtonDown:
            case WinMessage.MButtonDown:
            case WinMessage.XButtonDown: {
                    var wAsShort = (MouseButton)(ushort.MaxValue & w);
                    var change = wAsShort ^ Buttons;
                    Buttons = wAsShort;
                    ButtonDown?.Invoke(this, new(change, new(l)));
                }
                break;
            case WinMessage.LButtonUp:
            case WinMessage.RButtonUp:
            case WinMessage.MButtonUp:
            case WinMessage.XButtonUp: {
                    var wAsShort = (MouseButton)(ushort.MaxValue & w);
                    var change = wAsShort ^ Buttons;
                    Buttons = wAsShort;
                    ButtonUp?.Invoke(this, new(change, new(l)));
                }
                break;
            case WinMessage.SetFocus:
                FocusChanged?.Invoke(this, new(IsFocused = true));
                return 0;
            case WinMessage.KillFocus:
                FocusChanged?.Invoke(this, new(IsFocused = false));
                return 0;
            case WinMessage.KeyDown: {
                    var key = (Key)(w & byte.MaxValue);
                    var (hi, lo) = FindIndex(key);
                    KeyState[hi] |= lo;
                    Debug.Write($"down {key}\n");
                    KeyDown?.Invoke(this, new(key, 0 != (l & 0x40000000)));
                }
                return 0;
            case WinMessage.KeyUp: {
                    var key = (Key)(w & byte.MaxValue);
                    var (hi, lo) = FindIndex(key);
                    KeyState[hi] &= ~lo;
                    Debug.Write($"up {key}\n");
                    KeyUp?.Invoke(this, new(key, false));
                }
                return 0;
            case WinMessage.Paint:
                PaintStruct ps = new();
                var eh = User32.BeginPaint(Handle, ref ps);
                Paint?.Invoke(this, new(eh, in ps));
                User32.EndPaint(Handle, ref ps);
                return 0;
            case WinMessage.Input:
                if (Input is not null) {
                    RawMouse data = new();
                    if (User32.GetRawInputData(l, ref data))
                        if (0 != data.lastX || 0 != data.lastY) {
                            var r = Rect.Center;
                            User32.SetCursorPos(r.X, r.Y);
                            Input(this, new(data.lastX, data.lastY));
                        }
                }
                break;
        }
        return User32.DefWindowProc(h, m, w, l);
    }

    public virtual void Dispose () {
        if (!disposed) {
            disposed = true;
            Dc.Close();
            if (!Windows.TryRemove(Handle, out _))
                throw new ApplicationException();
            User32.DestroyWindow(this);
            GC.SuppressFinalize(this);
        }
    }

    private static (int h, int l) FindIndex (Key k) =>
        ((int)k >> 5, 1 << ((int)k & 31));

    private static Vector2i Split (nint l) {
        var i = (int)(l & int.MaxValue);
        return new(i & ushort.MaxValue, (i >> 16) & ushort.MaxValue);
    }
}
