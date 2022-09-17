namespace Win32;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct PixelFormatDescriptor {
    public ushort size = Size;
    public ushort version = 1;
    public PixelFlag flags;
    public byte pixelType;
    public byte colorBits;
    public byte rBits;
    public byte rShift;
    public byte gBits;
    public byte gShift;
    public byte bBits;
    public byte bShift;
    public byte aBits;
    public byte aShift;
    public byte accBits;
    public byte accRBits;
    public byte accGBits;
    public byte accBBits;
    public byte accABits;
    public byte depthBits;
    public byte stencilBits;
    public byte auxBuffers;
    public byte layerType;
#pragma warning disable IDE0044 // Add readonly modifier
    private byte unused;
#pragma warning restore IDE0044 // Add readonly modifier
    public uint layerMask;
    public uint visibleMask;
    public uint damageMask;
    public PixelFormatDescriptor () { }
    public override string ToString () =>
        $"{colorBits,2} {depthBits,2} {stencilBits,2} {visibleMask:x8} {string.Join(", ", Common.Functions.ToFlags(flags))}";

    public const string Header =
        "cl dt st    vmask flags";

    public static readonly ushort Size = (ushort)Marshal.SizeOf<PixelFormatDescriptor>();

}
