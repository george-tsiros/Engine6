namespace Win32;

using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public delegate nint WndProc (IntPtr hWnd, WinMessage msg, nuint wparam, nint lparam);

public class Window:IDisposable {

    protected Win32Window nativeWindow;
    public DeviceContext Dc { get; private set; }
    readonly Vector2i clientSize;
    public Window (Vector2i? size = null) {
        nativeWindow = new(WndProc, clientSize = size ?? new(640, 480)) { };
    }

    public bool IsFocused { get; private set; }
    public MouseButton Buttons { get; private set; }

    protected Rectangle Rect { get; set; }

    private readonly int[] KeyState = new int[256 / 32];
    private bool painting;
    private TrackMouseEvent trackMouseStruct;
    private bool tracking = false;

    protected virtual void OnActivateApp (bool activated) { }
    protected virtual void OnButtonDown (MouseButton depressed) { }
    protected virtual void OnButtonUp (MouseButton released) { }
    protected virtual void OnClosed () { }
    protected virtual void OnFocusChanged (bool isFocused) { }
    protected virtual void OnGetMinMaxInfo (ref MinMaxInfo minMaxInfo) { }
    protected virtual void OnIdle () { }
    protected virtual void OnKeyDown (Key k) { }
    protected virtual void OnKeyUp (Key k) { }
    protected virtual void OnLoad () { }
    protected virtual void OnMouseLeave () { }
    protected virtual void OnMouseMove (in Vector2i currentPosition) { }
    protected virtual void OnMove (in Vector2i topLeft) { }
    protected virtual void OnNcCalcSize (ref CalcSizeParameters p) { }
    protected virtual void OnNcCreate (ref CreateStructW createStruct) { }
    protected virtual void OnPaint () { }
    protected virtual void OnShowWindow (bool shown, ShowWindow reason) { }
    protected virtual void OnWindowPosChanged (ref WindowPos windowPos) { }
    protected virtual void OnWindowPosChanging (ref WindowPos p) { }

    public bool IsKeyDown (Key key) {
        var (h, l) = Split(key);
        return (KeyState[h] & l) != 0;
    }

    protected List<IDisposable> Disposables { get; } = new();
    bool invalidated = false;
    protected void Invalidate () {
        if (!invalidated) {
            invalidated = true;
            User32.InvalidateWindow(nativeWindow.WindowHandle);
        }
    }

    public void Run () {
        trackMouseStruct = new() {
            size = TrackMouseEvent.Size,
            flags = TrackMouseFlag.Leave,
            window = nativeWindow.WindowHandle,
        };
        OnLoad();
        User32.UpdateWindow(nativeWindow.WindowHandle);
        _ = User32.ShowWindow(nativeWindow.WindowHandle, CmdShow.ShowNormal);
        var m = new Message();
        while (User32.GetMessage(ref m))
            _ = User32.DispatchMessage(ref m);
        OnClosed();
        foreach (var disposable in Disposables)
            disposable.Dispose();
    }

    public Vector2i CursorLocation { get; private set; } = new(-1, -1);

    protected unsafe nint WndProc (IntPtr h, WinMessage m, nuint w, nint l) {
        //Debug.WriteLine(m);
        switch (m) {
            case WinMessage.Move: {
                    var location = Split(l);
                    OnMove(location);
                }
                return 0;
            case WinMessage.EraseBkgnd:
                return 1;
            case WinMessage.NcPaint:
                return 0;
            case WinMessage.NcActivate:
                break;
            case WinMessage.ActivateApp:
                OnActivateApp(0 != w);
                return 0;
            case WinMessage.WindowPosChanged: {
                    WindowPos* windowPos = (WindowPos*)l;
                    var p = windowPos->flags.HasFlag(WindowPosFlags.NoMove) ? Rect.Location : new(windowPos->x, windowPos->y);
                    var s = windowPos->flags.HasFlag(WindowPosFlags.NoSize) ? Rect.Size : new(windowPos->w, windowPos->h);
                    Rect = new(p, s);
                    OnWindowPosChanged(ref *windowPos);
                    return 0;
                }
            case WinMessage.WindowPosChanging: {
                    WindowPos* windowPos = (WindowPos*)l;
                    OnWindowPosChanging(ref *windowPos);
                    return 0;
                }
            case WinMessage.ShowWindow:
                OnShowWindow(0 != w, 0 != w ? (ShowWindow)(int)(l & int.MaxValue) : ShowWindow.None);
                return 0;
            case WinMessage.Create:
                if (0 != l) {
                    CreateStructW createStruct = *(CreateStructW*)l;
                    Rect = new(createStruct.x, createStruct.y, createStruct.x + createStruct.w, createStruct.y + createStruct.h);
                    Dc = new(h);
                    // yes, it is different from NcCreate
                    return 0;
                }
                break;
            case WinMessage.NcCalcSize:
                if (0 != w) {
                    CalcSizeParameters* p = (CalcSizeParameters*)l;
                    OnNcCalcSize(ref *p);
                    return 1;
                }
                break;
            case WinMessage.NcCreate:
                if (0 != l) {
                    CreateStructW* createStruct = (CreateStructW*)l;
                    OnNcCreate(ref *createStruct);
                    return 1;
                }
                break;
            case WinMessage.GetMinMaxInfo:
                if (0 != l) {
                    MinMaxInfo* minMaxInfo = (MinMaxInfo*)l;
                    OnGetMinMaxInfo(ref *minMaxInfo);
                    return 0;
                }
                break;
            case WinMessage.Size: {
                    var size = Split(l);
                    var resizeType = (ResizeType)(int)(w & int.MaxValue);
                }
                return 0;
            case WinMessage.MouseMove: {
                    if (!IsFocused)
                        break;
                    if (!tracking) {
                        User32.TrackMouseEvent(ref trackMouseStruct);
                        tracking = true;
                    }
                    var p = Split(l);
                    if (p != CursorLocation) {
                        CursorLocation = p;
                        //Debug.WriteLine($"{DateTime.Now:mm:ss.fff}MouseMove {p}");
                        OnMouseMove(p);
                    }
                }
                return 0;
            case WinMessage.SysCommand: {
                    if ((int)(w & int.MaxValue) == (int)SysCommand.Close) {
                        User32.PostQuitMessage(0);
                        return 0;
                    }
                }
                break;
            case WinMessage.LButtonDown:
            case WinMessage.RButtonDown:
            case WinMessage.MButtonDown:
            case WinMessage.XButtonDown: {
                    var wAsShort = (MouseButton)(ushort.MaxValue & w);
                    var change = wAsShort ^ Buttons;
                    Buttons = wAsShort;
                    OnButtonDown(change);
                }
                break;
            case WinMessage.LButtonUp:
            case WinMessage.RButtonUp:
            case WinMessage.MButtonUp:
            case WinMessage.XButtonUp: {
                    var wAsShort = (MouseButton)(ushort.MaxValue & w);
                    var change = wAsShort ^ Buttons;
                    Buttons = wAsShort;
                    OnButtonUp(change);
                }
                break;
            case WinMessage.MouseLeave:
                //Debug.WriteLine($"{DateTime.Now:mm:ss.fff}MouseLeave");
                tracking = false;
                OnMouseLeave();
                return 0;
            case WinMessage.SetFocus:
                OnFocusChanged(IsFocused = true);
                return 0;
            case WinMessage.KillFocus:
                OnFocusChanged(IsFocused = false);
                return 0;
            case WinMessage.KeyDown: {
                    var km = new KeyMessage(w, l);
                    if (km.wasDown)
                        break;
                    var key = km.key;
                    var (hi, lo) = Split(key);
                    KeyState[hi] |= lo;
                    OnKeyDown(key);
                    return 0;
                }
            case WinMessage.KeyUp: {
                    var key = (Key)(byte)(w & byte.MaxValue);
                    var (hi, lo) = Split(key);
                    KeyState[hi] &= ~lo;
                    OnKeyUp(key);
                    return 0;
                }
            case WinMessage.Paint:
                if (!painting)
                    try {
                        var ps = new PaintStruct();
                        painting = true;
                        var dc = User32.BeginPaint(h, ref ps);
                        if (!ps.rect.IsEmpty)
                            OnPaint();
                        User32.EndPaint(h, ref ps);
                        invalidated = false;
                    } finally {
                        painting = false;
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

    private static (int h, int l) Split (Key k) =>
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
            nativeWindow.Dispose();
        }
    }
}
