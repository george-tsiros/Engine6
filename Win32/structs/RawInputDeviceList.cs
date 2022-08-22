namespace Win32;

using System;
using System.Runtime.InteropServices;

public struct RawInputDeviceList {
    public IntPtr device;
    public RawInputDeviceType type;
    public static int Size => Marshal.SizeOf<RawInputDeviceList>();
}
