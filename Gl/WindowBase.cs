namespace Gl;

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using Win32;

public abstract class WindowBase:IDisposable {
    static WindowBase () {
        Windows = new();
        staticWndProc = new(StaticWndProc);
        ClassAtom = User.RegisterWindowClass(staticWndProc, ClassName);
    }
    private static readonly Dictionary<IntPtr, WindowBase> Windows;
    private bool disposed;

    public void Dispose () {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected List<IDisposable> Disposables { get; } = new();

    public void Dispose (bool dispose) {
        if (dispose && !disposed) {
            disposed = true;
            if (!cursorVisible)
                _ = User.ShowCursor(true);
            foreach (var disposable in Disposables)
                disposable.Dispose();

            Demand(User.DestroyWindow(WindowHandle));

        }
    }

    /*
    CreateWindowEx(0, eh, NULL, WS_OVERLAPPEDWINDOW, 0, 0, 800, 600, NULL, NULL, GetModuleHandle(NULL), (LPVOID)this);

LRESULT CALLBACK TWindow::staticWndProc(HWND h, UINT m, WPARAM w, LPARAM l) {
    TWindow* self;
    if (m == WM_NCCREATE) {
        LPCREATESTRUCT lpcs = reinterpret_cast<LPCREATESTRUCT>(l);
        self = static_cast<TWindow*>(lpcs->lpCreateParams);
        self->windowHandle = h;
        SetWindowLongPtr(h, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(self));
    } else {
        self = reinterpret_cast<TWindow*>(GetWindowLongPtr(h, GWLP_USERDATA));
    }
    if (self)
        return self->WndProc(m, w, l);
    else
        return DefWindowProc(h, m, w, l);
}*/

    private static IntPtr StaticWndProc (IntPtr hWnd, WinMessage msg, IntPtr w, IntPtr l) =>
        Instance.WndProc(hWnd, msg, w, l);

    private string text;
    protected string Text {
        get {
            return text;
        }

        set {
            if (text != value)
                _ = User.SetWindowText(WindowHandle, text = value);
        }
    }

    protected static readonly WndProc staticWndProc;
    protected Vector2i Size { get; }
    protected int Width => Size.X;
    protected int Height => Size.Y;
    protected static readonly ushort ClassAtom;
    protected IntPtr WindowHandle { get; set; }
    protected static readonly IntPtr SelfHandle = Kernel.GetModuleHandleW(null);
    const string ClassName = "MYWINDOWCLASS";
    private bool cursorVisible = true;

    protected bool CursorVisible {
        get => cursorVisible;
        set {
            if (value != cursorVisible)
                _ = User.ShowCursor(cursorVisible = value);
        }
    }
    static WindowBase Instance;
    public WindowBase (Vector2i size) {
        if (size.X < 1 || size.Y < 1)
            throw new ArgumentOutOfRangeException(nameof(size));
        Instance = this;

        WindowHandle = User.CreateWindow(ClassAtom, size, SelfHandle);
        Size = size;
    }

    abstract protected IntPtr WndProc (IntPtr hWnd, WinMessage msg, IntPtr w, IntPtr l);
    protected event Action OnClosing;

    public static IntPtr Demand (IntPtr p) {
        Demand(IntPtr.Zero != p);
        return p;
    }

    public static int Demand (int p) {
        Demand(0 != p);
        return p;
    }

    public static void Demand (bool condition, string message = null) {
        if (!condition) {
            var stackFrame = new StackFrame(1, true);
            var m = $">{stackFrame.GetFileName()}({stackFrame.GetFileLineNumber()},{stackFrame.GetFileColumnNumber()}): {message ?? "?"} ({Kernel.GetLastError():X})";
            if (Debugger.IsAttached)
                throw new Exception(m);
            else
                Console.WriteLine(m);
        }
    }
}
