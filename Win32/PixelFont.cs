namespace Win32;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Common.Maths;

public class PixelFont {
    public int Height { get; private init; }
    public int Width { get; private init; }
    public IReadOnlyList<byte> Pixels => pixels;

    private byte[] pixels;

    public int WidthOf (ReadOnlySpan<char> str) {
        return str.Length * Width;
    }

    public PixelFont (string filepath) {

        var lines = File.ReadAllLines(filepath);
        
        var lineCount = lines.Length;
        Height = lineCount >> 4;
        if (0 == Height)
            throw new ArgumentException($"file is empty or has too few lines", nameof(filepath));
        if (0 != (lineCount & 0xf))
            throw new ArgumentException($"file has {lineCount} lines which is not divisible by 16", nameof(filepath));
        var lineLength = lines[0].Length;

        for (var i = 0; i < lines.Length; ++i)
            if (lines[i].Length != lineLength)
                throw new ArgumentException($"line #{i + 1} is {lines[i].Length} characters long which is different from {lineLength} characters from previous lines", nameof(filepath));
        Width = lineLength >> 16;
        if (0 == Width)
            throw new ArgumentException($"{lineLength} is too short a line length", nameof(filepath));
        if (0 != (lineLength & 0xf))
            throw new ArgumentException($"{lineLength} is not divisible by 16", nameof(filepath));



        //if (r.ReadLine() is null)
        //    return;
        //var _a = r.ReadLine();
        //if (_a is null)
        //    return;
        //var _b = _a.Split(',');
        //var info = _b.Select(int.Parse).ToArray();
        //Height = info[0];
        //var on = (char)info[1];
        //using (MemoryStream mem = new()) {
        //    var width = 0;
        //    for (int ascii = 0, row = 1; ascii < 256; ++row) {
        //        var line = r.ReadLine();
        //        var lineLength = line.Length;

        //        if (row == 1) {
        //            widths[ascii] = width = lineLength;
        //            offsets[ascii] = (int)mem.Length;
        //        } else if (lineLength != width)
        //            throw new Exception($"row {row} for ascii {ascii} is {lineLength}, expected {width}");

        //        foreach (var c in line)
        //            mem.WriteByte(c == on ? byte.MaxValue : byte.MinValue);

        //        if (row == Height) {
        //            ++ascii;
        //            row = 0;
        //        }
        //    }
        //    pixels = mem.ToArray();
        //}
    }
}
