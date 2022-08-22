namespace Win32;

using Linear;
using System;
using System.Collections.Generic;

public class Window:WindowBase {

    public bool IsFocused { get; private set; }

    bool running;

    public Buttons Buttons { get; private set; }

    public event EventHandler<Buttons> ButtonDown;
    protected virtual void OnButtonDown (Buttons depressed) => ButtonDown?.Invoke(this, depressed);

    public event EventHandler<Buttons> ButtonUp;
    protected virtual void OnButtonUp (Buttons released) => ButtonUp?.Invoke(this, released);

    public event EventHandler<bool> FocusChanged;
    protected virtual void OnFocusChanged (bool isFocused) => FocusChanged?.Invoke(this, isFocused);

    public event EventHandler<Keys> KeyDown;
    protected virtual void OnKeyDown (Keys k) => KeyDown?.Invoke(this, k);

    public event EventHandler<Keys> KeyUp;
    protected virtual void OnKeyUp (Keys k) => KeyUp?.Invoke(this, k);

    protected virtual void Load () { }

    public event EventHandler MouseLeave;
    protected virtual void OnMouseLeave () => MouseLeave?.Invoke(this, new());

    public event EventHandler<Vector2i> MouseMove;
    protected virtual void OnMouseMove (Vector2i currentPosition) => MouseMove?.Invoke(this, currentPosition);

    public event EventHandler<PaintEventArgs> Paint;
    protected virtual void OnPaint (IntPtr dc, Rect r) => Paint?.Invoke(this, new(dc, r));

    readonly int[] KeyState = new int[256 / 32];
    bool painting;

    public bool IsKeyDown (Keys key) {
        var (h, l) = Split(key);
        return (KeyState[h] & l) != 0;
    }

    protected List<IDisposable> Disposables { get; } = new();

    protected void Invalidate () {
        rect = User32.GetClientRect(WindowHandle);
        if (!User32.InvalidateRect(WindowHandle, ref rect, false))
            throw new Exception(nameof(User32.InvalidateRect));
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

    public void Run () {
        _ = User32.SetWindowLongPtrW(WindowHandle, -16, IntPtr.Zero);
        Load();
        running = true;
        _ = User32.ShowWindow(WindowHandle, CmdShow.Show);
        if (!User32.UpdateWindow(WindowHandle))
            throw new WinApiException(nameof(User32.UpdateWindow));

        Message m = new();
        while (running) {
            while (User32.PeekMessageW(ref m, WindowHandle, 0, 0, PeekRemove.NoRemove)) {
                var eh = User32.GetMessageW(ref m, IntPtr.Zero, 0, 0);
                if (-1 == eh)
                    Environment.FailFast(null);
                if (0 == eh) {
                    running = false;
                    break;
                }
                _ = User32.DispatchMessageW(ref m);
            }
            if (running && !painting)
                Invalidate();
        }
        foreach (var disposable in Disposables)
            disposable.Dispose();
    }
    private void Move (Vector2i p) {
    }

    private void Moving (Rect r) {
    }

    private void WindowPosChanging (WindowPos p) {
        if (!p.flags.HasFlag(WindowPosFlags.NoMove))
            rect = new(new(p.left, p.top), rect.Size);
        if (!p.flags.HasFlag(WindowPosFlags.NoSize))
            rect = new(rect.Location, new(p.width, p.height));
    }

    private void WindowPosChanged (WindowPos p) {
        if (!p.flags.HasFlag(WindowPosFlags.NoMove))
            rect = new(new(p.left, p.top), rect.Size);
        if (!p.flags.HasFlag(WindowPosFlags.NoSize))
            rect = new(rect.Location, new(p.width, p.height));
    }

    private void Size (Vector2i size) {
        rect = new(rect.Location, size);
    }

    override unsafe protected nint WndProc (nint h, WinMessage m, nuint w, nint l) {
        switch (m) {
            case WinMessage.Move:
                Move(Split(l));
                return 0;
            case WinMessage.Moving:
                Moving(*(Rect*)l);
                return 0;
            case WinMessage.WindowPosChanged:
                WindowPosChanged(*(WindowPos*)l);
                return 0;
            case WinMessage.Size:
                Size(Split(l));
                return 0;
            case WinMessage.WindowPosChanging:
                WindowPosChanging(*(WindowPos*)l);
                return 0;
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
                    if (km.WasDown)
                        break;
                    var key = km.Key;
                    var (hi, lo) = Split(key);
                    KeyState[hi] |= lo;
                    OnKeyDown(key);
                    return 0;
                }
            case WinMessage.KeyUp: {
                    var key = new KeyMessage(w, l).Key;
                    var (hi, lo) = Split(key);
                    KeyState[hi] &= ~lo;
                    OnKeyUp(key);
                    return 0;
                }
            case WinMessage.Paint:
                if (running && !painting) {
                    var ps = new PaintStruct();
                    painting = true;
                    var dc = User32.BeginPaint(WindowHandle, ref ps);
                    OnPaint(dc, ps.paint);
                    User32.EndPaint(WindowHandle, ref ps);
                    painting = false;
                }
                return 0;
        }
        return User32.DefWindowProcW(h, m, w, l);
    }

    private Font font;
    public Font Font {
        get =>
            font ??= new("data/ibm_3270.txt");
        set =>
            font = value;
    }

    static (int h, int l) Split (Keys k) =>
        ((int)k >> 5, 1 << ((int)k & 31));

    static Vector2i Split (nint l) {
        var i = (int)(l & int.MaxValue);
        return new(i & ushort.MaxValue, (i >> 16) & ushort.MaxValue);
    }
}
