namespace Common;
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Vector3i {
    public readonly int X, Y, Z;

    public static readonly Vector3i
        Zero = new(),
        One = new(1, 1, 1),
        UnitX = new(1, 0, 0),
        UnitY = new(0, 1, 0),
        UnitZ = new(0, 0, 1);

    public Vector3i (int x, int y, int z) => (X, Y, Z) = (x, y, z);
    public Vector3i (in Vector2i v2, int z) => (X, Y, Z) = (v2.X, v2.Y, z);

    public Vector2i Xy () => new(X, Y);
    public static double Dot (in Vector3i a, in Vector3i b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public static bool operator == (in Vector3i a, in Vector3i b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z;
    public static bool operator != (in Vector3i a, in Vector3i b) => a.X != b.X || a.Y != b.Y || a.Z != b.Z;
    public static Vector3i operator - (in Vector3i v) => new(-v.X, -v.Y, -v.Z);
    public static Vector3i operator * (in int d, in Vector3i v) => new(d * v.X, d * v.Y, d * v.Z);
    public static Vector3i operator * (in Vector3i a, in Vector3i b) => new(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    public static Vector3i operator + (in Vector3i a, in Vector3i b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3i operator - (in Vector3i a, in Vector3i b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public override bool Equals (object obj) => obj is Vector3i d && d == this;
    public override int GetHashCode () => HashCode.Combine(X, Y, Z);
    public override string ToString () => $"({X}, {Y}, {Z})";
    public void Deconstruct (out int x, out int y, out int z) => (x, y, z) = (X, Y, Z);
}
