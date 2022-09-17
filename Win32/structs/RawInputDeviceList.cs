namespace Win32;

using System.Runtime.InteropServices;

public struct RawInputDeviceList {
    public nint device;
    public RawInputDeviceType type;
    public static int Size => Marshal.SizeOf<RawInputDeviceList>();
}
