namespace Win32;
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct PixelFormatDescriptor {
    public ushort size;
    public ushort version;
    public PixelFlags flags;
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
    public override string ToString () {
        return $"{flags},{pixelType:x2},{rBits}/{gBits}/{bBits}/{aBits},{depthBits},{accRBits}/{accGBits}/{accBBits}/{accABits},{stencilBits},{auxBuffers},{layerType:x2},{layerMask:x2},{visibleMask:x2},{damageMask:x2}";

        //return $"{flags},color bits {colorBits} (rgba bits,shift) ({rBits},{rShift}/{gBits},{gShift}/{bBits},{bShift}/{aBits},{aShift}) depth {depthBits}, accum rgba {accRBits}/{accGBits}/{accBBits}/{accABits}, stencil {stencilBits}, aux {auxBuffers}, layertype {layerType} layermask {layerMask} visible mas {visibleMask} dmg mask {damageMask}";
    }
    public static int Size => Marshal.SizeOf<PixelFormatDescriptor>();
}
