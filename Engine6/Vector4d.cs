namespace Engine;

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

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

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
    public Vector3d Xyz () => new(X, Y, Z);
#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
    public static double Dot (in Vector4d a, in Vector4d b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
    public double Magnitude () => Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
    public static Vector4d Normalize (in Vector4d v) {
        var magnitude = v.Magnitude();
        return 1e-6 < magnitude ? 1 / magnitude * v : throw new ArgumentOutOfRangeException(nameof(v));
    }
#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
    public static Vector4d Transform (in Vector4d v, in Matrix4d m) => new(Vector4d.Dot(v, m.Col1), Vector4d.Dot(v, m.Col2), Vector4d.Dot(v, m.Col3), Vector4d.Dot(v, m.Col4));

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
    public static bool operator == (in Vector4d a, in Vector4d b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;
#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
    public static bool operator != (in Vector4d a, in Vector4d b) => a.X != b.X || a.Y != b.Y || a.Z != b.Z || a.W != b.W;
#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
    public static Vector4d operator - (in Vector4d v) => new(-v.X, -v.Y, -v.Z, -v.W);
#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
    public static Vector4d operator * (double d, in Vector4d v) => new(d * v.X, d * v.Y, d * v.Z, d * v.W);
#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
    public static Vector4d operator * (in Vector4d v, in Matrix4d m) => Transform(v, m);
#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
    public static Vector4d operator + (in Vector4d a, in Vector4d b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
    public static Vector4d operator - (in Vector4d a, in Vector4d b) => a + -b;

    public override bool Equals (object obj) => obj is Vector4d d && d == this;
    public override int GetHashCode () => HashCode.Combine(X, Y, Z, W);
    public override string ToString () => $"({X}, {Y}, {Z}, {W})";

    public unsafe double this[int i] {
#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
        get {
            fixed (Vector4d* p = &this)
                return 0 <= i && i <= 3 ? ((double*)p)[i] : throw new IndexOutOfRangeException("no such index");
        }
    }
}
