namespace Win32;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Vector3i {
    public readonly int X, Y, Z;
    public Vector3i (in int x, in int y, in int z) => (X, Y, Z) = (x, y, z);
    public static Vector3i operator + (Vector3i a, Vector3i b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3i operator - (Vector3i a, Vector3i b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
}
