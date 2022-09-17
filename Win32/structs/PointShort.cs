namespace Win32;

public struct PointShort {
    public short x = 0, y = 0;
    public PointShort () { }
    public PointShort (nint l) {
        x = (short)(l & ushort.MaxValue);
        y = (short)((l >> 16) & ushort.MaxValue);
    }
}
