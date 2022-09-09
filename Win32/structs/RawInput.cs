namespace Win32;

using System.Runtime.InteropServices;

public struct RawInput {
    public RawInputHeader header;
    public RawMouse mouse;
    public static readonly int Size = Marshal.SizeOf<RawInput>();
}
