namespace Win32;

internal  struct KeyMessage {
    public short repeatCount ;
    public Keys key ;
    public bool wasDown;
    public KeyMessage (nuint w, nint l) {
        repeatCount = (short)(l & short.MaxValue);
        key = (Keys)(byte)(w & byte.MaxValue);
        wasDown = (l & 0x40000000) != 0;
    }
}
