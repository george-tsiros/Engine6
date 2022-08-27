namespace Win32;

internal struct KeyMessage {
    public short repeatCount;
    public Key key;
    public bool wasDown;
    public KeyMessage (nuint w, nint l) {
        repeatCount = (short)(l & short.MaxValue);
        key = (Key)(byte)(w & byte.MaxValue);
        wasDown = (l & 0x40000000) != 0;
    }
}
