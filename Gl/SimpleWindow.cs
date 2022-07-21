namespace Gl;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Win32;

public class SimpleWindow:WindowBase {

    public SimpleWindow (Vector2i size) : base(size) { }

    protected bool IsForeground { get; private set; }

    protected virtual void MouseMove (int x, int y) { }
    protected virtual void ButtonDown (int button) { }
    protected virtual void ButtonUp (int button) { }
    protected virtual void MouseLeave () { }
    protected virtual void CaptureChanged () { }
    protected virtual void FocusChanged (bool focused) { }
    protected virtual void Paint () { }
    protected virtual void Load () { }
    protected virtual void KeyUp (Keys k) { }

    protected virtual void KeyDown (Keys k) {
        switch (k) {
            case Keys.Escape:
                User.PostQuitMessage(0);
                break;
        }
    }

    private Rect WindowRect = new();
    protected void Invalidate () {
        Demand(User.GetClientRect(WindowHandle, ref WindowRect));
        Demand(User.InvalidateRect(WindowHandle, ref WindowRect, IntPtr.Zero));
    }

    public virtual void Run () {
        Load();
        _ = User.ShowWindow(WindowHandle, 10);
        Demand(User.UpdateWindow(WindowHandle));
        Message m = new();
        var invalidPtr = new IntPtr(-1);
        var running = true;
        while (running) {
            while (User.PeekMessageW(ref m, WindowHandle, 0, 0, PeekRemove.NoRemove)) {
                var eh = User.GetMessageW(ref m, IntPtr.Zero, 0, 0);
                if (eh == invalidPtr)
                    Environment.FailFast(null);
                if (eh == IntPtr.Zero) {
                    running = false;
                    break;
                }
                _ = User.DispatchMessageW(ref m);
            }
            Invalidate();
        }
    }
    private int lastMouseButtonState = 0;
    override protected IntPtr WndProc (IntPtr hWnd, WinMessage msg, IntPtr wPtr, IntPtr lPtr) {
        switch (msg) {
            case WinMessage.MouseMove: {
                    if (!IsForeground)
                        break;
                    var (x, y) = lPtr.Split();
                    MouseMove(x, y);
                }
                return IntPtr.Zero;
            case WinMessage.SysCommand: {
                    var i = (SysCommand)(IntPtr.Size > 8 ? (int)(wPtr.ToInt64() & int.MaxValue) : wPtr.ToInt32());
                    if (i == SysCommand.Close) {
                        User.PostQuitMessage(0);
                        return IntPtr.Zero;
                    }
                }
                break;
            case WinMessage.LButtonDown:
            case WinMessage.RButtonDown:
            case WinMessage.MButtonDown:
            case WinMessage.XButtonDown: {
                    var w = (int)(ushort.MaxValue & wPtr.ToInt64());
                    var change = w ^ lastMouseButtonState;
                    ButtonDown(change);
                    lastMouseButtonState = w;
                }
                break;
            case WinMessage.LButtonUp:
            case WinMessage.RButtonUp:
            case WinMessage.MButtonUp:
            case WinMessage.XButtonUp: {
                    var w = (int)(ushort.MaxValue & wPtr.ToInt64());
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
                    return IntPtr.Zero;
                }
            case WinMessage.KeyUp: {
                    var m = new KeyMessage(wPtr, lPtr);
                    KeyUp(m.Key);
                    return IntPtr.Zero;
                }
            //case WinMessage.Close:
            //    User.PostQuitMessage(0);
            //    break;
            case WinMessage.Paint:
                var ps = new PaintStruct();
                _ = Demand(User.BeginPaint(WindowHandle, ref ps));
                Paint();
                _ = User.EndPaint(WindowHandle, ref ps);
                return IntPtr.Zero;
        }
        return User.DefWindowProcW(hWnd, msg, wPtr, lPtr);
    }
}
