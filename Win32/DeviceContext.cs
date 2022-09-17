namespace Win32;

using System.Runtime.InteropServices;
using System;

public class DeviceContext:SafeHandle {
    private readonly nint WindowHandle;

    public DeviceContext (nint hwnd) : base(User32.GetDC(hwnd), true) {
        WindowHandle = hwnd;
    }
    public override bool IsInvalid => 0 == (nint)handle;

    public static explicit operator nint (DeviceContext dc) => !dc.IsInvalid ? dc.handle : throw new InvalidOperationException();

    protected override bool ReleaseHandle () => User32.ReleaseDC(WindowHandle, handle);
}
