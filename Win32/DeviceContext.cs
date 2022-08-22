namespace Win32;

using System.Runtime.InteropServices;
using System;

public class DeviceContext:SafeHandle {
    private readonly IntPtr WindowHandle;

    public DeviceContext (IntPtr hwnd): base(User32.GetDC(hwnd), true) {
        WindowHandle = hwnd;
    }
    public override bool IsInvalid => IntPtr.Zero == handle;

    public static explicit operator IntPtr (DeviceContext dc) => !dc.IsInvalid ? dc.handle : throw new InvalidOperationException();

    protected override bool ReleaseHandle () => User32.ReleaseDC(WindowHandle, handle);
}
