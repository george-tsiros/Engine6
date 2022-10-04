namespace Common;

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using static Common.Maths;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Vector4d {

    public readonly double X, Y, Z, W;

    public static readonly Vector4d
        Zero = new(),
        One = new(1, 1, 1, 1),
        UnitX = new(1, 0, 0, 0),
        UnitY = new(0, 1, 0, 0),
        UnitZ = new(0, 0, 1, 0),
        UnitW = new(0, 0, 0, 1);

    public Vector4d (double x, double y, double z, double w) { X = x; Y = y; Z = z; W = w; }
    public Vector4d (in Vector3d v3, double w) { X = v3.X; Y = v3.Y; Z = v3.Z; W = w; }

    public static explicit operator Vector4 (in Vector4d v) => new((float)v.X, (float)v.Y, (float)v.Z, (float)v.W);
    public static implicit operator Vector4d (in Vector4 v) => new(v.X, v.Y, v.Z, v.W);

    public Vector3d Xyz () => new(X, Y, Z);

    public static double Dot (in Vector4d a, in Vector4d b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
    public double Magnitude () => DoubleSqrt(X * X + Y * Y + Z * Z + W * W);
    public static Vector4d Normalize (in Vector4d v) {
        var magnitude = v.Magnitude();
        return 1e-6 < magnitude ? 1 / magnitude * v : throw new ArgumentOutOfRangeException(nameof(v));
    }

    public static Vector4d Transform (in Vector4d v, in Matrix4d m) => new(Dot(v, m.Col1), Dot(v, m.Col2), Dot(v, m.Col3), Dot(v, m.Col4));

    public static bool operator == (in Vector4d a, in Vector4d b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;
    public static bool operator != (in Vector4d a, in Vector4d b) => a.X != b.X || a.Y != b.Y || a.Z != b.Z || a.W != b.W;

    public static Vector4d operator + (in Vector4d a, in Vector4d b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
    public static Vector4d operator - (in Vector4d v) => new(-v.X, -v.Y, -v.Z, -v.W);
    public static Vector4d operator - (in Vector4d a, in Vector4d b) => a + -b;
    public static Vector4d operator * (double d, in Vector4d v) => new(d * v.X, d * v.Y, d * v.Z, d * v.W);
    public static Vector4d operator * (in Vector4d v, in Matrix4d m) => Transform(v, m);


    public override bool Equals (object obj) => obj is Vector4d d && d == this;
    public override int GetHashCode () => HashCode.Combine(X, Y, Z, W);
    public override string ToString () => $"({X}, {Y}, {Z}, {W})";

    public unsafe double this[int i] {
        get {
            fixed (Vector4d* p = &this)
                return 0 <= i && i <= 3 ? ((double*)p)[i] : throw new IndexOutOfRangeException("no such index");
        }
    }
}
