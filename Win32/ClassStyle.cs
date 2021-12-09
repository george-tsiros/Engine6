namespace Win32;
using System;

[Flags]
public enum ClassStyle:uint {
    VRedraw = 1 << 0,
    HRedraw = 1 << 1,
    DoubleClicks = 1 << 3,
    OwnDc = 1 << 5,
    ClassDc = 1 << 6,
    ParentDc = 1 << 7,
    NoClose = 1 << 9,
    SaveBits = 1 << 11,
    ByteAlignClient = 1 << 12,
    ByteAlignWindow = 1 << 13,
    GlobalClass = 1 << 14,
    DropShadow = 1 << 17,
}
