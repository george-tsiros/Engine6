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

    public static IEnumerable<string> EnumLines (string filepath, bool skipBlank = false) {
        using var f = new StreamReader(filepath);
        while (f.ReadLine() is string line)
            if (!skipBlank || !string.IsNullOrWhiteSpace(line))
                yield return line;
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
    private static void PushAscii (Span<byte> a, ref long int64, ref int offset) {
        int64 = Math.DivRem(int64, 10, out var d);
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
            angle += 2 * (float)Math.PI;
        while (angle > 2 * Math.PI)
            angle -= 2 * (float)Math.PI;
        return angle;
    }
    internal static double ModuloTwoPi (ref double angle, double delta) {
        angle += delta;
        while (angle < 0)
            angle += 2 * Math.PI;
        while (angle > 2 * Math.PI)
            angle -= 2 * Math.PI;
        return angle;
    }
    internal static float Clamp (ref float angle, float delta, float min, float max) => angle = (float)Math.Max(min, Math.Min(angle + delta, max));
    internal static double Clamp (ref double angle, double delta, double min, double max) => angle = Math.Max(min, Math.Min(angle + delta, max));

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
