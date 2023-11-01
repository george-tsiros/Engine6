namespace Common;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Vector2i {

    public readonly int X, Y;
    public Vector2i (int x, int y) =>
        (X, Y) = (x, y);

    public static readonly Vector2i Zero = new();
    public static readonly Vector2i One = new(1, 1);
    public static readonly Vector2i UnitX = new(1, 0);
    public static readonly Vector2i UnitY = new(0, 1);

    public static Vector2i Min (in Vector2i a, in Vector2i b) =>
        new(int.Min(a.X, b.X), int.Min(a.Y, b.Y));

    public static Vector2i Max (in Vector2i a, in Vector2i b) =>
        new(int.Max(a.X, b.X), int.Max(a.Y, b.Y));

    public static Vector2i operator + (in Vector2i a, in Vector2i b) =>
        new(a.X + b.X, a.Y + b.Y);

    public static Vector2i operator - (in Vector2i a, in Vector2i b) =>
        new(a.X - b.X, a.Y - b.Y);

    public static Vector2i operator - (in Vector2i v) =>
        new(-v.X, -v.Y);

    public static Vector2 operator * (in Vector2i a, in Vector2 b) =>
        new(a.X * b.X, a.Y * b.Y);

    public static bool operator == (in Vector2i a, in Vector2i b) =>
        a.X == b.X && a.Y == b.Y;

    public static bool operator != (in Vector2i a, in Vector2i b) =>
        a.X != b.X || a.Y != b.Y;

    public static explicit operator Vector2 (in Vector2i v) =>
        new(v.X, v.Y);

    public static explicit operator Vector2i (in Vector2 v) =>
        new((int)v.X, (int)v.Y);

    public void Deconstruct (out int x, out int y) =>
        (x, y) = (X, Y);

    public override bool Equals (object obj) =>
        obj is Vector2i other && other == this;

    public override int GetHashCode () =>
        HashCode.Combine(X, Y);

    public override string ToString () =>
        $"({X}, {Y})";
}
