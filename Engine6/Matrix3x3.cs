namespace Engine;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public readonly struct Matrix3x3 {
    [FieldOffset(0 * sizeof(float))] public readonly float M11;
    [FieldOffset(1 * sizeof(float))] public readonly float M12;
    [FieldOffset(2 * sizeof(float))] public readonly float M13;
    [FieldOffset(3 * sizeof(float))] public readonly float M21;
    [FieldOffset(4 * sizeof(float))] public readonly float M22;
    [FieldOffset(5 * sizeof(float))] public readonly float M23;
    [FieldOffset(6 * sizeof(float))] public readonly float M31;
    [FieldOffset(7 * sizeof(float))] public readonly float M32;
    [FieldOffset(8 * sizeof(float))] public readonly float M33;
    [FieldOffset(0 * 3 * sizeof(float))] public readonly Vector3 Col0;
    [FieldOffset(1 * 3 * sizeof(float))] public readonly Vector3 Col1;
    [FieldOffset(2 * 3 * sizeof(float))] public readonly Vector3 Col2;

    public static readonly Matrix3x3 Zero = new();

    public static readonly Matrix3x3 Identity = new(Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ);

    public Matrix3x3 (Vector3 col0, Vector3 col1, Vector3 col2) : this() => (Col0, Col1, Col2) = (col0, col1, col2);

    public static Matrix3x3 From (Matrix4x4 m) => new(new(m.M11, m.M12, m.M13), new(m.M21, m.M22, m.M23), new(m.M31, m.M32, m.M33));

    public static Vector3 operator * (Matrix3x3 m, Vector3 v) => new(m.M11 * v.X + m.M21 * v.Y + m.M31 * v.Z, m.M12 * v.X + m.M22 * v.Y + m.M32 * v.Z, m.M13 * v.X + m.M23 * v.Y + m.M33 * v.Z);
    /*
    
    11 21 31
    12 22 32
    13 23 33
*/
    //public float Det () => M11 * (M22 * M33 - M32 * M23) - M21 * (M12 * M33 - M32 * M13) + M31 * (M12 * M23 - M22 * M13);
    //private Matrix3x3 Invert (float det) => throw new NotImplementedException();

    //public Matrix3x3 Invert () => Invert(Det());
    public bool TrySolve (Vector3 p, out Vector3 x) {
        var A = (M22 * M33 - M32 * M23);
        var B = -(M12 * M33 - M32 * M13);
        var C = (M12 * M23 - M22 * M13);
        var det = M11 * A + M12 * B + M13 * C;
        if (det < float.Epsilon) {
            x = Vector3.Zero;
            return false;
        }
        var d = -(M21 * M33 - M31 * M23) / det;
        var e = (M11 * M33 - M31 * M13) / det;
        var f = -(M11 * M23 - M21 * M13) / det;
        var g = (M21 * M32 - M31 * M22) / det;
        var h = -(M11 * M32 - M31 * M12) / det;
        var i = (M11 * M22 - M21 * M12) / det;
        x = new Matrix3x3(new((float)(A / det), (float)(B / det), (float)(C / det)), new(d, e, f), new(g, h, i)) * p;

        return true;
    }
}
