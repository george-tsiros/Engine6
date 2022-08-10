namespace Gl;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Win32;
using Linear;

public class SimpleWindow:Window {

    public SimpleWindow (Vector2i size) : base(size) { }

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

    public Vector2i CursorLocation { get; private set; } = new(-1, -1);
    protected virtual void OnButtonDown (Buttons depressed) => ButtonDown?.Invoke(this, depressed);
    protected virtual void OnButtonUp (Buttons released) => ButtonUp?.Invoke(this, released);
    protected virtual void OnFocusChanged (bool isFocused) => FocusChanged?.Invoke(this, isFocused);
    protected virtual void OnKeyDown (Keys k) => KeyDown?.Invoke(this, k);
    protected virtual void OnKeyUp (Keys k) => KeyUp?.Invoke(this, k);
    protected virtual void OnLoad () => Load?.Invoke(this, new());
    protected virtual void OnMouseLeave () => MouseLeave?.Invoke(this, new());
    protected virtual void OnMouseMove (Vector2i currentPosition) => MouseMove?.Invoke(this, currentPosition);
    protected virtual void OnPaint () => Paint?.Invoke(this, new());
    private Rect WindowRect = new();
    public Buttons Buttons { get; private set; }
    private int[] KeyState = new int[256 / 32];

    public bool IsKeyDown (Keys key) =>
        (KeyState[(int)key >> 5] & 1 << ((int)key) & 31) != 0;

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

    bool running = true;
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

    private static Vector2i Split (nint self) {
        var i = (int)(self & int.MaxValue);
        return new(i & ushort.MaxValue, (i >> 16) & ushort.MaxValue);
    }


    override protected nint WndProc (nint hWnd, WinMessage msg, nint wPtr, nint lPtr) {
        switch (msg) {
            case WinMessage.MouseMove: {
                    if (!IsFocused)
                        break;
                    var p = Split(lPtr);
                    OnMouseMove(CursorLocation = new(p.X, Height - p.Y - 1));
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
                    KeyState[(int)m.Key >> 5] |= 1 << ((int)m.Key & 31);
                    OnKeyDown(m.Key);
                    return 0;
                }
            case WinMessage.KeyUp: {
                    var k = new KeyMessage(wPtr, lPtr).Key;
                    KeyState[(int)k >> 5] &= ~(1 << ((int)k & 31));
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
    private bool painting;
}
