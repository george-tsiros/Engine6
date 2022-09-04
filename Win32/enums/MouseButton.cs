namespace Win32;
using System;

[Flags]
public enum MouseButton:ushort {
    None = 0,
    Left = 0x1,
    Right = 0x2,
    Middle = 0x10,
    X1 = 0x20,
    X2 = 0x40,
}
