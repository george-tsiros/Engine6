namespace Common;

using System.Numerics;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Quaterniond {
    public readonly double X, Y, Z, W;

    public static readonly Quaterniond Identity = new(0, 0, 0, 1);

    public Quaterniond (double x, double y, double z, double w) => (X, Y, Z, W) = (x, y, z, w);

    public static implicit operator Quaterniond (Quaternion q) => new(q.X, q.Y, q.Z, q.W);

    public static explicit operator Quaternion (Quaterniond q) => new((float)q.X, (float)q.Y, (float)q.Z, (float)q.W);

    public static Quaterniond CreateFromAxisAngle (Vector3d axis, double angle) {
        var (s, c) = double.SinCos(angle / 2);
        return new(axis.X * s, axis.Y * s, axis.Z * s, c);
    }

    public static Quaterniond Concatenate (Quaterniond value1, Quaterniond value2) {

        var q1x = value2.X;
        var q1y = value2.Y;
        var q1z = value2.Z;
        var q1w = value2.W;
        var q2x = value1.X;
        var q2y = value1.Y;
        var q2z = value1.Z;
        var q2w = value1.W;

        var cx = q1y * q2z - q1z * q2y;
        var cy = q1z * q2x - q1x * q2z;
        var cz = q1x * q2y - q1y * q2x;

        var dot = q1x * q2x + q1y * q2y + q1z * q2z;

        return new(q1x * q2w + q2x * q1w + cx,
         q1y * q2w + q2y * q1w + cy,
         q1z * q2w + q2z * q1w + cz,
         q1w * q2w - dot);
    }
}
