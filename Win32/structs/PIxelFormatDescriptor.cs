namespace Win32;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct PixelFormatDescriptor {
    public ushort size = Size;
    public ushort version = 1;
    public PixelFlag flags = PixelFlag.None;
    public byte pixelType = 0;
    public byte colorBits = 0;
    public byte rBits = 0;
    public byte rShift = 0;
    public byte gBits = 0;
    public byte gShift = 0;
    public byte bBits = 0;
    public byte bShift = 0;
    public byte aBits = 0;
    public byte aShift = 0;
    public byte accBits = 0;
    public byte accRBits = 0;
    public byte accGBits = 0;
    public byte accBBits = 0;
    public byte accABits = 0;
    public byte depthBits = 0;
    public byte stencilBits = 0;
    public byte auxBuffers = 0;
    public byte layerType = 0;
#pragma warning disable IDE0044 // Add readonly modifier
    private byte unused = 0;
#pragma warning restore IDE0044 // Add readonly modifier
    public uint layerMask = 0;
    public uint visibleMask = 0;
    public uint damageMask = 0;
    public PixelFormatDescriptor () { }
    public override string ToString () =>
        $"{colorBits,2} {depthBits,2} {stencilBits,2} {visibleMask:x8} {string.Join(", ", Common.Functions.ToFlags(flags))}";

    public const string Header =
        "cl dt st    vmask flags";

    public static readonly ushort Size = (ushort)Marshal.SizeOf<PixelFormatDescriptor>();

}
