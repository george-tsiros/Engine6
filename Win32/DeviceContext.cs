namespace Win32;

using System.Runtime.InteropServices;
using System;

public class DeviceContext:SafeHandle {
    private readonly nint WindowHandle;

    public DeviceContext (Window window) : this(window.Handle) {
    }

    public DeviceContext (nint windowHandle) : base(User32.GetDC(windowHandle), true) { 
        WindowHandle = windowHandle;
    
    }

    public override bool IsInvalid => 0 == (nint)handle;

    public static explicit operator nint (DeviceContext dc) => !dc.IsInvalid ? dc.handle : throw new InvalidOperationException();

    protected override bool ReleaseHandle () => User32.ReleaseDC(WindowHandle, handle);
}
