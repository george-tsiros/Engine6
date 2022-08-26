namespace Win32;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct PixelFormatDescriptor {
    public ushort size;
    public ushort version;
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
    private byte unused;
    public uint layerMask;
    public uint visibleMask;
    public uint damageMask;
    public override string ToString () =>
        $"pt:{pixelType} clr:{colorBits,2} dpt:{depthBits,2} acc:{accBits,2} stn:{stencilBits,2} {visibleMask:x8} {ToStr(flags)}";

    public static string ToStr (PixelFlag f) {
        var eh = Common.Functions.ToFlags(f, out int unknown);
        var str = string.Join(',', eh);
        if (unknown != 0)
            str += $",0x{unknown:x}";
        return str;
    }

    public static ushort Size { get; } = 
        (ushort)Marshal.SizeOf<PixelFormatDescriptor>();

}
