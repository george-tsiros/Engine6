namespace Engine;

using System;
using System.Reflection;
using System.IO;
#if !DEBUG
using System.Runtime.CompilerServices;
#endif
using System.Diagnostics;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Collections.Generic;

static class Extra {

    public static IEnumerable<string> EnumLines (StreamReader f, bool skipBlank = false) { 
        while (f.ReadLine() is string line)
            if (!skipBlank || !string.IsNullOrWhiteSpace(line))
                yield return line;
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
    private static void PushAscii (Span<byte> a, ref long int64, ref int offset) {
        (int64, var d) = long.DivRem(int64, 10);
        a[--offset] = (byte)(d + '0');
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
    internal static int ToChars (long int64, Span<byte> bytes) {
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


    internal static float ModuloTwoPi (ref float angle, float delta) {
        angle += delta;
        while (angle < 0)
            angle += 2 * float.Pi;
        while (angle > 2 * float.Pi)
            angle -= 2 * float.Pi;
        return angle;
    }
    internal static double ModuloTwoPi (ref double angle, double delta) {
        angle += delta;
        while (angle < 0)
            angle += 2 * double.Pi;
        while (angle > 2 * double.Pi)
            angle -= 2 * double.Pi;
        return angle;
    }

    internal static (int min, float max) Extrema (int[] ints) {
        var (min, max) = (int.MaxValue, int.MinValue);
        for (var i = 0; i < ints.Length; ++i)
            Extrema(ints[i], ref min, ref max);
        return (min, max);
    }
    internal static void Extrema (int i, ref int min, ref int max) {
        if (i < min)
            min = i;
        if (max < i)
            max = i;
    }

    internal static (double a, double b) Lin (double min, double max, double from, double to) {
        /*
        a * min + b = from
        a * max + b = to
        a (max-min) = to-from
        a = to-from/(max-min)
        b = to - a * max
*/
        var a = (to - from) / (max - min);
        return (a, to - a * max);

    }

    internal static (float min, float max) Extrema (float[] ycoords) {
#if !true
            var min = new Vector<float>(float.MaxValue);
            var max = new Vector<float>(float.MinValue);
            var total = ycoords.Length;
            var vectored = total & 0xfffffff8;
            for (var i = 0; i < vectored; i += 8) {
                var y = new Vector<float>(ycoords, i);
                min = Vector.Min(min, y);
                max = Vector.Max(max, y);
            }
            return (0f, 100f);
#else
        var min = float.MaxValue;
        var max = float.MinValue;
        for (var i = 0; i < ycoords.Length; i++)
            Extrema(ycoords[i], ref min, ref max);
        return (min, max);
#endif
    }
#if !DEBUG
    [MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
    private static void Extrema (float l, ref float min, ref float max) {
        if (l < min)
            min = l;
        if (max < l)
            max = l;
    }
}
