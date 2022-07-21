namespace Engine;

using BepuUtilities;
using System;
using System.Numerics;
using System.Runtime.InteropServices;


public readonly struct Vector3d {

    public readonly double X, Y, Z;

    public static readonly Vector3d
        Zero = new(),
        One = new(1, 1, 1),
        UnitX = new(1, 0, 0),
        UnitY = new(0, 1, 0),
        UnitZ = new(0, 0, 1);


    public Vector3d (double x, double y, double z) => (X, Y, Z) = (x, y, z);
    public Vector3d (Vector3 v) => (X, Y, Z) = (v.X, v.Y, 0);

    public static explicit operator Vector3 (Vector3d v) => new((float)v.X, (float)v.Y, (float)v.Z);

    public static double Dot (Vector3d a, Vector3d b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    public double MagnitudeSquared () => X * X + Y * Y + Z * Z;
    public double Magnitude () => Math.Sqrt(MagnitudeSquared());
    public static Vector3d Cross (Vector3d a, Vector3d b) => new(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
    public static Vector3d Normalize (Vector3d v) {
        var magnitude = v.Magnitude();
        return 1e-6 < magnitude ? 1 / magnitude * v : throw new ArgumentOutOfRangeException(nameof(v));
    }

    public static bool operator == (Vector3d a, Vector3d b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z;
    public static bool operator != (Vector3d a, Vector3d b) => a.X != b.X || a.Y != b.Y || a.Z != b.Z;
    public static Vector3d operator - (Vector3d v) => new(-v.X, -v.Y, -v.Z);
    public static Vector3d operator * (double d, Vector3d v) => new(d * v.X, d * v.Y, d * v.Z);
    public static Vector3d operator + (Vector3d a, Vector3d b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3d operator - (Vector3d a, Vector3d b) => a + -b;

    public override bool Equals (object obj) => obj is Vector3d d && d == this;
    public override int GetHashCode () => HashCode.Combine(X, Y, Z);
    public override string ToString () => $"({X}, {Y}, {Z})";

}

//[StructLayout(LayoutKind.Explicit)]
public readonly struct Matrix3x3 {
    public readonly float M11;
    public readonly float M12;
    public readonly float M13;
    public readonly float M21;
    public readonly float M22;
    public readonly float M23;
    public readonly float M31;
    public readonly float M32;
    public readonly float M33;
    public static readonly Matrix3x3 Zero = new();

    public static readonly Matrix3x3 Identity = new(Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ);

    public Matrix3x3 (Vector3 col0, Vector3 col1, Vector3 col2) : this() {
        M11 = col0.X; M21 = col1.X; M31 = col2.X;
        M12 = col0.Y; M22 = col1.Y; M32 = col2.Y;
        M13 = col0.Z; M23 = col1.Z; M33 = col2.Z;
    }

    public static Matrix3x3 From (Matrix4x4 m) => new(new(m.M11, m.M12, m.M13), new(m.M21, m.M22, m.M23), new(m.M31, m.M32, m.M33));

    public static Vector3 operator * (Matrix3x3 m, Vector3 v) => new(m.M11 * v.X + m.M21 * v.Y + m.M31 * v.Z, m.M12 * v.X + m.M22 * v.Y + m.M32 * v.Z, m.M13 * v.X + m.M23 * v.Y + m.M33 * v.Z);
    /*
    11 21 31
    12 22 32
    13 23 33
*/
    public bool TrySolve (Vector3 p, out Vector3 x) {
        var V = new Vector3d((double)M22 * M33 - (double)M32 * M23, (double)M32 * M13 - (double)M12 * M33, (double)M12 * M23 - (double)M22 * M13);
        var ddet = M11 * V.X + M21 * V.Y + M31 * V.Z;
        if (double.Abs(ddet) < float.Epsilon) {
            x = Vector3.Zero;
            return false;
        }
        var det = (float)ddet;
        var d = (M31 * M23 - M21 * M33) / det;
        var e = (M11 * M33 - M31 * M13) / det;
        var f = (M21 * M13 - M11 * M23) / det;
        var g = (M21 * M32 - M31 * M22) / det;
        var h = (M31 * M12 - M11 * M32) / det;
        var i = (M11 * M22 - M21 * M12) / det;
        x = new Matrix3x3((Vector3)V / det, new(d, e, f), new(g, h, i)) * p;

        return true;
    }
    public bool TrySolveDouble (Vector3 p, out Vector3 x) {
        var A = (double)M22 * M33 - (double)M32 * M23;
        var B = (double)M32 * M13 - (double)M12 * M33;
        var C = (double)M12 * M23 - (double)M22 * M13;
        var det = M11 * A + M21 * B + M31 * C;
        if (Math.Abs(det) < float.Epsilon) {
            x = Vector3.Zero;
            return false;
        }
        var d = (float)(((double)M31 * M23 - (double)M21 * M33) / det);
        var e = (float)(((double)M11 * M33 - (double)M31 * M13) / det);
        var f = (float)(((double)M21 * M13 - (double)M11 * M23) / det);
        var g = (float)(((double)M21 * M32 - (double)M31 * M22) / det);
        var h = (float)(((double)M31 * M12 - (double)M11 * M32) / det);
        var i = (float)(((double)M11 * M22 - (double)M21 * M12) / det);
        x = new Matrix3x3(new((float)(A / det), (float)(B / det), (float)(C / det)), new(d, e, f), new(g, h, i)) * p;

        return true;
    }
}
