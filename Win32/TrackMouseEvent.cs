namespace Win32;

using System;
using System.Runtime.InteropServices;

public struct TrackMouseEvent {
    public uint size; // unsigned long
    public uint flags;
    public IntPtr track;
    public uint hoverTime;
    public static TrackMouseEvent Create () => new () { size = (uint)Marshal.SizeOf<TrackMouseEvent>() };
}
