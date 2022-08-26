namespace Win32;
using System;

[Flags]
public enum WindowPosFlags:uint {
    None = 0x0,
    NoSize = 0x1,
    NoMove = 0x2,
    NoZOrder = 0x4,
    NoRedraw = 0x8,
    NoActivate = 0x10,
    DrawFrame = 0x20,
    ShowWindow = 0x40,
    HideWindow = 0x80,
    NoCopyBits = 0x100,
    NoOwnerZOrder = 0x200,
    NoSendChanging = 0x400,
    DeferErase = 0x2000,
    AsyncWindowPosition = 0x4000,
}
