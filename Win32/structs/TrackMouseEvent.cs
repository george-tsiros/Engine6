namespace Win32;

using System;
using System.Runtime.InteropServices;



public struct TrackMouseEvent {
    /// <summary>
    /// size of the <see cref="TrackMouseEvent"/> structure, in bytes. Constant.
    /// </summary>
    public int size;
    /// <summary>
    /// The services requested. This is a combination of <seealso cref="TrackMouseFlags"/>.
    /// </summary>
    public TrackMouseFlags flags;
    public IntPtr window;
    public uint hoverTime;
    public static int Size =>
        Marshal.SizeOf<TrackMouseEvent>();
}
