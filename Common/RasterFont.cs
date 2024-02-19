namespace Common;

using System;
using System.Globalization;
using System.IO;

public class RasterFont {
    public int Height { get; private init; }
    public int Width { get; private init; }
    private readonly byte[] Pixels;
    private static byte ReverseBits (byte b) => 
        (byte)(((b * 0x80200802ul) & 0x0884422110ul) * 0x0101010101ul >> 32);
    public ReadOnlySpan<byte> this[char c] =>
        ' ' <= c && c <= '~' ? Pixels.AsSpan((c - ' ') * Height, Height) : throw new ArgumentOutOfRangeException(nameof(c), "must be 0 <= c <= 255");

    public static readonly RasterFont Default = FromBdf("data/spleen-8x16.bdf");

    public static RasterFont FromBdf (string filepath) {
        var chars = File.ReadAllText(filepath).AsSpan();
        var pixels = new byte[chars.Length / 2];
        for (var i = 0; i < pixels.Length; ++i)
            pixels[i] = ReverseBits(byte.Parse(chars.Slice(2 * i, 2), NumberStyles.AllowHexSpecifier));
        return new(pixels) { Height = 16, Width = 8 };
    }

    private RasterFont (byte[] pixels) {
        Pixels = pixels;
    }
}
