namespace Win32;

public struct Message {
    public nint hWnd;
    public WinMessage msg;
    public nint wparam;
    public nint lparam;
    public nint result;
    public uint time;
    public int px;
    public int py;
}
