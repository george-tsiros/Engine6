namespace Win32;
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct PixelFormatDescriptor {
    public ushort structSize;
    public ushort Version;
    public PixelFlags Flags;
    public byte PixelType;
    public byte ColorBits;
    public byte RedBits;
    public byte RedShift;
    public byte GreenBits;
    public byte GreenShift;
    public byte BlueBits;
    public byte BlueShift;
    public byte AlphaBits;
    public byte AlphaShift;
    public byte AccumBits;
    public byte AccumRedBits;
    public byte AccumGreenBits;
    public byte AccumBlueBits;
    public byte AccumAlphaBits;
    public byte DepthBits;
    public byte StencilBits;
    public byte AuxBuffers;
    public byte LayerType;
    private byte Reserved;
    public uint LayerMask;
    public uint VisibleMask;
    public uint DamageMask;
    public static ushort Size => (ushort)Marshal.SizeOf<PixelFormatDescriptor>();
    public static readonly Predicate<PixelFormatDescriptor> Typical = d =>
d.RedBits == 8 &&
d.GreenBits == 8 &&
d.BlueBits == 8 &&
//d.DepthBits == 24 &&
d.Flags.HasFlag(PixelFlags.DrawToWindow | PixelFlags.SupportComposition | PixelFlags.DoubleBuffer | PixelFlags.SupportOpengl);
    public override string ToString () {
        return $"{Flags}, {RedBits}/{GreenBits}/{BlueBits}/{DepthBits}";
    }

    //Use this function to make a new one with Size and Version already filled in.
    public static PixelFormatDescriptor Create () {
        var pfd = new PixelFormatDescriptor {
            structSize = (ushort)Marshal.SizeOf<PixelFormatDescriptor>(),
            Version = 1
        };

        return pfd;
    }
}