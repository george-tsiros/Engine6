namespace Win32;

using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class Win32Window:IDisposable {
    private const string ClassName = nameof(Win32Window);

    private static readonly Dictionary<nint,Win32Window> Windows=new();
    private static readonly WndProc staticWndProc = StaticWndProc;
    private static readonly ushort ClassAtom;
    private static readonly IntPtr SelfHandle = Kernel32.GetModuleHandle(null);
    private static Win32Window creating;

    static Win32Window () {
        var wc = new WindowClassW() {
            style = ClassStyle.None,
            wndProc = staticWndProc,
            hCursor = User32.LoadCursor(SystemCursor.Arrow),
            classname = nameof(Win32Window),
        };
        ClassAtom = User32.RegisterClass(ref wc);
    }

    public IntPtr WindowHandle { get; private set; }
    public WindowStyle Style { get; init; } = WindowStyle.OverlappedWindow;
    private readonly WndProc WndProc;

    public Win32Window (WndProc proc, Vector2i size) {
        WndProc = proc;
        creating = this;
        var eh = User32.CreateWindow(ClassAtom, size.X, size.Y, SelfHandle, Style);
        Debug.Assert(eh == WindowHandle);
    }

    private static nint StaticWndProc (IntPtr h, WinMessage m, nuint w, nint l) {
        if (WinMessage.Create == m)
            Windows.Add(creating.WindowHandle = h, creating);
        return creating.WndProc(h, m, w, l);
    }

    private bool disposed;

    public void Dispose () {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual void Dispose (bool dispose) {
        if (dispose && !disposed) {
            disposed = true;
            _ = Windows.Remove(WindowHandle);
            User32.DestroyWindow(WindowHandle);
        }
    }
}
