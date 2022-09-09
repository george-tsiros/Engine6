namespace Win32;

using System;

[Flags]
public enum RawInputDeviceFlag:uint {
    Remove = 0x00000001,
    Exclude = 0x00000010,
    PageOnly = 0x00000020,
    NoLegacy = 0x00000030,
    InputSink = 0x00000100,
    CaptureMouse = 0x00000200,
    AppKeys = 0x00000400,
    ExInputSink = 0x00001000,
    DevNotify = 0x00002000,
}
