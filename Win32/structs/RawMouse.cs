namespace Win32;

public struct RawMouse {
    public ushort flags;
    public uint buttons;
    public uint rawButtons;
    public int lastX;
    public int lastY;
    public uint extraInformation;
}
