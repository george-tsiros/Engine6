namespace Win32;

using System.Runtime.InteropServices;

public struct RawInputHeader {
    public uint type;
    public uint size;
    public nint device;
    public nuint w;
    public static readonly int Size = Marshal.SizeOf<RawInputHeader>();
}
