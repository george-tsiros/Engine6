namespace Gl;

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

public class Font {
    public int Height { get; private set; }
    public bool IsProportional { get; private set; } = true;
    public IReadOnlyList<int> Offset => offsets;
    public IReadOnlyList<byte> Pixels => pixels;
    public IReadOnlyList<int> Width => widths;
    public string FamilyName { get; private set; }
    public float EmSize { get; private set; }

    int[] widths, offsets;
    byte[] pixels;

    public int WidthOf (ReadOnlySpan<char> str) {
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

    public Font (string familyName, float size) {
        EmSize = size;
        var startInfo = new ProcessStartInfo("bitmaptoraster.exe", $"\"{familyName}\" {size}") {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
        };
        using (var p = Process.Start(startInfo)) {
            ReadFrom(p.StandardOutput);
            p.WaitForExit();
            if (p.ExitCode != 0)
                throw new Exception($"failed to create a size {size} {familyName} font file");
        }
    }

    public Font (string filepath) {
        using var f = new StreamReader(filepath);
        ReadFrom(f);
    }

    void ReadFrom (StreamReader r) {
        FamilyName = r.ReadLine();
        if (FamilyName is null)
            return;
        var _a = r.ReadLine();
        if (_a is null)
            return;
        var _b = _a.Split(',');
        var info = _b.Select(int.Parse).ToArray();
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
