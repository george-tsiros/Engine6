namespace Win32;

using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Vector2i {
    public readonly int X, Y;
    public Vector2i (in int x, in int y) => (X, Y) = (x, y);
    public static Vector2i operator + (Vector2i a, Vector2i b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2i operator - (Vector2i a, Vector2i b) => new(a.X - b.X, a.Y - b.Y);
    public static bool operator == (Vector2i a, Vector2i b) => a.X == b.X && a.Y == b.Y;
    public static bool operator != (Vector2i a, Vector2i b) => a.X != b.X || a.Y != b.Y;
    public void Deconstruct (out int x, out int y) => (x, y) = (X, Y);
    public override bool Equals ([NotNullWhen(true)] object obj) => obj is Vector2i other && other == this;
    public override int GetHashCode () => System.HashCode.Combine(X, Y);
}
