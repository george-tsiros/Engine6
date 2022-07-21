namespace Gl;

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using Win32;

public abstract class WindowBase:IDisposable {

    private bool disposed;

    public void Dispose () {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected List<IDisposable> Disposables { get; } = new();

    public void Dispose (bool dispose) {
        if (dispose && !disposed) {
            disposed = true;
            foreach (var disposable in Disposables)
                disposable.Dispose();

            Demand(User.DestroyWindow(WindowHandle));
            Demand(User.UnregisterClassW(new IntPtr(ClassAtom), IntPtr.Zero));
            Instance = null;
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
    protected static WindowBase Instance;

    private static IntPtr StaticWndProc (IntPtr hWnd, WinMessage msg, IntPtr w, IntPtr l) {
        if (msg == WinMessage.NCCREATE)
            Instance.WindowHandle = hWnd;
        return Instance.WndProc(hWnd, msg, w, l);
    }

    protected readonly WndProc wndProc;
    protected int Width { get; init; }
    protected int Height { get; init; }
    protected ushort ClassAtom { get; }
    protected IntPtr WindowHandle { get; set; }
    protected static readonly IntPtr SelfHandle = Kernel.GetModuleHandleW(null);
    protected virtual string ClassName => "MYWINDOWCLASS";

    public WindowBase (Vector2i size) {
        if (Instance is not null)
            throw new Exception("instance exists");
        Instance = this;
        if (size.X < 1 || size.Y < 1)
            throw new ArgumentOutOfRangeException(nameof(size));
        wndProc = new(StaticWndProc);
        ClassAtom = User.RegisterWindowClass(wndProc, ClassName);
        User.CreateWindow(ClassAtom, size, SelfHandle);
        (Width, Height) = size;
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
