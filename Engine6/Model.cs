namespace Engine;

using System;
//using System.Numerics;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Gl;
using System.Text.RegularExpressions;
using Win32;

public class Model {
    public List<Vector3i> Faces { get; init; } = new();
    public List<Vector3d> Vertices { get; init; } = new();
    public Vector3d Min { get; private set; }
    public Vector3d Max { get; private set; }
    private static readonly char[] space = { ' ' };
    private static readonly IFormatProvider AllowDot = CultureInfo.InvariantCulture;
    private Model () { }
    public Model (StreamReader reader, bool center = false) {
        Read(reader, center);
    }
    public Model (string filepath, bool center = false) {
        using var reader = new StreamReader(filepath);
        Read(reader, center);
    }

    static readonly Regex FaceRegex = new(@"^(\d+)(/(\d+)?)*$");

    static int FromFace (string part) => int.Parse(FaceRegex.Match(part).Groups[1].Value);

    private void Read (StreamReader reader, bool center) {
        var min = new Vector3d(double.MaxValue);
        var max = new Vector3d(double.MinValue);
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
                    var x = new Vector3d(float.Parse(parts[1], AllowDot), float.Parse(parts[2], AllowDot), float.Parse(parts[3], AllowDot));
                    min = Vector3d.Min(x, min);
                    max = Vector3d.Max(x, max);
                    Vertices.Add(x);
                    break;
            }
        }
        Min = min;
        Max = max;
        if (center) {
            var c = 0.5f * (Max + Min);
            for (var i = 0; i < Vertices.Count; ++i)
                Vertices[i] -= c;
        }
    }

    static List<Vector3d> CubeVertices (float w, float h, float d) => new() { new(-w / 2, -h / 2, -d / 2), new(+w / 2, -h / 2, -d / 2), new(+w / 2, +h / 2, -d / 2), new(-w / 2, +h / 2, -d / 2), new(-w / 2, -h / 2, +d / 2), new(+w / 2, -h / 2, +d / 2), new(+w / 2, +h / 2, +d / 2), new(-w / 2, +h / 2, +d / 2), };

    public static Model Quad (float w, float h) => new() {
        Vertices = new() { new(-w / 2, -h / 2, 0), new(w / 2, -h / 2, 0), new(w / 2, h / 2, 0), new(-w / 2, h / 2, 0) },
        Faces = new() {
            new(0, 1, 2),
            new(0, 2, 3),
        },
    };

    public static Model Cube (float w, float h, float d) => new() {
        Vertices = CubeVertices(w, h, d),
        Faces = new List<Vector3i> {
            new(4, 5, 6),
            new(4, 6, 7),
            new(1, 0, 3),
            new(1, 3, 2),
            new(0, 1, 5),
            new(0, 5, 4),
            new(7, 6, 2),
            new(7, 2, 3),
            new(5, 1, 2),
            new(5, 2, 6),
            new(0, 4, 7),
            new(0, 7, 3),
        }
    };

}
