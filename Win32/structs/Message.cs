namespace Win32;

using System;

public struct Message {
    public IntPtr hWnd;
    public WinMessage msg;
    public IntPtr wparam;
    public IntPtr lparam;
    public IntPtr result;
    public uint time;
    public int px;
    public int py;
}
