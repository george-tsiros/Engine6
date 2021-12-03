namespace Gl;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Vector2i {
    public readonly int X, Y;
    public Vector2i (in int x, in int y) => (X, Y) = (x, y);
    public static Vector2i operator + (Vector2i a, Vector2i b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2i operator - (Vector2i a, Vector2i b) => new(a.X - b.X, a.Y - b.Y);
    public void Deconstruct (out int x, out int y) => (x, y) = (X, Y);
}


[StructLayout(LayoutKind.Sequential)]
public readonly struct Vector3i {
    public readonly int X, Y, Z;
    public Vector3i (in int x, in int y, in int z) => (X, Y, Z) = (x, y, z);
    public static Vector3i operator + (Vector3i a, Vector3i b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3i operator - (Vector3i a, Vector3i b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
}

[StructLayout(LayoutKind.Sequential)]
public struct ArraysCommand {
    public int VerticesCount, InstancesCount, FirstVertexIndex, BaseInstanceIndex;
}

[StructLayout(LayoutKind.Sequential)]
public struct ElementsCommand {
    public int VerticesCount, InstancesCount, FirstVertexIndex, BaseVertexIndex, BaseInstanceIndex;
}
