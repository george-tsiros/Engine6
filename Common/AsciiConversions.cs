namespace Common;

using System;
using System.Diagnostics;

internal static class AsciiConversions {
    public static Ascii IntPtr (nint iptr) => throw new NotImplementedException();
    public static Ascii UIntPtr (nuint uptr) => throw new NotImplementedException();
    public static Ascii Int16 (short i16) => Int64(i16);

    public static Ascii Int32 (int i32) => Int64(i32);

    public static Ascii Int64 (long i64) {
        Span<byte> bytes = stackalloc byte[20];
        var isNegative = false;
        var i = 20;

        do {
            (i64, var x) = Maths.Int64DivRem(i64, 10l);
            if (x < 0l || i64 < 0l) {
                isNegative = true;
                x = -x;
                i64 = -i64;
            }
            bytes[--i] = (byte)((int)x + Zero);
        } while (0l != i64);

        if (isNegative)
            bytes[--i] = Minus;

        return new(bytes[i..]);
    }

    public static Ascii UInt16 (ushort u16) => UInt64(u16);

    public static Ascii UInt32 (uint u32) => UInt64(u32);

    public static Ascii UInt64 (ulong u64) {
        Span<byte> bytes = stackalloc byte[20];
        var i = 20;
        do {
            (u64, var x) = Maths.UInt64DivRem(u64, 10ul);
            bytes[--i] = (byte)(x + Zero);
        } while (0ul != u64);
        return new(bytes[i..]);
    }

    public static Ascii DateTime (DateTime dt) => throw new NotImplementedException();
    public static Ascii TimeSpan (TimeSpan ts) => throw new NotImplementedException();
    public static Ascii Guid (Guid guid) => throw new NotImplementedException();

    private const int Zero = '0';
    private const byte Minus = (byte)'-';

}
