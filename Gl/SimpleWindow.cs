namespace Gl;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Win32;

public class SimpleWindow:WindowBase {

    public SimpleWindow (Vector2i size) : base(size) { }

    protected bool IsForeground { get; private set; }

    protected virtual void MouseMove (Vector2i location) { }
    protected virtual void ButtonDown (int button) { }
    protected virtual void ButtonUp (int button) { }
    protected virtual void MouseLeave () { }
    protected virtual void CaptureChanged () { }
    protected virtual void FocusChanged (bool focused) { }
    protected virtual void Paint () { }
    protected virtual void Load () { }
    protected virtual void KeyUp (Keys k) { }
    protected Vector2i CursorLocation { get; private set; }

    private Rect WindowRect = new();
    private int lastMouseButtonState = 0;

    protected virtual void KeyDown (Keys k) {
        switch (k) {
            case Keys.Escape:
                User.PostQuitMessage(0);
                break;
        }
    }

    protected void Invalidate () {
        Demand(User.GetClientRect(WindowHandle, ref WindowRect));
        Demand(User.InvalidateRect(WindowHandle, ref WindowRect, 0));
    }

    public virtual void Run () {
        Load();
        _ = User.ShowWindow(WindowHandle, 10);
        Demand(User.UpdateWindow(WindowHandle));
        Message m = new();
        var invalidPtr = (nint)(-1);
        var running = true;
        while (running) {
            while (User.PeekMessageW(ref m, WindowHandle, 0, 0, PeekRemove.NoRemove)) {
                var eh = User.GetMessageW(ref m, 0, 0, 0);
                if (eh == invalidPtr)
                    Environment.FailFast(null);
                if (eh == 0) {
                    running = false;
                    break;
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
    readonly Queue<WinMessage> lastMessages = new(MessageHistoryLength);
    const int MessageHistoryLength = 1000;
    protected IReadOnlyCollection<WinMessage> LastMessages => lastMessages;
    override protected nint WndProc (nint hWnd, WinMessage msg, nint wPtr, nint lPtr) {
        if (lastMessages.Count == MessageHistoryLength)
            _ = lastMessages.Dequeue();
        lastMessages.Enqueue(msg);
        switch (msg) {
            case WinMessage.MouseMove: {
                    if (!IsForeground)
                        break;
                    var p = Split(lPtr);
                    CursorLocation = p;
                    MouseMove(p);
                }
                return 0;
            case WinMessage.SysCommand: {
                    var i = (SysCommand)((int)(wPtr & int.MaxValue));
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
                    var w = (int)(ushort.MaxValue & wPtr);
                    var change = w ^ lastMouseButtonState;
                    ButtonDown(change);
                    lastMouseButtonState = w;
                }
                break;
            case WinMessage.LButtonUp:
            case WinMessage.RButtonUp:
            case WinMessage.MButtonUp:
            case WinMessage.XButtonUp: {
                    var w = (int)(ushort.MaxValue & wPtr);
                    var change = w ^ lastMouseButtonState;
                    ButtonUp(change);
                    lastMouseButtonState = w;
                }
                break;
            case WinMessage.MouseLeave:
                MouseLeave();
                break;
            case WinMessage.CaptureChanged:
                CaptureChanged();
                break;
            case WinMessage.SetFocus:
                FocusChanged(IsForeground = true);
                break;
            case WinMessage.KillFocus:
                FocusChanged(IsForeground = false);
                break;
            //case WinMessage.Destroy:
            //    User.PostQuitMessage(0);
            //    break;
            case WinMessage.KeyDown: {
                    var m = new KeyMessage(wPtr, lPtr);
                    if (m.WasDown)
                        break;
                    KeyDown(m.Key);
                    return 0;
                }
            case WinMessage.KeyUp: {
                    var m = new KeyMessage(wPtr, lPtr);
                    KeyUp(m.Key);
                    return 0;
                }
            //case WinMessage.Close:
            //    User.PostQuitMessage(0);
            //    break;
            case WinMessage.Paint:
                Paint();
                //var ps = new PaintStruct();
                //_ = Demand(User.BeginPaint(WindowHandle, ref ps));
                //_ = User.EndPaint(WindowHandle, ref ps);
                return 0;
        }
        return User.DefWindowProcW(hWnd, msg, wPtr, lPtr);
    }
}
