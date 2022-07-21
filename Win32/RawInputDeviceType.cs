namespace Win32;

using System;
[Flags]
public enum RawInputDeviceType:uint {
    Mouse,
    Keyboard,
    Hid,
}
