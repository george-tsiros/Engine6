namespace Win32;

using System;

[Flags]
public enum PeekRemove:uint {
    NoRemove,
    Remove,
    NoYield,
}
