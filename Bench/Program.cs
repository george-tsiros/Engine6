namespace Bench;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
#if !DEBUG
using System.Runtime.CompilerServices;
#endif
using System.Text;

class Program {
    private enum Kind:byte {
        Stamp,
        Enter,
        Leave,
    }
    private class BenchResult:IComparable<BenchResult> {
        public double Performance { get; }
        public string Message { get; }
        public BenchResult (double performance, string message) => (Performance, Message) = (performance, message);
        public int CompareTo (BenchResult other) => Performance.CompareTo(other.Performance);
        public override string ToString () => Message;
    }
#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
    private static void PushAscii (Span<byte> bytes, ref long int64, ref int offset) {
        int64 = Math.DivRem(int64, 10, out var d);
        bytes[--offset] = (byte)(d + '0');
    }
#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
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

    public static int ToCharsInlined (long int64, Span<byte> bytes) {
        var isNegative = int64 < 0l;
        if (isNegative)
            int64 = -int64;
        var offset = 20;
        do {
            int64 = Math.DivRem(int64, 10, out var d);
            bytes[--offset] = (byte)(d + '0');
        } while (int64 != 0);
        if (isNegative)
            bytes[--offset] = (byte)'-';
        return offset;
    }

    static void Main () {
        Console.WriteLine("press enter when debugger diagnostics wake up");
        _ = Console.ReadLine();
        Console.WriteLine("press any key to stop");
        TestOneUseStringMemoryPressure(TextWriter.Null);
        File.Delete("test.bin");
        _ = Console.ReadLine();
    }

    private static void TestOneUseStringMemoryPressure (TextWriter writer) {
        var r = new Random();
        while (!Console.KeyAvailable)
        for (var i = 0; i < 1000; ++i)
            writer.Write(RandomString(r));
    }

    private static void Trace () => Console.WriteLine(new StackFrame(1).GetMethod().Name);
    private static void Bench_Actual_Stream (string filename) {
        Trace();
        using var stream = File.Create(filename);
        Bench(stream);
    }
    private static void Bench_Null_Stream () {
        Trace();
        Bench(Stream.Null);
    }

    private static void Bench_StringFormat_Actual_StreamWriter () {
        Trace();
        using var writer = new StreamWriter("test.txt");
        Bench(writer);
    }
    private static void Bench_StringFormat_Null_StreamWriter () {
        Trace();
        Bench(StreamWriter.Null);
    }
    private static void Bench_Binary_Null () {
        Trace();
        Bench_Binary(Stream.Null);
    }
    private static void Bench_BinaryWriter (string filename, long count = 1000000l) {
        Trace();
        using BinaryWriter writer = new BinaryWriter(File.Create(filename));
        var kinds = new Kind[count];
        var longs = new long[count];
        var strings = new string[count];
        var foos = new FooEnum[count];
        var r = new Random();
        var results = new List<BenchResult>();
        do {
            for (var i = 0; i < count; ++i) {
                kinds[i] = (Kind)r.Next(0, 3);
                longs[i] = r.NextInt64(1_000_000, 1_000_000_000);
                strings[i] = r.Next(2) == 1 ? RandomString(r) : null;
                foos[i] = (FooEnum)r.Next(0, 4);
            }
            var (t0, t1) = (0l, 0l);
            t0 = Stopwatch.GetTimestamp();
            for (var i = 0; i < count; ++i)
                WithBinaryWriterSimplest(writer, longs[i], (int)foos[i]);
            t1 = Stopwatch.GetTimestamp();
            results.Add(new(1.0 / (t1 - t0), Format(t1 - t0, count, "FooEnum :")));

            t0 = Stopwatch.GetTimestamp();
            for (var i = 0; i < count; ++i)
                WithBinaryWriterUnsafe(writer, longs[i], (int)foos[i]);
            t1 = Stopwatch.GetTimestamp();
            results.Add(new(1.0 / (t1 - t0), Format(t1 - t0, count, "FooEnum :")));


            //results.Sort();
            Console.WriteLine(string.Join("\n", results));
            Console.WriteLine();
            results.Clear();

        } while (!Console.KeyAvailable);
    }
    unsafe private static void WithBinaryWriterUnsafe (BinaryWriter writer, long int64, int int32) {
        Span<byte> bytes = stackalloc byte[sizeof(long) + sizeof(byte)];
        fixed (byte* p = bytes) {
            *(long*)p = int64;
            p[+sizeof(long)] = (byte)int32;
        }
        writer.Write(bytes);
    }
    private static void WithBinaryWriterSimplest (BinaryWriter writer, long int64, int int32) {
        writer.Write(int64);
        writer.Write((byte)int32);
    }
    private static void Bench_Binary_Actual (string filename) {
        Trace();
        using Stream writer = File.Create(filename);
        Bench_Binary(writer);
    }

    private static int BestIndex<T> (List<T> list) where T : class, IComparable<T> {
        T best = null;
        var bestIndex = -1;
        var count = list.Count;
        for (var i = 0; i < count; ++i)
            if (best is null || list[i].CompareTo(best) > 0) {
                best = list[i];
                bestIndex = i;
            }
        return bestIndex;
    }
    private enum FooEnum:byte {
        A, B, C, D
    }
    private static void Bench_Binary (Stream writer, long count = 1000000l) {
        var kinds = new Kind[count];
        var longs = new long[count];
        var strings = new string[count];
        var foos = new FooEnum[count];
        var r = new Random();
        var results = new List<BenchResult>();
        do {
            for (var i = 0; i < count; ++i) {
                kinds[i] = (Kind)r.Next(0, 3);
                longs[i] = r.NextInt64(1_000_000, 1_000_000_000);
                strings[i] = r.Next(2) == 1 ? RandomString(r) : null;
                foos[i] = (FooEnum)r.Next(0, 4);
            }
            var (t0, t1) = (0l, 0l);
            //t0 = Stopwatch.GetTimestamp();
            //for (var i = 0; i < count; ++i)
            //    CastingSequential(writer, kinds[i], longs[i], strings[i]);
            //t1 = Stopwatch.GetTimestamp();
            //results.Add(new(1.0 / (t1 - t0), Format(t1 - t0, count, "casting sequential :")));

            //t0 = Stopwatch.GetTimestamp();
            //for (var i = 0; i < count; ++i)
            //    AsciiGetBytesSequential(writer, kinds[i], longs[i], strings[i]);
            //t1 = Stopwatch.GetTimestamp();
            //results.Add(new(1.0 / (t1 - t0), Format(t1 - t0, count, "ascii sequential :")));

            //t0 = Stopwatch.GetTimestamp();
            //for (var i = 0; i < count; ++i)
            //    CastingInOne(writer, kinds[i], longs[i], strings[i]);
            //t1 = Stopwatch.GetTimestamp();
            //results.Add(new(1.0 / (t1 - t0), Format(t1 - t0, count, "casting in one :")));

            //t0 = Stopwatch.GetTimestamp();
            //for (var i = 0; i < count; ++i)
            //    AsciiGetBytesInOne(writer, kinds[i], longs[i], strings[i]);
            //t1 = Stopwatch.GetTimestamp();
            //results.Add(new(1.0 / (t1 - t0), Format(t1 - t0, count, "ascii in one :")));

            //t0 = Stopwatch.GetTimestamp();
            //for (var i = 0; i < count; ++i)
            //    CastingInOnePointers(writer, kinds[i], longs[i], strings[i]);
            //t1 = Stopwatch.GetTimestamp();
            //results.Add(new(1.0 / (t1 - t0), Format(t1 - t0, count, "casting in one pointers :")));

            //t0 = Stopwatch.GetTimestamp();
            //for (var i = 0; i < count; ++i)
            //    AsciiGetBytesInOnePointers(writer, kinds[i], longs[i], strings[i]);
            //t1 = Stopwatch.GetTimestamp();
            //results.Add(new(1.0 / (t1 - t0), Format(t1 - t0, count, "ascii in one pointers :")));

            t0 = Stopwatch.GetTimestamp();
            for (var i = 0; i < count; ++i)
                SimplestPossible(writer, longs[i], (int)foos[i]);
            t1 = Stopwatch.GetTimestamp();
            results.Add(new(1.0 / (t1 - t0), Format(t1 - t0, count, "FooEnum :")));


            //results.Sort();
            Console.WriteLine(string.Join("\n", results));
            Console.WriteLine();
            results.Clear();
        } while (!Console.KeyAvailable);
    }
#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#endif
    unsafe static private void SimplestPossible (Stream stream, long int64, int int32) {
        Span<byte> bytes = stackalloc byte[sizeof(long) + sizeof(byte)];
        fixed (byte* p = bytes) {
            *(long*)p = int64;
            p[+sizeof(long)] = (byte)int32;
        }
        stream.Write(bytes);
    }

    private static void Bench (StreamWriter writer, long count = 1000000l) {
        const string format = "{0} {1}\n";
        var longs = new long[count];
        var strings = new string[count];
        var r = new Random();
        do {
            for (var i = 0; i < count; ++i) {
                longs[i] = r.NextInt64(1_000_000, 1_000_000_000);
                var randomString = RandomString(r);
                strings[i] = randomString;
            }
            var t0 = Stopwatch.GetTimestamp();
            for (var i = 0; i < count; ++i)
                writer.Write(string.Format(format, longs[i], strings[i]));
            var t1 = Stopwatch.GetTimestamp();
            Rep(t1 - t0, count, "string.Format: ");

            t0 = Stopwatch.GetTimestamp();
            for (var i = 0; i < count; ++i)
                writer.Write($"{longs[i]} {strings[i]}\n");
            t1 = Stopwatch.GetTimestamp();
            Rep(t1 - t0, count, "interp. str.: ");
            Console.WriteLine();
        } while (!Console.KeyAvailable);
    }

    private static void Bench (Stream stream, long count = 1000000l) {
        var longs = new long[count];
        var r = new Random();
        var arrays = new byte[count][];
        var strings = new string[count];
        do {
            for (var i = 0; i < count; ++i) {
                longs[i] = r.NextInt64(1_000_000, 1_000_000_000);
                var randomString = RandomString(r);
                strings[i] = randomString;
                arrays[i] = Encoding.ASCII.GetBytes(randomString);
            }
            var t0 = Stopwatch.GetTimestamp();
            for (var i = 0; i < count; ++i)
                Foo(stream, longs[i], arrays[i]);
            var t1 = Stopwatch.GetTimestamp();
            Rep(t1 - t0, count, "byte arrays, normal: ");

            t0 = Stopwatch.GetTimestamp();
            for (var i = 0; i < count; ++i)
                FooInlined(stream, longs[i], arrays[i]);
            t1 = Stopwatch.GetTimestamp();
            Rep(t1 - t0, count, "byte arrays, inlined: ");

            t0 = Stopwatch.GetTimestamp();
            for (var i = 0; i < count; ++i)
                Foo(stream, longs[i], strings[i]);
            t1 = Stopwatch.GetTimestamp();
            Rep(t1 - t0, count, "strings, normal: ");

            t0 = Stopwatch.GetTimestamp();
            for (var i = 0; i < count; ++i)
                FooInlined(stream, longs[i], strings[i]);
            t1 = Stopwatch.GetTimestamp();
            Rep(t1 - t0, count, "strings, inlined: ");
            Console.WriteLine();

        } while (!Console.KeyAvailable);
    }

    private static void Foo (Stream stream, long int64, ReadOnlySpan<byte> str) {
        Span<byte> bytes = stackalloc byte[20];
        var offset = ToChars(int64, bytes);
        stream.Write(bytes.Slice(offset));
        stream.WriteByte((byte)' ');
        stream.Write(str);
        stream.WriteByte((byte)'\n');
    }

    private static void FooInlined (Stream stream, long int64, ReadOnlySpan<byte> str) {
        Span<byte> bytes = stackalloc byte[20];
        var offset = ToCharsInlined(int64, bytes);
        stream.Write(bytes.Slice(offset));
        stream.WriteByte((byte)' ');
        stream.Write(str);
        stream.WriteByte((byte)'\n');
    }

    private static void Foo (Stream stream, long int64, string str) {
        var l = str.Length;
        Span<byte> bytes = stackalloc byte[l + 21];
        bytes[20] = (byte)' ';
        bytes[l + 20] = (byte)'\n';
        var offset = ToChars(int64, bytes);
        _ = Encoding.ASCII.GetBytes(str, bytes.Slice(20));
        stream.Write(bytes.Slice(offset));
    }

    private static void FooInlined (Stream stream, long int64, string str) {
        var l = str.Length;
        Span<byte> bytes = stackalloc byte[l + 21];
        bytes[20] = (byte)' ';
        bytes[l + 20] = (byte)'\n';
        var offset = ToCharsInlined(int64, bytes);
        _ = Encoding.ASCII.GetBytes(str, bytes.Slice(20));
        stream.Write(bytes.Slice(offset));
    }


    unsafe private static void CastingSequential (Stream stream, Kind kind, long int64, string str) {
        stream.WriteByte((byte)kind);
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        fixed (byte* p = bytes)
            *(long*)p = int64;
        stream.Write(bytes);
        var len = str?.Length ?? 0;
        if (len != 0) {
            stream.WriteByte((byte)len);
            Span<byte> chars = stackalloc byte[len];
            for (var i = 0; i < len; ++i)
                chars[i] = (byte)str[i];
            stream.Write(chars);
        }
    }

    unsafe private static void AsciiGetBytesSequential (Stream stream, Kind kind, long int64, string str) {
        stream.WriteByte((byte)kind);
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        fixed (byte* p = bytes)
            *(long*)p = int64;
        stream.Write(bytes);
        var len = str?.Length ?? 0;
        if (len != 0) {
            stream.WriteByte((byte)len);
            Span<byte> chars = stackalloc byte[len];
            _ = Encoding.ASCII.GetBytes(str, chars);
            stream.Write(chars);
        }
    }

    unsafe private static void CastingInOne (Stream stream, Kind kind, long int64, string str) {
        const int int64_Offset = sizeof(byte);
        const int length_Offset = int64_Offset + sizeof(long);
        const int string_Offset = length_Offset + sizeof(byte);
        var len = str?.Length ?? 0;
        Span<byte> bytes = stackalloc byte[string_Offset + len];
        bytes[0] = (byte)kind;
        fixed (byte* p = &bytes[int64_Offset])
            *(long*)p = int64;
        if (len != 0) {
            bytes[length_Offset] = (byte)len;
            for (int i = 0, o = length_Offset; i < len; ++i, ++o)
                bytes[o] = (byte)str[i];
        }
        stream.Write(bytes);
    }

    unsafe private static void CastingInOnePointers (Stream stream, Kind kind, long int64, string str) {
        const int int64_Offset = sizeof(byte);
        const int length_Offset = int64_Offset + sizeof(long);
        const int string_Offset = length_Offset + sizeof(byte);
        var len = str?.Length ?? 0;
        var byteCount = string_Offset + len;
        var bytes = stackalloc byte[byteCount];
        bytes[0] = (byte)kind;
        *(long*)(bytes + int64_Offset) = int64;
        if (len != 0) {
            bytes[length_Offset] = (byte)len;
            for (int i = 0, o = length_Offset; i < len; ++i, ++o)
                bytes[o] = (byte)str[i];
        }
        stream.Write(new ReadOnlySpan<byte>(bytes, byteCount));
    }


    unsafe private static void AsciiGetBytesInOne (Stream stream, Kind kind, long int64, string str) {
        const int int64_Offset = 1;// sizeof(byte);
        const int length_Offset = 9;// int64Offset + sizeof(long);
        const int string_Offset = 10;// strlenOffset + sizeof(byte);
        var len = str?.Length ?? 0;
        Span<byte> bytes = stackalloc byte[string_Offset + len];
        bytes[0] = (byte)kind;
        fixed (byte* p = &bytes[int64_Offset])
            *(long*)p = int64;
        if (len != 0) {
            bytes[length_Offset] = (byte)len;
            _ = Encoding.ASCII.GetBytes(str, bytes.Slice(string_Offset));
        }
        stream.Write(bytes);
    }

    unsafe private static void AsciiGetBytesInOnePointers (Stream stream, Kind kind, long int64, string str) {
        const int int64_Offset = 1;// sizeof(byte);
        const int length_Offset = 9;// int64Offset + sizeof(long);
        const int string_Offset = 10;// strlenOffset + sizeof(byte);
        var len = str?.Length ?? 0;
        var byteCount = string_Offset + len;
        var bytes = stackalloc byte[byteCount];
        bytes[0] = (byte)kind;
        *(long*)(bytes + int64_Offset) = int64;

        if (len != 0) {
            bytes[length_Offset] = (byte)len;
            _ = Encoding.ASCII.GetBytes(str, new Span<byte>(bytes + string_Offset, len));
        }
        stream.Write(new ReadOnlySpan<byte>(bytes, byteCount));
    }

    private static void Rep (long ticks, long count, string info = null) => Console.WriteLine(Format(ticks, count, info));

    private static string RandomString (Random r) {
        var l = r.Next(5, 100);
        Span<byte> bytes = stackalloc byte[l];
        for (var i = 0; i < l; ++i)
            bytes[i] = (byte)r.Next(' ', '~');
        return System.Text.Encoding.ASCII.GetString(bytes);
    }


    public static string ToEng (double value, string unit = null) {
        int exp = (int)(Math.Floor(Math.Log10(value) / 3.0) * 3.0);
        double newValue = value * Math.Pow(10.0, -exp);
        if (newValue >= 1000.0) {
            newValue = newValue / 1000.0;
            exp += 3;
        }

        var symbol = exp switch {
            3 => "k",
            6 => "M",
            9 => "G",
            12 => "T",
            -3 => "m",
            -6 => "u",
            -9 => "n",
            -12 => "p",
            _ => "",
        };
        return $"{newValue:##0.000} {symbol}{unit}";
    }
    private static string Format (long ticks, long count, string info = null) => $"{info}{ticks} ticks, {(double)ticks / count} ticks/item, {ToEng((double)ticks / Stopwatch.Frequency)}s, {ToEng((double)ticks / count / Stopwatch.Frequency)}s/item ";
}