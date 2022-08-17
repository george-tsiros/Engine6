namespace Win32;

using System;
using System.Diagnostics;

public abstract class BaseWindow:IDisposable {
    private const string ClassName = "MYWINDOWCLASS";

    protected static readonly WndProc staticWndProc = StaticWndProc;
    protected static readonly ushort ClassAtom = User32.RegisterWindowClass(staticWndProc, ClassName);
    protected static readonly IntPtr SelfHandle = Kernel32.GetModuleHandleA(null);
    private static BaseWindow instance;

    private static nint StaticWndProc (IntPtr h, WinMessage m, nuint w, nint l) {
        if (WinMessage.NCCREATE == m || WinMessage.Create == m)
            instance.WindowHandle = h;
        return instance.WndProc(h, m, w, l);
    }

    public IntPtr WindowHandle { get; private set; }
    public IntPtr DeviceContext { get; private set; }

    private void Destroy () {
        if (IntPtr.Zero != DeviceContext) {
            if (!User32.ReleaseDC(WindowHandle, DeviceContext))
                throw new WinApiException(nameof(User32.ReleaseDC));
            DeviceContext = IntPtr.Zero;
        }
        if (IntPtr.Zero != WindowHandle) {
            User32.DestroyWindow(WindowHandle);
                WindowHandle = IntPtr.Zero;
        }
    }

    protected virtual void Create () {
        Destroy();
        instance = this;
        var eh = User32.CreateWindow(ClassAtom, SelfHandle);
        Debug.Assert(eh == WindowHandle);
        DeviceContext = User32.GetDC(WindowHandle);
    }

    public BaseWindow () {
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
