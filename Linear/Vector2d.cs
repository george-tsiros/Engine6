namespace Linear;
using System;
using System.Numerics;
using static Maths;
public readonly struct Vector2d {

    public readonly double X, Y;

    public static readonly Vector2d
        Zero = new(),
        One = new(1, 1),
        UnitX = new(1, 0),
        UnitY = new(0, 1);

    public Vector2d (in double x, in double y) => (X, Y) = (x, y);
    public Vector2d (in Vector2 v) => (X, Y) = (v.X, v.Y);
    public Vector2d (in Vector2i v) => (X, Y) = (v.X, v.Y);
    public static explicit operator Vector2 (in Vector2d v) => new((float)v.X, (float)v.Y);
    public static implicit operator Vector2d (in Vector2 v) => new(v);
    public static implicit operator Vector2d (in Vector2i v) => new(v);
    public static double Dot (in Vector2d a, in Vector2d b) => a.X * b.X + a.Y * b.Y;
    public static Vector2i Round (in Vector2d v) => new((int)DoubleRound(v.X), (int)DoubleRound(v.Y));
    public double Magnitude () => DoubleSqrt(X * X + Y * Y);
    public static Vector2d Normalize (in Vector2d v) {
        var magnitude = v.Magnitude();
        return 1e-6 < magnitude ? 1 / magnitude * v : throw new ArgumentOutOfRangeException(nameof(v));
    }

    public static bool operator == (in Vector2d a, in Vector2d b) => a.X == b.X && a.Y == b.Y;
    public static bool operator != (in Vector2d a, in Vector2d b) => a.X != b.X || a.Y != b.Y;
    public static Vector2d operator - (in Vector2d v) => new(-v.X, -v.Y);
    public static Vector2d operator * (in double d, in Vector2d v) => new(d * v.X, d * v.Y);
    public static Vector2d operator * (in Vector2d a, in Vector2d b) => new(a.X * b.X, a.Y * b.Y);

    public static Vector2d operator + (in Vector2d a, in Vector2d b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2d operator - (in Vector2d a, in Vector2d b) => a + -b;

    public override bool Equals (object obj) => obj is Vector2d d && d == this;
    public override int GetHashCode () => HashCode.Combine(X, Y);
    public override string ToString () => $"({X}, {Y})";
}
