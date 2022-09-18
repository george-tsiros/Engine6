namespace Common;

using System;
using System.Numerics;
using static Common.Maths;

public static class Extensions {
    public static Vector3 Xyz (this Vector4 self) => new(self.X, self.Y, self.Z);
    public static Vector2 Xy (this Vector4 self) => new(self.X, self.Y);
    public static Vector2 Xy (this Vector3 self) => new(self.X, self.Y);
    public static (T, T, T) Dex<T> (this T[] self, Vector3i i) => (self[i.X], self[i.Y], self[i.Z]);
    public static void Deconstruct (this Vector3 self, out float x, out float y, out float z) => (x, y, z) = (self.X, self.Y, self.Z);
    public static Vector2i Round (this Vector2 self) => new((int)FloatRound(self.X), (int)FloatRound(self.Y));

    public static Ascii ToAscii (this object ob) => ob switch {
        string str => new(str),
        nint iptr => AsciiConversions.IntPtr(iptr),
        nuint uptr => AsciiConversions.UIntPtr(uptr),
        short i16 => AsciiConversions.Int16(i16),
        int i32 => AsciiConversions.Int32(i32),
        long i64 => AsciiConversions.Int64(i64),
        ushort u16 => AsciiConversions.UInt16(u16),
        uint u32 => AsciiConversions.UInt32(u32),
        ulong u64 => AsciiConversions.UInt64(u64),
        DateTime dt => AsciiConversions.DateTime(dt),
        TimeSpan ts => AsciiConversions.TimeSpan(ts),
        Guid guid => AsciiConversions.Guid(guid),
        null => throw new ArgumentNullException(nameof(ob)),
        _ => new(ob.ToString())
    };

}
