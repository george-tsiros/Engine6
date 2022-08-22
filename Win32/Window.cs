namespace Win32;

using Linear;
using System;
using System.Collections.Generic;

public class Window:WindowBase {

    public bool IsFocused { get; private set; }
    public Buttons Buttons { get; private set; }

    private readonly int[] KeyState = new int[256 / 32];
    private bool painting;

    protected virtual void OnButtonDown (Buttons depressed) { }
    protected virtual void OnButtonUp (Buttons released) { }
    protected virtual void OnFocusChanged (bool isFocused) { }
    protected virtual void OnKeyDown (Keys k) { }
    protected virtual void OnKeyUp (Keys k) { }
    protected virtual void OnLoad () { }
    protected virtual void OnMouseLeave () { }
    protected virtual void OnMouseMove (Vector2i currentPosition) { }
    protected virtual void OnPaint (IntPtr dc, in Rectangle r) { }
    protected virtual void OnIdle () { }

    public bool IsKeyDown (Keys key) {
        var (h, l) = Split(key);
        return (KeyState[h] & l) != 0;
    }

    protected List<IDisposable> Disposables { get; } = new();

    protected void Invalidate () {
        rect = User32.GetClientRect(WindowHandle);
        User32.InvalidateWindow(WindowHandle);
    }

    public void Run () {
        OnLoad();
        if (!User32.UpdateWindow(WindowHandle))
            throw new WinApiException(nameof(User32.UpdateWindow));
        _ = User32.ShowWindow(WindowHandle, CmdShow.ShowNormal);
        Loop();
        foreach (var disposable in Disposables)
            disposable.Dispose();
    }

    private void Loop () {
        for (var m = new Message(); ; ) {
            while (User32.PeekMessageA(ref m, WindowHandle, 0, 0, PeekRemove.NoRemove)) {
                var gotMessage = User32.GetMessageA(ref m, IntPtr.Zero, 0, 0);
                if (0 == gotMessage)
                    return;
                if (-1 == gotMessage)
                    Environment.FailFast(null);
                _ = User32.DispatchMessageA(ref m);
            }
            OnIdle();
        }
    }

    //private void Move (Vector2i p) {
    //}

    //private void Moving (Rectangle r) {
    //}

    //private void WindowPosChanging (WindowPos p) {
    //    if (!p.flags.HasFlag(WindowPosFlags.NoMove))
    //        rect = new(new(p.left, p.top), rect.Size);
    //    if (!p.flags.HasFlag(WindowPosFlags.NoSize))
    //        rect = new(rect.Location, new(p.width, p.height));
    //}

    //private void WindowPosChanged (WindowPos p) {
    //    if (!p.flags.HasFlag(WindowPosFlags.NoMove))
    //        rect = new(new(p.left, p.top), rect.Size);
    //    if (!p.flags.HasFlag(WindowPosFlags.NoSize))
    //        rect = new(rect.Location, new(p.width, p.height));
    //}

    private void Size (Vector2i size) {
        rect = new(rect.Location, size);
    }

    override unsafe protected nint WndProc (nint h, WinMessage m, nuint w, nint l) {
        switch (m) {
            //case WinMessage.Move:
            //    Move(Split(l));
            //    return 0;
            //case WinMessage.Moving:
            //    Moving(*(Rectangle*)l);
            //    return 0;
            //case WinMessage.WindowPosChanged:
            //    WindowPosChanged(*(WindowPos*)l);
            //    return 0;
            case WinMessage.Size:
                Size(Split(l));
                return 0;
            //case WinMessage.WindowPosChanging:
            //    WindowPosChanging(*(WindowPos*)l);
            //    return 0;
            case WinMessage.MouseMove: {
                    if (!IsFocused)
                        break;
                    var position = Split(l);
                    var p = new Vector2i(position.X, Rect.Height - position.Y - 1);
                    OnMouseMove(p);
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
                OnMouseLeave();
                break;
            case WinMessage.SetFocus:
                OnFocusChanged(IsFocused = true);
                break;
            case WinMessage.KillFocus:
                OnFocusChanged(IsFocused = false);
                break;
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
