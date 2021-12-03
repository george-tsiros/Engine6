namespace Gl;

using System;
using System.Numerics;

public static class Extensions {
    public static Vector3 Xyz (this Vector4 self) => new(self.X, self.Y, self.Z);
    public static float Float (this Random self, float min, float max) => (float)(self.NextDouble() * (max - min) + min);
    public static Vector3 Vector3 (this Random self, Vector3 min, Vector3 max) => new(self.Float(min.X, max.X), self.Float(min.Y, max.Y), self.Float(min.Z, max.Z));
}
