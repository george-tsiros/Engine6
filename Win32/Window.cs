namespace Win32;

using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public delegate nint WndProc (IntPtr hWnd, WinMessage msg, nuint wparam, nint lparam);

public class Window:WindowBase {

    public bool IsFocused { get; private set; }
    public Buttons Buttons { get; private set; }

    private readonly int[] KeyState = new int[256 / 32];
    private bool painting;
    private TrackMouseEvent trackMouseStruct;
    private bool tracking = false;
    protected virtual void OnButtonDown (Buttons depressed) { }
    protected virtual void OnButtonUp (Buttons released) { }
    protected virtual void OnFocusChanged (bool isFocused) { }
    protected virtual void OnKeyDown (Keys k) { }
    protected virtual void OnKeyUp (Keys k) { }
    protected virtual void OnLoad () { }
    protected virtual void OnMouseLeave () { }
    protected virtual void OnMouseMove (in Vector2i currentPosition) { }
    protected virtual void OnPaint (IntPtr dc, in Rectangle r) { }
    //protected virtual void OnIdle () { }
    protected virtual void OnMove (in Vector2i topLeft) {
        Debug.WriteLine(topLeft);
    }

    protected virtual void OnSize (ResizeType type, in Vector2i clientSize) {
        Rect = new(Rect.Location, clientSize);
        Debug.WriteLine($"{type}, {clientSize}");
    }

    protected virtual void OnActivateApp (bool activated) {
        Debug.WriteLine($"{nameof(OnActivateApp)} {activated}");
    }

    protected virtual void OnWindowPosChanged (ref WindowPos windowPos) {
        var p = windowPos.flags.HasFlag(WindowPosFlags.NoMove) ? Rect.Location : new(windowPos.x, windowPos.y);
        var s = windowPos.flags.HasFlag(WindowPosFlags.NoSize) ? Rect.Size : new(windowPos.w, windowPos.h);
        Rect = new(p, s);
        Debug.WriteLine(windowPos);
    }

    protected virtual void OnWindowPosChanging (ref WindowPos p) {
        Debug.WriteLine(p);
    }

    protected virtual void OnShowWindow (bool shown, ShowWindow reason) {
        Debug.WriteLine(shown ? reason.ToString() : "not shown");
    }

    protected virtual void OnCreate (ref CreateStructA createStruct) {
        Rect = new(createStruct.x, createStruct.y, createStruct.x + createStruct.w, createStruct.y + createStruct.h);
        Debug.WriteLine($"({createStruct.x},{createStruct.y}), {createStruct.w}x{createStruct.h}, {createStruct.style}, {createStruct.exStyle}");
    }

    protected virtual void OnNcCalcSize (ref CalcSizeParameters p) {
        //
    }

    protected virtual void OnNcCreate (ref CreateStructA createStruct) {
        Debug.WriteLine($"({createStruct.x},{createStruct.y}), {createStruct.w}x{createStruct.h}, {createStruct.style}, {createStruct.exStyle}");
    }

    protected virtual void OnGetMinMaxInfo (ref MinMaxInfo minMaxInfo) {
        Debug.WriteLine($"{minMaxInfo.maxPosition}, {minMaxInfo.maxSize}");
    }

    public bool IsKeyDown (Keys key) {
        var (h, l) = Split(key);
        return (KeyState[h] & l) != 0;
    }

    protected List<IDisposable> Disposables { get; } = new();

    protected void Invalidate () {
        User32.InvalidateWindow(WindowHandle);
    }

    public void Run () {
        trackMouseStruct = new() {
            size = TrackMouseEvent.Size,
            flags = TrackMouseFlags.Leave,
            window = WindowHandle,
        };
        OnLoad();
        User32.UpdateWindow(WindowHandle);
        _ = User32.ShowWindow(WindowHandle, CmdShow.ShowNormal);
        Loop();
        foreach (var disposable in Disposables)
            disposable.Dispose();
    }

    private void Loop () {
        for (var m = new Message(); ;) {
            var gotMessage = User32.GetMessageA(ref m, IntPtr.Zero, 0, 0);
            if (0 == gotMessage)
                return;
            if (-1 == gotMessage)
                Environment.FailFast(null);
            _ = User32.DispatchMessageA(ref m);
            //while (User32.PeekMessageA(ref m, WindowHandle, 0, 0, PeekRemove.NoRemove)) {
            //}
            //OnIdle();
        }
    }
    Vector2i lastCursorLocation = new(-1, -1);
    override unsafe protected nint WndProc (nint h, WinMessage m, nuint w, nint l) {
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
                    CreateStructA* createStruct = (CreateStructA*)l;
                    OnCreate(ref *createStruct);
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
                    CreateStructA* createStruct = (CreateStructA*)l;
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
                    OnSize(resizeType, size);
                }
                return 0;
            case WinMessage.MouseMove: {
                    if (!IsFocused)
                        break;
                    if (!tracking) {
                        User32.TrackMouseEvent(ref trackMouseStruct);
                        tracking = true;
                    }
                    var position = Split(l);
                    var p = new Vector2i(position.X, Rect.Height - position.Y - 1);
                    if (p != lastCursorLocation) {
                        lastCursorLocation = p;
                        Debug.WriteLine($"{DateTime.Now:mm:ss.fff}MouseMove {p}");
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
                    var wAsShort = (Buttons)(ushort.MaxValue & w);
                    var change = wAsShort ^ Buttons;
                    Buttons = wAsShort;
                    OnButtonDown(change);
                }
                break;
            case WinMessage.LButtonUp:
            case WinMessage.RButtonUp:
            case WinMessage.MButtonUp:
            case WinMessage.XButtonUp: {
                    var wAsShort = (Buttons)(ushort.MaxValue & w);
                    var change = wAsShort ^ Buttons;
                    Buttons = wAsShort;
                    OnButtonUp(change);
                }
                break;
            case WinMessage.MouseLeave:
                Debug.WriteLine($"{DateTime.Now:mm:ss.fff}MouseLeave");
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
                    var key = (Keys)(byte)(w & byte.MaxValue);
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
                        var dc = User32.BeginPaint(WindowHandle, ref ps);
                        if (!ps.rect.IsEmpty)
                            OnPaint(dc, ps.rect);
                        User32.EndPaint(WindowHandle, ref ps);
                    } finally {
                        painting = false;
                    }
                return 0;
        }
        return User32.DefWindowProcA(h, m, w, l);
    }

    private Font font;
    public Font Font {
        get =>
            font ??= new("data/ibm_3270.txt");
        set =>
            font = value;
    }

    private static (int h, int l) Split (Keys k) =>
        ((int)k >> 5, 1 << ((int)k & 31));

    private static Vector2i Split (nint l) {
        var i = (int)(l & int.MaxValue);
        return new(i & ushort.MaxValue, (i >> 16) & ushort.MaxValue);
    }
}
