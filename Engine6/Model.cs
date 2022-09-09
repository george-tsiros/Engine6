namespace Engine6;

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Win32;
using static Common.Maths;
using Common;
using System.Numerics;

public class Model {
    public List<Vector3i> Faces { get; private set; }
    public List<Vector3> Vertices { get; private set; }
    public Vector3 Min { get; private set; }
    public Vector3 Max { get; private set; }
    protected static readonly char[] space = { ' ' };
    protected static readonly IFormatProvider AllowDot = CultureInfo.InvariantCulture;

    public static void InvertFaces (Model m) {
        var count = m.Faces.Count;
        for (var i = 0; i < count; ++i) {
            var (a, b, c) = m.Faces[i];
            m.Faces[i] = new(a, c, b);
        }
    }

    Model () { }

    public Model (StreamReader reader, bool center = false) {
        Read(reader, center);
    }

    public Model (string filepath, bool center = false) {
        using var reader = new StreamReader(filepath);
        Read(reader, center);
    }

    static readonly Regex FaceRegex = new(@"^(\d+)(/(\d+)?)*$");

    static int FromFace (string part) => int.Parse(FaceRegex.Match(part).Groups[1].Value);
    static readonly Vector3 Vector3MaxValue = new(float.MaxValue, float.MaxValue, float.MaxValue);
    static readonly Vector3 Vector3MinValue = new(float.MinValue, float.MinValue, float.MinValue);
    void Read (StreamReader reader, bool center) {
        Faces = new();
        Vertices = new();
        foreach (var line in Functions.EnumLines(reader, true)) {
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
                    Vertices.Add(new(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3])));
                    break;
            }
        }
        (Min, Max) = (Vector3MaxValue, Vector3MinValue);
        foreach (var v in Vertices)
            (Min, Max) = (Vector3.Min(Min, v), Vector3.Max(Max, v));

        if (center) {
            // Center of bounding volume
            var c = 0.5f * (Max + Min);
            for (var i = 0; i < Vertices.Count; ++i)
                Vertices[i] -= c;
        }
    }

    public IEnumerable<Vector3d> CreateNormals () {
        foreach (var (a, b, c) in Faces) {
            var (v0, v1, v2) = (Vertices[a], Vertices[b], Vertices[c]);
            yield return Vector3d.Normalize(Vector3d.Cross(v1 - v0, v2 - v0));
        }
    }

    public static Model Quad (float w, float h) => new() {
        Vertices = new() { new(-w / 2, -h / 2, 0), new(w / 2, -h / 2, 0), new(w / 2, h / 2, 0), new(-w / 2, h / 2, 0) },
        Faces = new() { new(0, 1, 2), new(0, 2, 3), },
    };

    public static Model Sphere (int nTheta, int nPhi, float radius) {
        int vertexCount = 2 + nTheta * (nPhi - 1);
        int triangleCount = 2 * (nPhi - 1) * nTheta;
        var dTheta = 2 * fPi / nTheta;
        var fPhi = fPi / nPhi;
        var phi = fPhi;
        var model = new Model() { Vertices = new(), Faces = new() };
        model.Vertices.Add(radius * Vector3.UnitY);

        for (int vi = 1; vi < nPhi; ++vi, phi += fPhi) {
            var (sin, cos) = FloatSinCos(phi);
            var theta = 0f;
            for (var hi = 0; hi < nTheta; ++hi, theta += dTheta)
                model.Vertices.Add(new(radius * sin * FloatCos(theta), radius * cos, radius * sin * FloatSin(theta)));
        }

        model.Vertices.Add(-radius * Vector3.UnitY);

        var faceIndex = 0;
        for (var i = 1; i <= nTheta; ++i, ++faceIndex)
            model.Faces.Add(new(0, i % nTheta + 1, i));

        for (var y = 1; y < nPhi - 1; ++y)
            for (var x = 1; x <= nTheta; ++x) {
                // a--d
                // |\ |
                // | \|
                // b--c
                int
                    a = (y - 1) * nTheta + x,
                    b = y * nTheta + x,
                    c = y * nTheta + x % nTheta + 1,
                    d = (y - 1) * nTheta + x % nTheta + 1;
                model.Faces.Add(new(a, c, b));
                model.Faces.Add(new(a, d, c));
            }

        for (var i = 1; i <= nTheta; ++i, ++faceIndex) {
            // y = 0 => 1 vertex
            // y = 1 ntheta vertices, starting from '1' = (y-1) * nTheta + 1
            // y = nphi-2 , second to last, starting from (nphi-3) * ntheta +1
            // y = nphi - 1, last row, 1 vertex (the last one)
            int
                a = (nPhi - 2) * nTheta + i,
                b = (nPhi - 2) * nTheta + i % nTheta + 1;

            Debug.Assert(a < model.Vertices.Count);
            Debug.Assert(b < model.Vertices.Count);
            model.Faces.Add(new(a, b, model.Vertices.Count - 1));
        }
        Debug.Assert(triangleCount == model.Faces.Count);
        return model;
    }

    public static Model Cube (float side) =>
        Cube(side, side, side);

    public static Model Cube (float w, float h, float d) => new() {
        Vertices = new() {
            new(-w / 2, -h / 2, -d / 2), // near, left, down
            new(+w / 2, -h / 2, -d / 2), // near, right, down
            new(+w / 2, +h / 2, -d / 2), // near, right, up
            new(-w / 2, +h / 2, -d / 2), // near, left, up
            new(-w / 2, -h / 2, +d / 2), // far, left, down
            new(+w / 2, -h / 2, +d / 2), // far, right, down
            new(+w / 2, +h / 2, +d / 2), // far, right, up
            new(-w / 2, +h / 2, +d / 2), // far, left, up
        },
        /*
              3........2
            / .      / .
          /   .    /   .
        7--------6     .
        |     .  |     .
        |     0..|.....1
        |   /    |   /
        | /      | /
        4--------5


*/

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

    public static Model Plane (Vector2 size, Vector2i divisions) {
        if (size.X < float.Epsilon || size.Y < float.Epsilon)
            throw new ArgumentException($"size must be at least {float.Epsilon} in both dimensions", nameof(size));
        if (divisions.X < 1 || divisions.Y < 1)
            throw new ArgumentException("subdivisions must be at least 1 in both dimensions", nameof(divisions));
        // 1 subd -> 2 vert
        // 2 subd -> 3 vert
        // total vert = (subd.X+1)*(subd.Y+1)
        var vertexCount = (divisions.X + 1) * (divisions.Y + 1);
        var vertices = new Vector3[vertexCount];
        var dx = size.X / divisions.X;
        var dy = size.Y / divisions.Y;


        /*
 0,0 0,1 0,2 0,3
  Xa  Xb  X   X

 1,0 1,1 1,2 1,3
  Xd  Xc  X   X
        +------->
        |      x
        |
  X   X | X   X
        |
        vz
*/

        for (var (row, z) = (0, -size.Y / 2); row <= divisions.Y; ++row, z += dy)
            for (var (column, x) = (0, -size.X / 2); column <= divisions.X; ++column, x += dx)
                vertices[row * (divisions.X + 1) + column] = new(x, 0, z);

        var faces = new Vector3i[divisions.X * divisions.Y * 2];
        for (var (row, i) = (0, 0); row < divisions.Y; ++row)
            for (var column = 0; column < divisions.X; ++column, ++i) {
                var a = row * (divisions.X + 1) + column;
                var b = a + 1;
                var c = b + divisions.X + 1;
                var d = c - 1;
                faces[i] = new(b, a, d);
                faces[++i] = new(b, d, c);
            }
        return new() { Faces = new(faces), Vertices = new(vertices), Min = new(-size.X / 2, 0, -size.Y / 2), Max = new(size.X / 2, 0, size.Y / 2), };
    }
}
