namespace Gl;

using System;
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
        }
    }

    protected readonly WndProc wndProc;
    protected int Width { get; init; }
    protected int Height { get; init; }
    protected ushort ClassAtom { get; }
    protected IntPtr WindowHandle { get; init; }
    protected static readonly IntPtr SelfHandle = Kernel.GetModuleHandleW(null);
    protected virtual string ClassName => "MYWINDOWCLASS";

    public WindowBase (Vector2i size) {
        if (size.X < 1 || size.Y < 1)
            throw new ArgumentOutOfRangeException(nameof(size));
        wndProc = new(WndProc);
        ClassAtom = User.RegisterWindowClass(wndProc, ClassName);
        WindowHandle = User.CreateWindow(ClassAtom, size, SelfHandle);
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
