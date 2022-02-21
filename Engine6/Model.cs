namespace Engine;

using System;
using System.Numerics;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;

public class Model {
    public List<(int i, int j, int k)> Faces { get; } = new();
    public List<Vector3> Vertices { get; } = new();
    public Vector3 Min { get; }
    public Vector3 Max { get; }
    static readonly IFormatProvider AllowDot = CultureInfo.InvariantCulture;
    public Model (string filepath) {
        var (minx, miny, minz) = (float.MaxValue, float.MaxValue, float.MaxValue);
        var (maxx, maxy, maxz) = (float.MinValue, float.MinValue, float.MinValue);
        foreach (var line in Extra.EnumLines(filepath)) {
            var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                continue;
            Debug.Assert(parts.Length == 4);
            switch (parts[0]) {
                case "f":
                    Faces.Add((int.Parse(parts[1]) - 1, int.Parse(parts[2]) - 1, int.Parse(parts[3]) - 1));
                    break;
                case "v":
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
