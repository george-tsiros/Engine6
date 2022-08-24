namespace Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using static Common.Maths;

public static class Functions {
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
