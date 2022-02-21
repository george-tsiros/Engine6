namespace Engine;

using System;
using System.Numerics;
using System.Diagnostics;
using System.Collections.Generic;

public class Model {
    public List<(int i, int j, int k)> Faces { get; } = new();
    public List<Vector3> Vertices { get; } = new();
    public Vector3 Min { get; }
    public Vector3 Max { get; }
    public Model (string filepath) {
        var minx = float.MaxValue;
        var miny = float.MaxValue;
        var minz = float.MaxValue;
        var maxx = float.MinValue;
        var maxy = float.MinValue;
        var maxz = float.MinValue;
        foreach (var line in Extra.EnumLines(filepath)) {
            var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                continue;
            switch (parts[0]) {
                case "f":
                    Debug.Assert(parts.Length == 4);
                    Faces.Add((int.Parse(parts[1]) - 1, int.Parse(parts[2]) - 1, int.Parse(parts[3]) - 1));
                    break;
                case "v":
                    Debug.Assert(parts.Length == 4);
                    var x = float.Parse(parts[1]);
                    var y = float.Parse(parts[2]);
                    var z = float.Parse(parts[3]);
                    minx = Math.Min(x, minx);
                    miny = Math.Min(y, miny);
                    minz = Math.Min(z, minz);
                    maxx = Math.Max(x, maxx);
                    maxy = Math.Max(y, maxy);
                    maxz = Math.Max(z, maxz);
                    Vertices.Add(new Vector3(x, y, z));
                    break;
            }
        }
        Min = new(minx, miny, minz);
        Max = new(maxx, maxy, maxz);
    }
}
