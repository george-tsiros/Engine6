namespace Common;

using System;
using System.Numerics;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using static Common.Maths;

public static class Functions {

    public static IEnumerable<string> ToFlags<T> (T value) where T : Enum {
        Debug.Assert(typeof(T).IsEnum);
        if (typeof(T).GetCustomAttribute<FlagsAttribute>() is null)
            throw new ArgumentException("must have Flags Attribute", nameof(T));
        var type = typeof(T).GetEnumUnderlyingType();
        if (typeof(int) == type)
            return ToFlagsInt32(value);
        if (typeof(uint) == type)
            return ToFlagsUInt32(value);
        if (typeof(long) == type)
            return ToFlagsInt64(value);
        if (typeof(ulong) == type)
            return ToFlagsUInt64(value);
        if (typeof(short) == type)
            return ToFlagsInt16(value);
        if (typeof(ushort) == type)
            return ToFlagsUInt16(value);
        if (typeof(byte) == type)
            return ToFlagsByte(value);
        if (typeof(sbyte) == type)
            return ToFlagsSByte(value);
        throw new ArgumentException("not supported", nameof(value));
    }

    static IEnumerable<string> ToFlagsInt32<T> (T value) where T : Enum {
        for (int i32 = (int)(object)value; 0 != i32;) {
            var mask = 1 << BitOperations.TrailingZeroCount(i32);
            yield return EnumToStrInt32<T>(mask);
            i32 &= ~mask;
        }
    }

    static string EnumToStrInt32<T> (int value) where T : Enum =>
        Enum.IsDefined(typeof(T), value) ? ((T)Enum.ToObject(typeof(T), value)).ToString() : $"0x{value:x}";

    static string EnumToStrUInt32<T> (uint value) where T : Enum =>
        Enum.IsDefined(typeof(T), value) ? ((T)Enum.ToObject(typeof(T), value)).ToString() : $"0x{value:x}";

    static IEnumerable<string> ToFlagsUInt32<T> (T value) where T : Enum {
        for (uint u32 = (uint)(object)value; 0 != u32;) {
            var mask = 1u << BitOperations.TrailingZeroCount(u32);
            yield return EnumToStrUInt32<T>(mask);
            u32 &= ~mask;
        }
    }

    static IEnumerable<string> ToFlagsInt64<T> (T value) where T : Enum { throw new NotImplementedException(); }
    static IEnumerable<string> ToFlagsUInt64<T> (T value) where T : Enum { throw new NotImplementedException(); }
    static IEnumerable<string> ToFlagsInt16<T> (T value) where T : Enum { throw new NotImplementedException(); }
    static IEnumerable<string> ToFlagsUInt16<T> (T value) where T : Enum { throw new NotImplementedException(); }
    static IEnumerable<string> ToFlagsByte<T> (T value) where T : Enum { throw new NotImplementedException(); }
    static IEnumerable<string> ToFlagsSByte<T> (T value) where T : Enum { throw new NotImplementedException(); }

    public static IEnumerable<string> EnumLines (StreamReader f, bool skipBlank = false) {
        while (f.ReadLine() is string line)
            if (!skipBlank || !string.IsNullOrWhiteSpace(line))
                yield return line;
    }

    private static void PushAscii (Span<byte> a, ref long int64, ref int offset) {
        (int64, var d) = LongDivRem(int64, 10);
        a[--offset] = (byte)(d + '0');
    }

    public static int ToChars (long int64, Span<byte> bytes) {
        var isNegative = int64 < 0l;
        if (isNegative)
            int64 = -int64;
        var offset = 20;
        do
            PushAscii(bytes, ref int64, ref offset);
        while (int64 != 0);
        if (isNegative)
            bytes[--offset] = (byte)'-';
        return offset;
    }

    public static float ModuloTwoPi (ref float angle, float delta) {
        angle += delta;
        while (angle < 0)
            angle += fTau;
        while (angle > fTau)
            angle -= fTau;
        return angle;
    }

    public static double ModuloTwoPi (double angle, double delta) {
        var d = delta < 0 ? dTau - (-delta % dTau) : (delta % dTau);
        return (angle + d) % dTau;
    }

    public static (int min, float max) Extrema (int[] ints) {
        var (min, max) = (int.MaxValue, int.MinValue);
        for (var i = 0; i < ints.Length; ++i)
            Extrema(ints[i], ref min, ref max);
        return (min, max);
    }

    public static void Extrema (int i, ref int min, ref int max) {
        if (i < min)
            min = i;
        if (max < i)
            max = i;
    }

    public static (double a, double b) Lin (double min, double max, double from, double to) {
        // if 
        // a * min + b = from
        // a * max + b = to
        // then
        // a (max-min) = to-from
        // a = (to-from)/(max-min)
        // b = to - a * max

        var a = (to - from) / (max - min);
        return (a, to - a * max);

    }

    public static (float min, float max) Extrema (float[] ycoords) {
        var min = float.MaxValue;
        var max = float.MinValue;
        for (var i = 0; i < ycoords.Length; i++)
            Extrema(ycoords[i], ref min, ref max);
        return (min, max);
    }

    private static void Extrema (float l, ref float min, ref float max) {
        if (l < min)
            min = l;
        if (max < l)
            max = l;
    }
}
