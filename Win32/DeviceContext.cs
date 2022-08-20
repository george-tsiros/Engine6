namespace Win32;

using System.Runtime.InteropServices;
using System;

public class DeviceContext:SafeHandle {
    private const string dll = "user32.dll";
    [DllImport(dll, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    private static extern IntPtr GetDC (IntPtr windowHandle);

    [DllImport(dll, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ReleaseDC (IntPtr hwnd, IntPtr dc);

    readonly IntPtr WindowHandle;
    public DeviceContext (IntPtr hwnd): base(GetDC(hwnd), true) {
        WindowHandle = hwnd;
    }
    public override bool IsInvalid => IntPtr.Zero == handle;

    public static explicit operator IntPtr (DeviceContext dc) => !dc.IsInvalid ? dc.handle : throw new InvalidOperationException();

    protected override bool ReleaseHandle () => ReleaseDC(WindowHandle, handle);
}
