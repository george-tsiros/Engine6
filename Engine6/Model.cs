namespace Engine;

using System;
using System.Numerics;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Gl;
using System.Text.RegularExpressions;
using Win32;

public class Model {
    public List<Vector3i> Faces { get; } = new();
    public List<Vector3> Vertices { get; } = new();
    public Vector3 Min { get; private set; }
    public Vector3 Max { get; private set; }
    private static readonly char[] space = { ' ' };
    private static readonly IFormatProvider AllowDot = CultureInfo.InvariantCulture;
    public Model (StreamReader reader) {
        Read(reader);
    }
    public Model (string filepath) {
        using var reader = new StreamReader(filepath);
        Read(reader);
    }

    static readonly Regex FaceRegex = new(@"^(\d+)(/\d+)*$");

    static int FromFace (string part) => int.Parse(FaceRegex.Match(part).Groups[1].Value);

    private void Read (StreamReader reader) {
        var (minx, miny, minz) = (float.MaxValue, float.MaxValue, float.MaxValue);
        var (maxx, maxy, maxz) = (float.MinValue, float.MinValue, float.MinValue);
        foreach (var line in Extra.EnumLines(reader, true)) {
            if (line[0] == '#')
                continue;

            var parts = line.Split(space, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0 || parts[0].Length != 1)
                continue;


            switch (parts[0]) {
                case "f":
                    if (parts.Length != 4)
                        throw new ArgumentException($"line '{line}' invalid");
                    Faces.Add(new(FromFace(parts[1]) - 1, FromFace(parts[2]) - 1, FromFace(parts[3]) - 1));
                    break;
                case "v":
                    if (parts.Length != 4)
                        throw new ArgumentException($"line '{line}' invalid");
                    var (x, y, z) = (float.Parse(parts[1], AllowDot), float.Parse(parts[2], AllowDot), float.Parse(parts[3], AllowDot));
                    (minx, miny, minz) = (Math.Min(x, minx), Math.Min(y, miny), Math.Min(z, minz));
                    (maxx, maxy, maxz) = (Math.Max(x, maxx), Math.Max(y, maxy), Math.Max(z, maxz));
                    Vertices.Add(new Vector3(x, y, z));
                    break;
            }
        }
        Min = new(minx, miny, minz);
        Max = new(maxx, maxy, maxz);

    }
}
