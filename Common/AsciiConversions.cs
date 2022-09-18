namespace Common;

using System;

internal static class AsciiConversions {
    public static Ascii IntPtr (nint iptr) => throw new NotImplementedException();
    public static Ascii UIntPtr (nuint uptr) => throw new NotImplementedException();
    public static Ascii Int16 (short i16) => throw new NotImplementedException();
    public static Ascii Int32 (int i32) => throw new NotImplementedException();
    public static Ascii Int64 (long i64) => throw new NotImplementedException();
    public static Ascii UInt16 (ushort u16) => throw new NotImplementedException();
    public static Ascii UInt32 (uint u32) => throw new NotImplementedException();
    public static Ascii UInt64 (ulong u64) => throw new NotImplementedException();
    public static Ascii DateTime (DateTime dt) => throw new NotImplementedException();
    public static Ascii TimeSpan (TimeSpan ts) => throw new NotImplementedException();
    public static Ascii Guid (Guid guid) => throw new NotImplementedException();

    private static Ascii Int32ToAscii (int i) {
        Span<byte> bytes = stackalloc byte[11];
        throw new NotImplementedException();
    }
}
