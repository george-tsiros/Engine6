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
    

    //static void WriteBmpHeader (System.IO.Stream stream, int width, int height) {
    //    stream.WriteByte((byte)'B');
    //    stream.WriteByte((byte)'M'); // 1
    //    var length = 0x36 + 4 * width * height;
    //    var zero = BitConverter.GetBytes(0);
    //    stream.Write(BitConverter.GetBytes(length), 0, sizeof(int)); // 5
    //    stream.Write(zero, 0, sizeof(int)); // 9
    //    stream.Write(BitConverter.GetBytes(0x36), 0, sizeof(int)); // 13
    //    stream.Write(BitConverter.GetBytes(0x28), 0, sizeof(int)); // 17
    //    stream.Write(BitConverter.GetBytes(width), 0, sizeof(int)); // 21
    //    stream.Write(BitConverter.GetBytes(height), 0, sizeof(int)); // 25
    //    stream.Write(BitConverter.GetBytes(0x200001), 0, sizeof(int)); // 29
    //    stream.Write(zero, 0, sizeof(int)); // 33
    //    stream.Write(zero, 0, sizeof(int)); // 37
    //    var ec4 = BitConverter.GetBytes(0xec4);
    //    stream.Write(ec4, 0, sizeof(int)); // 
    //    stream.Write(ec4, 0, sizeof(int)); // 
    //    stream.Write(zero, 0, sizeof(int));
    //    stream.Write(zero, 0, sizeof(int));
    //    //stream.Write(zero, 0, 3);
    //}

    //static void WriteBmp ((byte r, byte g, byte b, byte a)[] colors, int width, int height, byte[] pixels) {
    //    using var f = System.IO.File.Create("bmp.bmp");
    //    using var w = new System.IO.BinaryWriter(f);
    //    w.Write((byte)0x42);
    //    w.Write((byte)0x4d);
    //    var filesize = 0x36 + 256 * 4 + width * height;
    //    w.Write(filesize);
    //    w.Write(0);
    //    w.Write(0x436);
    //    w.Write(0x28);
    //    w.Write(width);
    //    w.Write(height);
    //    w.Write((byte)1);
    //    w.Write((byte)0);
    //    w.Write((byte)8);
    //    w.Write((byte)0);
    //    w.Write(0);
    //    w.Write(0);
    //    w.Write(0xec4);
    //    w.Write(0xec4);
    //    w.Write(0x100);
    //    w.Write(0x100);
    //    foreach (var (r, g, b, a) in colors) {
    //        w.Write(r);
    //        w.Write(g);
    //        w.Write(b);
    //        w.Write(a);
    //    }
    //    w.Write(pixels);
    //}

    public static IEnumerable<string> EnumLines (string filepath, bool skipBlank = false) {
        using var f = new StreamReader(filepath);
        return EnumLines(f, skipBlank);
    }

    public static IEnumerable<string> EnumLines (StreamReader f, bool skipBlank = false) { 
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
