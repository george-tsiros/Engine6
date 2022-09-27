using System.Runtime.InteropServices;

namespace Win32;

public unsafe struct MonitorInfoExA {
    const int CCHDEVICENAME = 32;
    public uint size;
    public Rectangle entireDisplay;
    public Rectangle workArea;
    public uint flags;
    public fixed byte name[CCHDEVICENAME];
    public static uint Size { get; } = (uint)Marshal.SizeOf<MonitorInfoExA>();
}
