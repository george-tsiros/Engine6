namespace Common;

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using static Common.Maths;

[StructLayout(LayoutKind.Explicit)]
public readonly struct Matrix4d {

    [FieldOffset(0 * sizeof(double))] public readonly double M11;
    [FieldOffset(1 * sizeof(double))] public readonly double M12;
    [FieldOffset(2 * sizeof(double))] public readonly double M13;
    [FieldOffset(3 * sizeof(double))] public readonly double M14;
    [FieldOffset(4 * sizeof(double))] public readonly double M21;
    [FieldOffset(5 * sizeof(double))] public readonly double M22;
    [FieldOffset(6 * sizeof(double))] public readonly double M23;
    [FieldOffset(7 * sizeof(double))] public readonly double M24;
    [FieldOffset(8 * sizeof(double))] public readonly double M31;
    [FieldOffset(9 * sizeof(double))] public readonly double M32;
    [FieldOffset(10 * sizeof(double))] public readonly double M33;
    [FieldOffset(11 * sizeof(double))] public readonly double M34;
    [FieldOffset(12 * sizeof(double))] public readonly double M41;
    [FieldOffset(13 * sizeof(double))] public readonly double M42;
    [FieldOffset(14 * sizeof(double))] public readonly double M43;
    [FieldOffset(15 * sizeof(double))] public readonly double M44;

    [FieldOffset(0 * 4 * sizeof(double))] public readonly Vector4d Row1;
    [FieldOffset(1 * 4 * sizeof(double))] public readonly Vector4d Row2;
    [FieldOffset(2 * 4 * sizeof(double))] public readonly Vector4d Row3;
    [FieldOffset(3 * 4 * sizeof(double))] public readonly Vector4d Row4;

    public Matrix4d (
        double m11, double m12, double m13, double m14,
        double m21, double m22, double m23, double m24,
        double m31, double m32, double m33, double m34,
        double m41, double m42, double m43, double m44) {
        M11 = m11; M12 = m12; M13 = m13; M14 = m14;
        M21 = m21; M22 = m22; M23 = m23; M24 = m24;
        M31 = m31; M32 = m32; M33 = m33; M34 = m34;
        M41 = m41; M42 = m42; M43 = m43; M44 = m44;
    }

    public unsafe Vector4d this[int row] {
        get {
            fixed (Matrix4d* self = &this)
                return 0 <= row && row <= 3
                    ? ((Vector4d*)self)[row]
                    : throw new IndexOutOfRangeException("no such row in a 4x4 matrix");
        }
    }

    public unsafe double this[int row, int column] {
        get {
            fixed (Matrix4d* self = &this)
                return 0 <= row && row <= 3 && 0 <= column && column <= 3
                    ? ((double*)self)[row * 4 + column]
                    : throw new IndexOutOfRangeException("no such index in a 4x4 matrix");
        }
    }

    public Vector4d Col1 => new(M11, M21, M31, M41);
    public Vector4d Col2 => new(M12, M22, M32, M42);
    public Vector4d Col3 => new(M13, M23, M33, M43);
    public Vector4d Col4 => new(M14, M24, M34, M44);

    public static Matrix4d operator * (Matrix4d a, Matrix4d b) {
        var m11 = Vector4d.Dot(in a.Row1, b.Col1); var m12 = Vector4d.Dot(in a.Row1, b.Col2); var m13 = Vector4d.Dot(in a.Row1, b.Col3); var m14 = Vector4d.Dot(in a.Row1, b.Col4);
        var m21 = Vector4d.Dot(in a.Row2, b.Col1); var m22 = Vector4d.Dot(in a.Row2, b.Col2); var m23 = Vector4d.Dot(in a.Row2, b.Col3); var m24 = Vector4d.Dot(in a.Row2, b.Col4);
        var m31 = Vector4d.Dot(in a.Row3, b.Col1); var m32 = Vector4d.Dot(in a.Row3, b.Col2); var m33 = Vector4d.Dot(in a.Row3, b.Col3); var m34 = Vector4d.Dot(in a.Row3, b.Col4);
        var m41 = Vector4d.Dot(in a.Row4, b.Col1); var m42 = Vector4d.Dot(in a.Row4, b.Col2); var m43 = Vector4d.Dot(in a.Row4, b.Col3); var m44 = Vector4d.Dot(in a.Row4, b.Col4);
        return new(
            m11, m12, m13, m14,
            m21, m22, m23, m24,
            m31, m32, m33, m34,
            m41, m42, m43, m44);
    }

    public static readonly Matrix4d Identity = new(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);

    public static readonly Matrix4d Zero = new();

    public static implicit operator Matrix4d (Matrix4x4 m) => new(
        m.M11, m.M12, m.M13, m.M14,
        m.M21, m.M22, m.M23, m.M24,
        m.M31, m.M32, m.M33, m.M34,
        m.M41, m.M42, m.M43, m.M44);

    public static explicit operator Matrix4x4 (Matrix4d m) => new(
        (float)m.M11, (float)m.M12, (float)m.M13, (float)m.M14,
        (float)m.M21, (float)m.M22, (float)m.M23, (float)m.M24,
        (float)m.M31, (float)m.M32, (float)m.M33, (float)m.M34,
        (float)m.M41, (float)m.M42, (float)m.M43, (float)m.M44);

    //public static Matrix4d LookAt (Vector3d from, Vector3d to) => LookAt(from, to, Vector3.UnitY);
    //public static Matrix4d LookAt (Vector3d from, Vector3d to, Vector3d up) {
    //    var forward = Vector3d.Normalize(to - from);
    //    var right = 
    //}
    /*
    2n/(r+r) 0 0 0 
    0 2n/(t+t) 0 0 
    0 0 -(f+n)/(f-n) -1
    0 0 -2dfn/(f-n) 0
    
    n/r

*/
    public static Matrix4d CreatePerspectiveFieldOfView (double fieldOfView, double aspectRatio, double nearPlaneDistance, double farPlaneDistance) {
        if (fieldOfView <= 0.0 || fieldOfView >= Maths.dPi)
            throw new ArgumentOutOfRangeException(nameof(fieldOfView));

        if (nearPlaneDistance <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(nearPlaneDistance));

        if (farPlaneDistance <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(farPlaneDistance));

        if (nearPlaneDistance >= farPlaneDistance)
            throw new ArgumentOutOfRangeException(nameof(nearPlaneDistance));

        var yScale = 1.0f / Maths.DoubleTan(fieldOfView * 0.5);
        var xScale = yScale / aspectRatio;

        var negFarRange = double.IsPositiveInfinity(farPlaneDistance) ? -1.0 : farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
        return new Matrix4d(
            xScale, 0, 0, 0, 
            0, yScale, 0, 0, 
            0, 0, negFarRange, -1, 
            0, 0, nearPlaneDistance * negFarRange, 0);
    }

    public static Matrix4d Transpose (Matrix4d m) => new(
        m.M11, m.M21, m.M31, m.M41,
        m.M12, m.M22, m.M32, m.M42,
        m.M13, m.M23, m.M33, m.M43,
        m.M14, m.M24, m.M34, m.M44);

    public static Matrix4d Translate (Vector3d v) => Translate(v.X, v.Y, v.Z);
    public static Matrix4d Translate (double dx, double dy, double dz) => new(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        dx, dy, dz, 1
    );

    public static Matrix4d RotationX (double a) {
        var (s, c) = DoubleSinCos(a);
        return new Matrix4d(
            1, 0, 0, 0,
            0, c, s, 0,
            0, -s, c, 0,
            0, 0, 0, 1
        );
    }

    public static Matrix4d RotationY (double a) {
        var (s, c) = DoubleSinCos(a);
        return new Matrix4d(
            c, 0, -s, 0,
            0, 1, 0, 0,
            s, 0, c, 0,
            0, 0, 0, 1
        );
    }

    public static Matrix4d RotationZ (double a) {
        var (s, c) = DoubleSinCos(a);
        return new Matrix4d(
           c, s, 0, 0,
           -s, c, 0, 0,
           0, 0, 1, 0,
           0, 0, 0, 1
        );
    }
}
