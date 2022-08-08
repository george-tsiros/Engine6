namespace Engine;

using System;
using System.Numerics;

public readonly struct Vector3d {

    public readonly double X, Y, Z;

    public static readonly Vector3d
        Zero = new(),
        One = new(1, 1, 1),
        UnitX = new(1, 0, 0),
        UnitY = new(0, 1, 0),
        UnitZ = new(0, 0, 1),
        MaxValue = new(double.MaxValue,double.MaxValue,double.MaxValue),
        MinValue = new(double.MinValue,double.MinValue,double.MinValue);

    public Vector3d (double value) => (X, Y, Z) = (value, value, value);
    public Vector3d (double x, double y, double z) => (X, Y, Z) = (x, y, z);
    public Vector3d (in Vector3 v) => (X, Y, Z) = (v.X, v.Y, 0);

    public static explicit operator Vector3 (in Vector3d v) => new((float)v.X, (float)v.Y, (float)v.Z);

    public static double Dot (in Vector3d a, in Vector3d b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    public double MagnitudeSquared () => X * X + Y * Y + Z * Z;
    public double Magnitude () => double.Sqrt(MagnitudeSquared());
    public static Vector3d Cross (in Vector3d a, in Vector3d b) => new(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
    public static Vector3d Normalize (in Vector3d v) {
        var magnitude = v.Magnitude();
        return 1e-6 < magnitude ? 1 / magnitude * v : throw new ArgumentOutOfRangeException(nameof(v));
    }
    public static Vector3d Min (in Vector3d a, in Vector3d b) => new(double.Min(a.X, b.X), double.Min(a.Y, b.Y), double.Min(a.Z, b.Z));
    public static Vector3d Max (in Vector3d a, in Vector3d b) => new(double.Max(a.X, b.X), double.Max(a.Y, b.Y), double.Max(a.Z, b.Z));
    public static bool operator == (in Vector3d a, in Vector3d b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z;
    public static bool operator != (in Vector3d a, in Vector3d b) => a.X != b.X || a.Y != b.Y || a.Z != b.Z;
    public static Vector3d operator - (in Vector3d v) => new(-v.X, -v.Y, -v.Z);
    public static Vector3d operator * (double d, in Vector3d v) => new(d * v.X, d * v.Y, d * v.Z);
    public static Vector3d operator + (in Vector3d a, in Vector3d b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3d operator - (in Vector3d a, in Vector3d b) => a + -b;
    public static Vector3d Transform (in Vector3d v, in Matrix3d m) => new(Dot(v, m.Col1), Dot(v, m.Col2), Dot(v, m.Col3));
    public static Vector3d operator * (in Vector3d v, in Matrix3d m) => Transform(v, m);
    public override bool Equals (object obj) => obj is Vector3d d && d == this;
    public override int GetHashCode () => HashCode.Combine(X, Y, Z);
    public override string ToString () => $"({X}, {Y}, {Z})";
}
