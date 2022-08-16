namespace Gl;

using System;
using Win32;

readonly struct KeyMessage {
    public short RepeatCount { get; }
    public Keys Key { get; }
    public bool WasDown { get; }
    public KeyMessage (nuint w, nint l) {
        RepeatCount = (short)(l & short.MaxValue);
        Key = (Keys)(byte)(w & byte.MaxValue);
        WasDown = (l & 0x40000000) != 0;
    }
}
