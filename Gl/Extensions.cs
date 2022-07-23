namespace Gl;

using System;
using System.Numerics;

public static class Extensions {

    public static Vector3 Xyz (this Vector4 self) => new(self.X, self.Y, self.Z);
}
