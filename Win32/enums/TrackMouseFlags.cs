namespace Win32;
using System;

[Flags]
public enum TrackMouseFlags:uint {
    Hover = 1,
    Leave = 2,
    NonClient = 0x10,
    Query = 0x40000000,
    Cancel = 0x80000000,
}
