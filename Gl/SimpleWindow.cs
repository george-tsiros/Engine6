namespace Gl;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Win32;

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

    protected Vector2i CursorLocation { get; private set; }
    protected virtual void OnButtonDown (Buttons changed) => ButtonDown?.Invoke(this, changed);
    protected virtual void OnButtonUp (Buttons changed) => ButtonUp?.Invoke(this, changed);
    protected virtual void OnFocusChanged (bool isFocused) => FocusChanged?.Invoke(this, isFocused);
    protected virtual void OnKeyDown (Keys k) => KeyDown?.Invoke(this, k);
    protected virtual void OnKeyUp (Keys k) => KeyUp?.Invoke(this, k);
    protected virtual void OnLoad () => Load?.Invoke(this, new());
    protected virtual void OnMouseLeave () => MouseLeave?.Invoke(this, new());
    protected virtual void OnMouseMove (Vector2i v) => MouseMove?.Invoke(this, v);
    protected virtual void OnPaint () => Paint?.Invoke(this, new());
    private Rect WindowRect = new();
    private Buttons lastMouseButtonState = Buttons.None;

    protected void Invalidate () {
        Demand(User.GetClientRect(WindowHandle, ref WindowRect));
        Demand(User.InvalidateRect(WindowHandle, ref WindowRect, 0));
    }

    protected void Pump () {
        Message m = new();
        if (User.PeekMessageW(ref m, WindowHandle, 0, 0, PeekRemove.NoRemove)) {
            var eh = User.GetMessageW(ref m, 0, 0, 0);
            if (-1 == eh)
                Environment.FailFast(null);
            if (0 == eh) {
                running = false;
                return;
            }
            _ = User.DispatchMessageW(ref m);
        }
    }

    bool running = true;
    public virtual void Run () {
        OnLoad();
        _ = User.ShowWindow(WindowHandle, 10);
        Demand(User.UpdateWindow(WindowHandle));
        Message m = new();
        while (running) {
            while (User.PeekMessageW(ref m, WindowHandle, 0, 0, PeekRemove.NoRemove)) {
                var eh = User.GetMessageW(ref m, 0, 0, 0);
                if (-1 == eh)
                    Environment.FailFast(null);
                if (0 == eh) {
                    running = false;
                    return;
                }
                _ = User.DispatchMessageW(ref m);
            }
            Invalidate();
        }
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
                    CursorLocation = new(p.X, Height - p.Y - 1);
                    OnMouseMove(p);
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
                    var change = w ^ lastMouseButtonState;
                    OnButtonDown(change);
                    lastMouseButtonState = w;
                }
                break;
            case WinMessage.LButtonUp:
            case WinMessage.RButtonUp:
            case WinMessage.MButtonUp:
            case WinMessage.XButtonUp: {
                    var w = (Buttons)(ushort.MaxValue & wPtr);
                    var change = w ^ lastMouseButtonState;
                    OnButtonUp(change);
                    lastMouseButtonState = w;
                }
                break;
            case WinMessage.MouseLeave:
                OnMouseLeave();
                break;
            //case WinMessage.CaptureChanged:
            //    OnCaptureChanged();
            //    break;
            case WinMessage.SetFocus:
                OnFocusChanged(IsFocused = true);
                break;
            case WinMessage.KillFocus:
                OnFocusChanged(IsFocused = false);
                break;
            //case WinMessage.Destroy:
            //    User.PostQuitMessage(0);
            //    break;
            case WinMessage.KeyDown: {
                    var m = new KeyMessage(wPtr, lPtr);
                    if (m.WasDown)
                        break;
                    OnKeyDown(m.Key);
                    return 0;
                }
            case WinMessage.KeyUp: {
                    var m = new KeyMessage(wPtr, lPtr);
                    OnKeyUp(m.Key);
                    return 0;
                }
            //case WinMessage.Close:
            //    User.PostQuitMessage(0);
            //    break;
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