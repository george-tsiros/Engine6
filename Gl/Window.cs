namespace Gl;

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using Win32;
using Linear;

public abstract class Window:IDisposable {
    private static IntPtr StaticWndProc (IntPtr hWnd, WinMessage msg, IntPtr w, IntPtr l) =>
        Instance.WndProc(hWnd, msg, w, l);

    protected static void WriteLine (object ob) {
        if (Debugger.IsAttached)
            Debug.WriteLine(ob);
        else 
            Console.WriteLine(ob);
    }

    protected static readonly WndProc staticWndProc;
    protected static readonly ushort ClassAtom;
    protected static readonly IntPtr SelfHandle = Kernel.GetModuleHandleW(null);

    static Window () {
        staticWndProc = new(StaticWndProc);
        ClassAtom = User.RegisterWindowClass(staticWndProc, ClassName);
    }
    private bool disposed;

    public void Dispose () {
        Dispose(true);
        GC.SuppressFinalize(this);
    }


    public virtual void Dispose (bool dispose) {
        if (dispose && !disposed) {
            disposed = true;
            if (!cursorVisible)
                _ = User.ShowCursor(true);
            Demand(User.DestroyWindow(WindowHandle));
        }
    }

    static Window Instance;

    private string text;
    public string Text {
        get =>
            text;
        set {
            if (value != text)
                _ = User.SetWindowText(WindowHandle, text = value);
        }
    }

    private Font font;
    public Font Font {
        get =>
            font ??= new("data/ibm_3270.txt");
        set =>
            font = value;
    }

    public Vector2i Size { get; }
    public int Width => Size.X;
    public int Height => Size.Y;
    public IntPtr WindowHandle { get; protected set; }
    const string ClassName = "MYWINDOWCLASS";
    private bool cursorVisible = true;
    protected IntPtr DeviceContext;

    public bool CursorVisible {
        get =>
            cursorVisible;
        set {
            if (value != cursorVisible)
                _ = User.ShowCursor(cursorVisible = value);
        }
    }

    public Window (Vector2i size, Vector2i? position = null) {

        if (size.X < 1 || size.Y < 1)
            throw new ArgumentOutOfRangeException(nameof(size));
        Instance = this;
        WindowHandle = User.CreateWindow(ClassAtom, new(position ?? new(), size), SelfHandle);
        DeviceContext = User.GetDC(WindowHandle);
        Size = size;
    }

    abstract protected IntPtr WndProc (IntPtr hWnd, WinMessage msg, IntPtr w, IntPtr l);

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
            var f = new StackFrame(1, true);
            var m = $">{f.GetFileName()}({f.GetFileLineNumber()},{f.GetFileColumnNumber()}): {message ?? "?"} ({Kernel.GetLastError():X})";
            throw new Exception(m);
        }
    }
}
