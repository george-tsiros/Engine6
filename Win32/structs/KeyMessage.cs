namespace Win32;

internal readonly struct KeyMessage {
    public readonly short RepeatCount;
    public readonly Key Key;
    public readonly bool WasDown;
    public KeyMessage (nuint w, nint l) {
        RepeatCount = (short)(l & short.MaxValue);
        Key = (Key)(byte)(w & byte.MaxValue);
        WasDown = (l & 0x40000000) != 0;
    }
}
