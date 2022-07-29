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
    public override string ToString () =>
        $"{ToStr(flags)},{pixelType:x2},{rBits}/{gBits}/{bBits}/{aBits},{depthBits},{accRBits}/{accGBits}/{accBBits}/{accABits},{stencilBits},{auxBuffers},{layerType:x2},{layerMask:x2},{visibleMask:x2},{damageMask:x2}";

    static readonly ushort _size;
    static PixelFormatDescriptor () {
        _size = (ushort)Marshal.SizeOf<PixelFormatDescriptor>();
    }
    public static string ToStr (PixelFlags f) {
        var eh = ToFlags(f, out int unknown);
        var str = string.Join(" | ", eh);
        if (unknown != 0)
            str += $" | 0x{unknown:x}";
        return str;
    }

    public static ushort Size => _size;
        public static List<T> ToFlags<T> (T value, out int unknown) where T : Enum {
        Debug.Assert(typeof(T).IsEnum);
        if (typeof(T).GetCustomAttribute<FlagsAttribute>() is null)
            throw new ArgumentException();
        var list = new List<T>();
        var entries = Enum.GetValues(typeof(T)) as T[];
        var values = Array.ConvertAll(entries, e => (int)(object)e);
        var v = (int)(object)value;
        unknown = 0;
        for (var shift = 0; shift < 31; ++shift) {
            var i = 1 << shift;
            if ((i & v) != 0) {
                var idx = Array.IndexOf(values, i);
                if (idx < 0)
                    unknown |= i;
                else
                    list.Add(entries[idx]);
            }
        }
        return list;
    }

}
