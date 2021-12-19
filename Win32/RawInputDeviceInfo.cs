namespace Win32;

using System;
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
public struct MouseInfo {
    public uint id, buttonCount, samplerate;
    public IntPtr hasHorWheel;
}
public struct KeyboardInfo {
    public uint type, subtype, mode, fnKeyCount, ledCount, keyCount;

}
public struct HidInfo {
    public uint vendor, product, version;
    public ushort usagePage, usage;
}
