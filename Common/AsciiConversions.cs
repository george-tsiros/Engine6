namespace Common;

using System;
using System.Diagnostics;

internal static class AsciiConversions {
    public static Ascii IntPtr (nint iptr) => throw new NotImplementedException();
    public static Ascii UIntPtr (nuint uptr) => throw new NotImplementedException();
    public static Ascii Int16 (short i16) => throw new NotImplementedException();
    public static Ascii Int32 (int i32) {
        Span<byte> bytes = stackalloc byte[11];
        var isNegative = false;
        var i = 11;
        do {
            (i32, var x) = Maths.IntDivRem(i32, 10);
            if (x < 0 || i32 < 0) {
                isNegative = true;
                x = -x;
                i32 = -i32;
            }
            bytes[--i] = (byte)(x + Zero);
        } while (0 != i32);

        if (isNegative)
            bytes[--i] = Minus;

        return new(bytes[i..]);
    }

    public static Ascii Int64 (long i64) {
        Span<byte> bytes = stackalloc byte[20];
        var isNegative = false;
        var i = 20;
        do {
            (i64, var x) = Maths.LongDivRem(i64, 10l);
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

    public static Ascii UInt16 (ushort u16) => throw new NotImplementedException();

    public static Ascii UInt32 (uint u32) {
        Span<byte> bytes = stackalloc byte[11];
        var i = 11;
        do {
            (u32, var x) = Maths.UIntDivRem(u32, 10u);
            bytes[--i] = (byte)(x + Zero);
        } while (0u != u32);
        return new(bytes[i..]);
    }

    public static Ascii UInt64 (ulong u64) => throw new NotImplementedException();
    public static Ascii DateTime (DateTime dt) => throw new NotImplementedException();
    public static Ascii TimeSpan (TimeSpan ts) => throw new NotImplementedException();
    public static Ascii Guid (Guid guid) => throw new NotImplementedException();

    private const int Zero = '0';
    private const byte Minus = (byte)'-';

}
