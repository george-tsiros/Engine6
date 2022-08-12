namespace Gl;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Win32;
using Linear;

public class SimpleWindow:Window {

    public SimpleWindow (Vector2i size, Vector2i? position = null) : base(size, position) { }

    public bool IsFocused { get; private set; }
    public event EventHandler<Buttons> ButtonDown;
    public event EventHandler<Buttons> ButtonUp;
    public event EventHandler<bool> FocusChanged;
    public event EventHandler<Keys> KeyDown;
    public event EventHandler<Keys> KeyUp;
    public event EventHandler Load;
    public event EventHandler MouseLeave;
    public event EventHandler<Vector2i> MouseMove;
    public event EventHandler Paint;

    bool cursorGrabbed;
    bool running = true;
    Vector2i clientSpaceCursorPositionBeforeGrab;

    public bool CursorGrabbed {
        get =>
            cursorGrabbed;
        set {
            if (value != cursorGrabbed)
                GrabCursor(value);
        }
    }

    void GrabCursor (bool grab) {
        cursorGrabbed = grab;
        if (grab) {
            _ = User.GetCursorPos(out var p);
            _ = User.ScreenToClient(WindowHandle, ref p);
            clientSpaceCursorPositionBeforeGrab = p;
        } else {
            var p = clientSpaceCursorPositionBeforeGrab;
            _ = User.ClientToScreen(WindowHandle, ref p);
            _ = User.SetCursorPos(p.X, p.Y);
        }
    }

    public Vector2i CursorLocation { get; private set; } = new(-1, -1);
    public Buttons Buttons { get; private set; }
    protected virtual void OnButtonDown (Buttons depressed) => ButtonDown?.Invoke(this, depressed);
    protected virtual void OnButtonUp (Buttons released) => ButtonUp?.Invoke(this, released);
    protected virtual void OnFocusChanged (bool isFocused) => FocusChanged?.Invoke(this, isFocused);
    protected virtual void OnKeyDown (Keys k) => KeyDown?.Invoke(this, k);
    protected virtual void OnKeyUp (Keys k) => KeyUp?.Invoke(this, k);
    protected virtual void OnLoad () => Load?.Invoke(this, new());
    protected virtual void OnMouseLeave () => MouseLeave?.Invoke(this, new());
    protected virtual void OnMouseMove (Vector2i currentPosition) => MouseMove?.Invoke(this, currentPosition);
    protected virtual void OnPaint () => Paint?.Invoke(this, new());

    Rect WindowRect = new();
    readonly int[] KeyState = new int[256 / 32];
    bool painting;

    public bool IsKeyDown (Keys key) {
        var (h, l) = Split(key);
        return (KeyState[h] & l) != 0;
    }

    protected List<IDisposable> Disposables { get; } = new();

    protected void Invalidate () {
        Demand(User.GetClientRect(WindowHandle, ref WindowRect));
        Demand(User.InvalidateRect(WindowHandle, ref WindowRect, 0));
    }

    //protected void Pump () {
    //    Message m = new();
    //    if (User.PeekMessageW(ref m, WindowHandle, 0, 0, PeekRemove.NoRemove)) {
    //        var eh = User.GetMessageW(ref m, 0, 0, 0);
    //        if (-1 == eh)
    //            Environment.FailFast(null);
    //        if (0 == eh) {
    //            running = false;
    //            return;
    //        }
    //        _ = User.DispatchMessageW(ref m);
    //    }
    //}

    public virtual void Run () {
        OnLoad();
        _ = User.ShowWindow(WindowHandle, CmdShow.Show);
        Demand(User.UpdateWindow(WindowHandle));
        Message m = new();
        while (running) {
            while (User.PeekMessageW(ref m, WindowHandle, 0, 0, PeekRemove.NoRemove)) {
                var eh = User.GetMessageW(ref m, 0, 0, 0);
                if (-1 == eh)
                    Environment.FailFast(null);
                if (0 == eh) {
                    running = false;
                    break;
                }
                _ = User.DispatchMessageW(ref m);
            }
            if (running && !painting)
                Invalidate();
        }
        foreach (var disposable in Disposables)
            disposable.Dispose();
    }

    override protected nint WndProc (nint hWnd, WinMessage msg, nint wPtr, nint lPtr) {
        switch (msg) {
            case WinMessage.MouseMove: {
                    if (!IsFocused)
                        break;
                    var position = Split(lPtr);
                    if (cursorGrabbed) {
                        var center = WindowRect.Center;
                        if (center == position)
                            return 0;
                        OnMouseMove(position - center);
                        _ = User.SetCursorPos(center.X, center.Y);
                    } else {
                        OnMouseMove(CursorLocation = new(position.X, Height - position.Y - 1));
                    }
                }
                return 0;
            case WinMessage.SysCommand: {
                    var i = (SysCommand)(int)(wPtr & int.MaxValue);
                    if (i == SysCommand.Close) {
                        User.PostQuitMessage(0);
                        return 0;
                    }
                }
                break;
            case WinMessage.LButtonDown:
            case WinMessage.RButtonDown:
            case WinMessage.MButtonDown:
            case WinMessage.XButtonDown: {
                    var w = (Buttons)(ushort.MaxValue & wPtr);
                    var change = w ^ Buttons;
                    Buttons = w;
                    OnButtonDown(change);
                }
                break;
            case WinMessage.LButtonUp:
            case WinMessage.RButtonUp:
            case WinMessage.MButtonUp:
            case WinMessage.XButtonUp: {
                    var w = (Buttons)(ushort.MaxValue & wPtr);
                    var change = w ^ Buttons;
                    Buttons = w;
                    OnButtonUp(change);
                }
                break;
            case WinMessage.MouseLeave:
                OnMouseLeave();
                break;
            case WinMessage.SetFocus:
                OnFocusChanged(IsFocused = true);
                break;
            case WinMessage.KillFocus:
                OnFocusChanged(IsFocused = false);
                break;
            case WinMessage.KeyDown: {
                    var m = new KeyMessage(wPtr, lPtr);
                    if (m.WasDown)
                        break;
                    var k = m.Key;
                    var (h, l) = Split(k);
                    KeyState[h] |= l;
                    OnKeyDown(k);
                    return 0;
                }
            case WinMessage.KeyUp: {
                    var k = new KeyMessage(wPtr, lPtr).Key;
                    var (h, l) = Split(k);
                    KeyState[h] &= ~l;
                    OnKeyUp(k);
                    return 0;
                }
            case WinMessage.Paint:
                if (running && !painting) {
                    var ps = new PaintStruct();
                    painting = true;
                    _ = Demand(User.BeginPaint(WindowHandle, ref ps));
                    OnPaint();
                    _ = User.EndPaint(WindowHandle, ref ps);
                    painting = false;
                }
                return 0;
        }
        return User.DefWindowProcW(hWnd, msg, wPtr, lPtr);
    }

    static (int h, int l) Split (Keys k) =>
        ((int)k >> 5, 1 << ((int)k & 31));

    static Vector2i Split (nint self) {
        var i = (int)(self & int.MaxValue);
        return new(i & ushort.MaxValue, (i >> 16) & ushort.MaxValue);
    }
}
