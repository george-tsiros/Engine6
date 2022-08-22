namespace Win32;

using System;
using System.Diagnostics;

public delegate nint WndProc (IntPtr hWnd, WinMessage msg, nuint wparam, nint lparam);

public abstract class WindowBase:IDisposable {
    private const string ClassName = nameof(WindowBase);

    protected static readonly WndProc staticWndProc = StaticWndProc;
    protected static readonly ushort ClassAtom = User32.RegisterWindowClass(staticWndProc, ClassName);
    protected static readonly IntPtr SelfHandle = Kernel32.GetModuleHandleW(null);
    private static WindowBase instance;

    private static nint StaticWndProc (IntPtr h, WinMessage m, nuint w, nint l) {
        if (WinMessage.NCCREATE == m || WinMessage.Create == m)
            instance.WindowHandle = h;
        return instance.WndProc(h, m, w, l);
    }

    public IntPtr WindowHandle { get; private set; }
    public DeviceContext Dc { get; private set; }

    private void Destroy () {
        if (Dc is not null) {
            Dc.SetHandleAsInvalid();
            Dc = null;
        }
        if (IntPtr.Zero != WindowHandle) {
            User32.DestroyWindow(WindowHandle);
            WindowHandle = IntPtr.Zero;
        }
    }

    protected virtual WindowStyle Style => WindowStyle.ClipPopup;

    protected virtual void Create () {
        Destroy();
        instance = this;
        var eh = User32.CreateWindow(ClassAtom, SelfHandle, Style);
        Debug.Assert(eh == WindowHandle);
        Dc = new(WindowHandle);
    }

    public WindowBase () {
        if (instance is not null)
            throw new InvalidOperationException("only one window at a time");
        Create();
    }

    protected Rect rect;
    public Rect Rect => rect;

    protected abstract IntPtr WndProc (IntPtr hWnd, WinMessage msg, nuint w, nint l);

    private bool disposed;

    public void Dispose () {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual void Dispose (bool dispose) {
        if (dispose && !disposed) {
            disposed = true;
            Destroy();
        }
    }
}
