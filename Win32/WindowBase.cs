namespace Win32;

using Common;
using System;
using System.Diagnostics;

public abstract class WindowBase:IDisposable {
    private const string ClassName = nameof(WindowBase);

    protected static readonly WndProc staticWndProc = StaticWndProc;
    protected static readonly ushort ClassAtom;
    protected static readonly IntPtr SelfHandle = Kernel32.GetModuleHandleA(null);
    private static WindowBase instance;

    static WindowBase () {
        var wc = new WindowClassA() {
            style = ClassStyle.VRedraw | ClassStyle.HRedraw,
            wndProc = staticWndProc,
            classname = nameof(WindowBase),
        };
        ClassAtom = User32.RegisterClass(ref wc);
    }

    private static nint StaticWndProc (IntPtr h, WinMessage m, nuint w, nint l) {
        if (WinMessage.NcCreate == m || WinMessage.Create == m)
            instance.WindowHandle = h;
        return instance.WndProc(h, m, w, l);
    }

    public IntPtr WindowHandle { get; private set; }
    public DeviceContext Dc { get; private set; }

    private void Destroy () {
        if (Dc is not null) {
            Dc.Close();
            Dc = null;
        }
        if (IntPtr.Zero != WindowHandle) {
            User32.DestroyWindow(WindowHandle);
            WindowHandle = IntPtr.Zero;
        }
        instance = null;
    }

    protected virtual WindowStyle Style => WindowStyle.ClipPopup;

    protected virtual void Create () {
        Destroy();
        instance = this;
        var eh = User32.CreateWindow(ClassAtom, clientSize.X, clientSize.Y, SelfHandle, Style);
        Debug.Assert(eh == WindowHandle);
        Dc = new(WindowHandle);
    }
    readonly Vector2i clientSize;
    public WindowBase (Vector2i? size = null) {
        clientSize = size ?? new(640, 480);
        if (instance is not null)
            throw new InvalidOperationException("only one window at a time");
        Create();
    }

    protected Rectangle Rect { get; set; }

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
