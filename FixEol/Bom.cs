namespace FixEol;

using System;
using System.Text;

sealed class Bom {
    private readonly byte[] Bytes;
    public int Length => Bytes.Length;
    public readonly Encoding Encoding;
    public bool Matches (byte[] bytes) {
        if (bytes.Length < Bytes.Length)
            throw new ArgumentException("too few bytes", nameof(bytes));
        for (var i = 0; i < Bytes.Length; ++i)
            if (bytes[i] != Bytes[i])
                return false;
        return true;
    }

    private Bom (byte[] bytes, Encoding encoding) =>
        (Bytes, Encoding) = (bytes, encoding);

    public static Bom FindBom (byte[] bytes) {
        foreach (var bom in new Bom[] { Utf32_BE, Utf32_LE, UtfEbcdic, Gb18030, Utf7, Utf1, Scsu, Bocu1, Utf8, Utf16_BE, Utf16_LE, })
            if (bom.Length <= bytes.Length && bom.Matches(bytes))
                return bom;
        return None;
    }

    public static readonly Bom None = new(Array.Empty<byte>(), Encoding.ASCII);
    public static readonly Bom Utf32_BE = new(new byte[] { 0x00, 0x00, 0xFE, 0xFF, }, null);
    public static readonly Bom Utf32_LE = new(new byte[] { 0xFF, 0xFE, 0x00, 0x00, }, Encoding.UTF32);
    public static readonly Bom UtfEbcdic = new(new byte[] { 0xDD, 0x73, 0x66, 0x73, }, null);
    public static readonly Bom Gb18030 = new(new byte[] { 0x84, 0x31, 0x95, 0x33, }, null);
    public static readonly Bom Utf7 = new(new byte[] { 0x2B, 0x2F, 0x76, }, Encoding.UTF7);
    public static readonly Bom Utf1 = new(new byte[] { 0xF7, 0x64, 0x4C, }, null);
    public static readonly Bom Scsu = new(new byte[] { 0x0E, 0xFE, 0xFF, }, null);
    public static readonly Bom Bocu1 = new(new byte[] { 0xFB, 0xEE, 0x28, }, null);
    public static readonly Bom Utf8 = new(new byte[] { 0xEF, 0xBB, 0xBF }, Encoding.UTF8);
    public static readonly Bom Utf16_BE = new(new byte[] { 0xFE, 0xFF, }, Encoding.BigEndianUnicode);
    public static readonly Bom Utf16_LE = new(new byte[] { 0xFF, 0xFE, }, Encoding.Unicode);
}
