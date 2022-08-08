namespace Linear;
using System;
using static Linear.Maths;

public readonly struct Matrix3d {

    public readonly double 
        M11, M12, M13, 
        M21, M22, M23, 
        M31, M32, M33;

    public Matrix3d (double m11, double m12, double m13, double m21, double m22, double m23, double m31, double m32, double m33) {
        M11 = m11; M12 = m12; M13 = m13;
        M21 = m21; M22 = m22; M23 = m23;
        M31 = m31; M32 = m32; M33 = m33;
    }

    unsafe public Vector3d this[int row] {
        get {
            fixed (Matrix3d* self = &this)
                return 0 <= row && row <= 2
                    ? ((Vector3d*)self)[row]
                    : throw new IndexOutOfRangeException("no such row in a 4x4 matrix");
        }
    }

    unsafe public double this[int row, int column] {
        get {
            fixed (Matrix3d* self = &this)
                return 0 <= row && row <= 2 && 0 <= column && column <= 2
                    ? ((double*)self)[row * 3 + column]
                    : throw new IndexOutOfRangeException("no such index in a 4x4 matrix");
        }
    }

    public Vector3d Row1 => new(M11, M12, M13);
    public Vector3d Row2 => new(M21, M22, M23);
    public Vector3d Row3 => new(M31, M32, M33);

    public Vector3d Col1 => new(M11, M21, M31);
    public Vector3d Col2 => new(M12, M22, M32);
    public Vector3d Col3 => new(M13, M23, M33);

    public static Matrix3d operator * (Matrix3d a, Matrix3d b) {
        var m11 = Vector3d.Dot(a.Row1, b.Col1); var m12 = Vector3d.Dot(a.Row1, b.Col2); var m13 = Vector3d.Dot(a.Row1, b.Col3);
        var m21 = Vector3d.Dot(a.Row2, b.Col1); var m22 = Vector3d.Dot(a.Row2, b.Col2); var m23 = Vector3d.Dot(a.Row2, b.Col3);
        var m31 = Vector3d.Dot(a.Row3, b.Col1); var m32 = Vector3d.Dot(a.Row3, b.Col2); var m33 = Vector3d.Dot(a.Row3, b.Col3);
        return new(
            m11, m12, m13,
            m21, m22, m23,
            m31, m32, m33);
    }

    public static readonly Matrix3d Identity = new(
        1, 0, 0,
        0, 1, 0,
        0, 0, 1);
    public static readonly Matrix3d Zero = new();

    public double Det () {
        return M11 * (M22 * M33 - M23 * M32) - M12 * (M21 * M33 - M23 * M31) + M13 * (M21 * M32 - M22 * M31);
    }

    public static Matrix3d RotationX (double a) {
        var (s, c) = DoubleSinCos(a);
        return new Matrix3d(
            1, 0, 0,
            0, c, s,
            0, -s, c
        );
    }

    public static Matrix3d RotationY (double a) {
        var (s, c) = DoubleSinCos(a);
        return new Matrix3d(
            c, 0, -s,
            0, 1, 0, 
            s, 0, c 
        );
    }

    public static Matrix3d RotationZ (double a) {
        var (s, c) = DoubleSinCos(a);
        return new Matrix3d(
           c, s, 0, 
           -s, c, 0, 
           0, 0, 1
        );
    }

}
