namespace Gl;

using System;
using Win32;

readonly struct KeyMessage {
    public short RepeatCount { get; }
    public Keys Key { get; }
    public bool WasDown { get; }
    public KeyMessage (IntPtr w, IntPtr l) {
        var wi = (uint)(w.ToInt64() & uint.MaxValue);
        var li = (uint)(l.ToInt64() & uint.MaxValue);
        RepeatCount = (short)(li & short.MaxValue);
        Key = (Keys)(byte)(wi & byte.MaxValue);
        WasDown = (li & 0x40000000) != 0;
    }
}
