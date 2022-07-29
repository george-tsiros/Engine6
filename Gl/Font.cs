namespace Gl;

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class Font {
    public readonly int Height;
    public readonly bool IsProportional = true;
    public IReadOnlyList<int> Offset => offsets;
    public IReadOnlyList<byte> Pixels => pixels;
    public IReadOnlyList<int> Width => widths;

    readonly int[] widths, offsets;
    readonly byte[] pixels;

    public int WidthOf (string str) {
        if (str is null)
            throw new ArgumentNullException(nameof(str));
        if (str.Length == 0)
            return 0;
        var width = 0;
        foreach (var c in str) {
            if (255 < c)
                throw new ArgumentException("only 8bit strings are supported", nameof(str));
            width += widths[c];
        }
        return width;
    }

    public Font (string filepath) {
        using var r = new StreamReader(filepath);
        var info = r.ReadLine().Split(',').Select(int.Parse).ToArray();
        widths = new int[256];
        offsets = new int[256];
        Height = info[0];
        var on = (char)info[1];
        using (var mem = new MemoryStream()) {
            var width = 0;
            for (int ascii = 0, row = 1; ascii < 256; ++row) {
                var line = r.ReadLine();
                var lineLength = line.Length;

                if (row == 1) {
                    widths[ascii] = width = lineLength;
                    offsets[ascii] = (int)mem.Length;
                } else if (lineLength != width)
                    throw new Exception($"row {row} for ascii {ascii} is {lineLength}, expected {width}");

                foreach (var c in line)
                    mem.WriteByte(c == on ? byte.MaxValue : byte.MinValue);

                if (row == Height) {
                    ++ascii;
                    row = 0;
                }
            }
            pixels = mem.ToArray();
        }
        var w = widths[0];
        foreach (var ww in widths) {
            if (ww != w) {
                IsProportional = false;
                break;
            }
        }
    }
}
