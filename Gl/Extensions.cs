namespace Gl;

using System.Numerics;
using Common;

public static class Extensions {

    public static Vector3 Xyz (this Vector4 self) => new(self.X, self.Y, self.Z);
    public static Vector2 Xy (this Vector4 self) => new(self.X, self.Y);
    public static Vector2 Xy (this Vector3 self) => new(self.X, self.Y);
    public static (T, T, T) Dex<T> (this T[] self, Vector3i i) => (self[i.X], self[i.Y], self[i.Z]);
    public static void Deconstruct (this Vector3 self, out float x, out float y, out float z) => (x, y, z) = (self.X, self.Y, self.Z);
    public static Vector2i Round (this Vector2 self) => new((int)Maths.FloatRound(self.X), (int)Maths.FloatRound(self.Y));
}
