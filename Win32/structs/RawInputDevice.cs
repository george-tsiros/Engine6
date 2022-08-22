namespace Win32;

using System;

public struct RawInputDevice {
    public ushort usagePage;
    public ushort usage;
    public uint flags;
    public IntPtr windowHandle;
    public const uint RemoveDevice = 1;
}
