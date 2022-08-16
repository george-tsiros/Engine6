namespace Gl;

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using Win32;
using Linear;

public abstract class BaseWindow:IDisposable {
    const string ClassName = "MYWINDOWCLASS";

    protected static readonly WndProc staticWndProc;
    protected static readonly ushort ClassAtom;
    protected static readonly IntPtr SelfHandle = Kernel32.GetModuleHandleA(null);
    static readonly Dictionary<IntPtr, BaseWindow> Windows = new();
    static BaseWindow () {
        staticWndProc = new(StaticWndProc);
        ClassAtom = User32.RegisterWindowClass(staticWndProc, ClassName);
    }
    static BaseWindow instanceUnderCreation;
    static nint StaticWndProc (IntPtr h, WinMessage m, nuint w, nint l) {
        if (instanceUnderCreation is not null && (WinMessage.NCCREATE == m || WinMessage.Create == m)) {
            Windows.Add(h, instanceUnderCreation);
            instanceUnderCreation = null;
        }

        if (Windows.TryGetValue(h, out var instance)) {
            if (WinMessage.NCDESTROY == m)
                _ = Windows.Remove(h);
            return instance.WndProc(h, m, w, l);
        } else
            throw new Exception($"no entry with handle 0x{h:x}");
    }

    public IntPtr WindowHandle { get; protected set; }
    protected IntPtr DeviceContext;

    protected virtual void Recreate () {
        Destroy();
        Create();
    }
    
    private void Destroy () {
        if (!User32.ReleaseDC(WindowHandle, DeviceContext))
            throw new WinApiException(nameof(User32.ReleaseDC));
        if (!User32.DestroyWindow(WindowHandle))
            throw new WinApiException(nameof(User32.DestroyWindow));
    }
    
    private void Create () {
        instanceUnderCreation = this;
        WindowHandle = User32.CreateWindow(ClassAtom, SelfHandle);
        DeviceContext = User32.GetDC(WindowHandle);
    }
    
    public BaseWindow () {
        Create();
    }

    protected Rect rect;
    public Rect Rect => rect;

    abstract protected IntPtr WndProc (IntPtr hWnd, WinMessage msg, nuint w, nint l);

    private string text;
    public string Text {
        get =>
            text;
        set {
            if (value != text)
                _ = User32.SetWindowText(WindowHandle, text = value);
        }
    }

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
            var m = $"{f.GetFileName()}({f.GetFileLineNumber()},{f.GetFileColumnNumber()}): {message ?? "?"} ({Kernel32.GetLastError():X})";
            throw new Exception(m);
        }
    }

    protected static void WriteLine (object ob) {
        if (Debugger.IsAttached)
            Debug.WriteLine(ob);
        else
            Console.WriteLine(ob);
    }
}
