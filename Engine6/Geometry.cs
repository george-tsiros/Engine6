namespace Engine;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Gl;
using Win32;

static class Cube {
    /*
       4-----5
      /.    /|
     / .   / |
    7-----6  |
    |  0..|..|1
    | .   | /
    |.    |/
    3-----2
    -------------------------------------------
    |0        |1 top    |2        |3        |4
    --------------------------------------------
    |5 left   |6 near   |7 right  |8  far   |9
    --------------------------------------------
    |10       |11 bottom|12       |13       |14
    --------------------------------------------
    |15       |16       |17       |18       |19
    --------------------------------------------

    */
    internal static Vector4[] Vertices => new Vector4[] { new(0, 0, 0, 1), new(1, 0, 0, 1), new(1, 0, 1, 1), new(0, 0, 1, 1), new(0, 1, 0, 1), new(1, 1, 0, 1), new(1, 1, 1, 1), new(0, 1, 1, 1), };
    internal static Vector2[] UvVectors => new Vector2[] {
            new(0.00f, 0.00f),
            new(0.25f, 0.00f),
            new(0.50f, 0.00f),
            new(0.75f, 0.00f),
            new(1.00f, 0.00f),

            new(0.00f, 0.25f),
            new(0.25f, 0.25f),
            new(0.50f, 0.25f),
            new(0.75f, 0.25f),
            new(1.00f, 0.25f),

            new(0.00f, 0.50f),
            new(0.25f, 0.50f),
            new(0.50f, 0.50f),
            new(0.75f, 0.50f),
            new(1.00f, 0.50f),

            new(0.00f, 0.75f),
            new(0.25f, 0.75f),
            new(0.50f, 0.75f),
            new(0.75f, 0.75f),
            new(1.00f, 0.75f),

            new(0.00f, 1.00f),
            new(0.25f, 1.00f),
            new(0.50f, 1.00f),
            new(0.75f, 1.00f),
            new(1.00f, 1.00f),

        };
    internal static int[] Indices { get; } = new int[] {
            1, 5, 6, 6, 2, 1, // right
            0, 3, 7, 7, 4, 0, // left
            4, 7, 6, 6, 5, 4, // top
            0, 1, 2, 2, 3, 0, // bottom
            2, 6, 7, 7, 3, 2, // near
            0, 4, 5, 5, 1, 0, // far
        };
    internal static int[] UvIndices => new int[] {
            13, 8, 7, 7 , 12, 13,
            10, 11, 6, 6, 5, 10,
            6, 7, 2, 2, 1, 6,
            16, 17, 12, 12, 11, 16,
            12, 7, 6, 6, 11, 12,
            14, 9, 8, 8, 13, 14,
        };
    internal static Vector4[] Normals => new Vector4[] { Vector4.UnitX, -Vector4.UnitX, Vector4.UnitY, -Vector4.UnitY, Vector4.UnitZ, -Vector4.UnitZ, };
    internal static int[] NormalIndices => new int[] { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5, };

}

static class Geometry {
    public static int Hit (in Ray ray, in (Vector3 A, Vector3 B, Vector3 C)[] faces) {
        var currentBest = -1;
        var currentNearest = float.MaxValue;
        for (var i = 0; i < faces.Length; i++)
            if (IsHit(faces[i], ray, out var x) && x.Z < currentNearest)
                (currentBest, currentNearest) = (i, x.Z);
        return currentBest;
    }
    public static int Hit (in Ray ray, in (Vector3 A, Vector3 B, Vector3 C)[] faces, out Vector3 hit) {
        var faceIndex = -1;
        hit = new(0, 0, float.MaxValue);
        for (var i = 0; i < faces.Length; i++)
            if (IsHit(faces[i], ray, out var x) && x.Z < hit.Z) {
                hit = x;
                faceIndex = i;
            }
        return faceIndex;
    }

    public static bool IsHit (in (Vector3 a, Vector3 b) face, in Vector3 dir, out Vector3 x) {
        var (a, b) = face;
        return new Matrix3x3(a, b, -dir)
           .TrySolve(Vector3.Zero, out x) &&
           0 <= x.X && x.X <= 1 &&
           0 <= x.Y && x.Y <= 1 &&
           0 < x.Z && x.X + x.Y <= 1f;
    }

    public static bool IsHit (in (Vector3 a, Vector3 b, Vector3 c) face, in Ray r, out Vector3 x) {
        var (a, b, c) = face;
        return new Matrix3x3(a - c, b - c, -r.Direction)
           .TrySolve(r.Origin - c, out x) &&
           0 <= x.X && x.X <= 1 &&
           0 <= x.Y && x.Y <= 1 &&
           0 < x.Z && x.X + x.Y <= 1f;
    }
    public static float ParallelDistance (Ray ray, in (Vector3 A, Vector3 B, Vector3 C)[] faces) {
        var nearest = float.MaxValue;
        _ = Parallel.ForEach(faces, () => nearest, (f, state, n) => IsHit(f, ray, out var x) && x.Z < n ? x.Z : n, n => nearest = n);
        return nearest;
    }

    public static float Distance (in Ray ray, in IEnumerable<(Vector3 A, Vector3 B, Vector3 C)> faces) {
        var nearest = float.MaxValue;
        foreach (var f in faces)
            if (IsHit(f, ray, out var x) && x.Z < nearest)
                nearest = x.Z;
        return nearest;
    }

    public static float Distance (in Vector3 dir, in (Vector3 A, Vector3 B)[] faces) {
        var nearest = float.MaxValue;
        for (var i = 0; i < faces.Length; i++)
            if (IsHit(faces[i], dir, out var x) && x.Z < nearest)
                nearest = x.Z;
        return nearest;
    }

    public static void Transform (IList<Vector3> array, Matrix4x4 m) {
        var count = array.Count;
        for (int i = 0; i < count; i++)
            array[i] = Vector3.Transform(array[i], m);
    }

    public static void Transform (Vector4[] array, Matrix4x4 m) {
        for (int i = 0; i < array.Length; i++)
            array[i] = Vector4.Transform(array[i], m);
    }

    internal static T[] Dex<T> (T[] array, int[] indices) where T : struct {
        T[] b = new T[indices.Length];
        Dex(array, indices, b);
        return b;
    }
    internal static void Dex<T> (T[] vertices, int[] indices, T[] dex) where T : struct {
        if (dex.Length != indices.Length)
            throw new Exception();
        for (var i = 0; i < indices.Length; ++i)
            dex[i] = vertices[indices[i]];
    }
    internal static Vector4[] ScaleInPlace (Vector4[] v, Vector4 s) {
        for (var i = 0; i < v.Length; ++i)
            v[i] *= s;
        return v;
    }

    internal static void ScaleInPlace (List<Vector4> v, float f) {
        var c = v.Count;
        for (var i = 0; i < c; i++)
            v[i] *= f;
    }

    internal static void ScaleInPlace (List<Vector3> v, float f) {
        var c = v.Count;
        for (var i = 0; i < c; i++)
            v[i] *= f;
    }

    internal static Vector4[] Scale (Vector4[] v, Vector3 f) {
        var scaled = new Vector4[v.Length];
        Array.Copy(v, scaled, v.Length);
        return ScaleInPlace(scaled, f);
    }
    internal static Vector4[] ScaleInPlace (Vector4[] v, float f) => ScaleInPlace(v, new Vector4(f, f, f, 1));
    internal static Vector4[] ScaleInPlace (Vector4[] v, Vector3 f) => ScaleInPlace(v, new Vector4(f, 1));

    internal static Vector4[] Translate (Vector4[] v, Vector3 d) {
        var translated = new Vector4[v.Length];
        var t = new Vector4(d, 0);
        for (var i = 0; i < v.Length; ++i)
            translated[i] = v[i] + t;
        return translated;
    }
    internal static Vector4[] TranslateInPlace (Vector4[] v, Vector3 d) {
        var t = new Vector4(d, 0);
        for (var i = 0; i < v.Length; ++i)
            v[i] += t;
        return v;
    }
    internal static int[] FlipWinding (int[] indices) {
        var flipped = new int[indices.Length];
        var triangleCount = indices.Length / 3;
        if (indices.Length % 3 != 0)
            throw new Exception();
        for (var i = 0; i < indices.Length; i += 3) {
            flipped[i] = indices[i];
            flipped[i + 1] = indices[i + 2];
            flipped[i + 2] = indices[i + 1];
        }
        return flipped;
    }

    internal static void FlipWindingInPlace (int[] indices) {
        var triangleCount = indices.Length / 3;
        if (indices.Length % 3 != 0)
            throw new Exception();
        for (var i = 0; i < indices.Length; i += 3)
            (indices[i + 2], indices[i + 1]) = (indices[i + 1], indices[i + 2]);
    }

    internal static Vector4[] CreateNormals (Vector4[] vertices, int[] indices) {
        var indexCount = indices.Length;
        var faceCount = indexCount / 3;
        if (indexCount % 3 != 0)
            throw new Exception();
        var faces = new Vector3i[faceCount];
        for (var i = 0; i < faceCount; ++i)
            faces[i] = new Vector3i(indices[3 * i], indices[3 * i + 1], indices[3 * i + 2]);
        var faceNormals = new Vector3[faces.Length];
        for (var i = 0; i < faces.Length; ++i)
            faceNormals[i] = FaceNormal(vertices, faces[i]);
        var vertexCount = vertices.Length;
        var stack = new Stack<int>();
        var vertexNormals = new Vector4[vertexCount];
        for (var vertexIndex = 0; vertexIndex < vertexCount; ++vertexIndex) {
            for (var i = 0; i < indexCount; ++i)
                if (indices[i] == vertexIndex)
                    stack.Push(i / 3);
            var normal = Vector3.Zero;
            while (stack.Count > 0)
                normal += faceNormals[stack.Pop()];
            vertexNormals[vertexIndex] = new(Vector3.Normalize(normal), 0);
        }
        return vertexNormals;
    }
    private static Vector3 FaceNormal (Vector4[] vertices, Vector3i face) {
        var a = vertices[face.X].Xyz();
        var b = vertices[face.Y].Xyz();
        var c = vertices[face.Z].Xyz();
        return Vector3.Normalize(Vector3.Cross(b - a, c - b));
    }

    internal static Vector4[] CreateNormals (Vector4[] vertices) {
        if (vertices.Length % 3 != 0)
            throw new Exception();
        var normals = new Vector4[vertices.Length];
        for (var i = 0; i < vertices.Length; i += 3) {
            var a = vertices[i + 0].Xyz();
            var b = vertices[i + 1].Xyz();
            var c = vertices[i + 2].Xyz();
            var n = Vector4.Normalize(new(Vector3.Cross(b - a, c - b), 0));
            normals[i + 0] = n;
            normals[i + 1] = n;
            normals[i + 2] = n;
        }
        return normals;
    }
}

static class Quad {
    internal static Vector4[] Vertices => new Vector4[] {
            new(-1, -1, 0, 1),
            new(+1, -1, 0, 1),
            new(+1, +1, 0, 1),
            new(+1, +1, 0, 1),
            new(-1, +1, 0, 1),
            new(-1, -1, 0, 1),
        };

    internal static Vector2[] Uv => new Vector2[] {
            new(0, 0),
            new(1, 0),
            new(1, 1),
            new(1, 1),
            new(0, 1),
            new(0, 0),
        };
}
