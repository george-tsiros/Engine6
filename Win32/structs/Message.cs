namespace Win32;

using System;

public struct Message {
    public IntPtr hWnd;
    public uint msg;
    public IntPtr wparam;
    public IntPtr lparam;
    public IntPtr result;
    public int time;
    public int px;
    public int py;
    public int lprivate;
}
