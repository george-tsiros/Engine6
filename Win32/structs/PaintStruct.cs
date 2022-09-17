namespace Win32;

public unsafe struct PaintStruct {
    public nint deviceContext;
    public int erase;
    public Rectangle rect;
    public int restore;
    public int incUpdate;
    public fixed byte reserved[32];
}
