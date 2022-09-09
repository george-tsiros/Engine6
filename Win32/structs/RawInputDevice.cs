namespace Win32;

using System.Runtime.InteropServices;

public struct RawInputDevice {
    public ushort usagePage;
    public ushort usage;
    public RawInputDeviceFlag flags;
    public nint target;
    public const uint RemoveDevice = 1;
    public static readonly int Size = Marshal.SizeOf<RawInputDevice>();
}
