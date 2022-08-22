namespace Win32;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public struct RawInputDeviceInfo {
    [FieldOffset(0)]
    public uint size;
    [FieldOffset(sizeof(uint))]
    public RawInputDeviceType type;
    [FieldOffset(2 * sizeof(uint))]
    public MouseInfo mouse;
    [FieldOffset(2 * sizeof(uint))]
    public KeyboardInfo keyboard;
    [FieldOffset(2 * sizeof(uint))]
    public HidInfo hid;
}
