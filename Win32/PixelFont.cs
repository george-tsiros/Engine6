namespace Win32;

using System;
using System.IO;
using Common;

public class PixelFont {
    public int Height { get; private init; }
    public int Width { get; private init; }
    public ReadOnlySpan<byte> Pixels => pixels;

    private readonly byte[] pixels;

    /// <summary>fails when strings start or end with newlines</summary>
    public Vector2i SizeOf (in ReadOnlySpan<char> str) {
        if (str.Length == 0)
            return Vector2i.Zero;

        var lineCount = 1;
        var maxLineLength = 0;
        var currentLineLength = 0;
        for (var i = 0; i < str.Length; ++i)
            if ('\n' == str[i]) {
                ++lineCount;
                currentLineLength = 0;
            } else if (maxLineLength < ++currentLineLength)
                maxLineLength = currentLineLength;
        return new(maxLineLength * Width, lineCount * Height);
    }

    public ReadOnlySpan<byte> this[char c] => c < 256 ? pixels.AsSpan(c * Width * Height, Width * Height) : throw new ArgumentOutOfRangeException(nameof(c), "must be 0 <= c <= 255");

    public PixelFont (string filepath) {

        var lines = File.ReadAllLines(filepath);
        var lineCount = lines.Length;
        Height = lineCount / 16;

        if (0 == Height)
            throw new ArgumentException("file is empty or has too few lines", nameof(filepath));

        if (0 != (lineCount & 0xf))
            throw new ArgumentException($"file has {lineCount} lines which is not divisible by 8", nameof(filepath));

        var lineLength = lines[0].Length;

        for (var i = 0; i < lines.Length; ++i)
            if (lines[i].Length != lineLength)
                throw new ArgumentException($"line #{i + 1} is {lines[i].Length} characters long which is different from {lineLength} characters from previous lines", nameof(filepath));

        Width = lineLength / 16;

        if (0 == Width)
            throw new ArgumentException($"{lineLength} is too short a line length", nameof(filepath));
        if (0 != (lineLength & 0xf))
            throw new ArgumentException($"{lineLength} is not divisible by 16", nameof(filepath));

        pixels = new byte[256 * Width * Height];


        for (var (i, ascii) = (0, 0); ascii < 256; ++ascii) {
            var (row, column) = int.DivRem(ascii, 16);
            for (var (cy, y) = (Height * row, 0); y < Height; ++y, ++cy)
                for (var (cx, x) = (Width * column, 0); x < Width; ++x, ++cx, ++i)
                    if (' ' != lines[cy][cx])
                        pixels[i] = 0xff;
        }
    }
}
