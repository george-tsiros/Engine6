namespace Gl;

using System;
using System.Numerics;

public static class Extensions {
    internal static (short x, short y) Split (this IntPtr self) {
        var i = (int)(self.ToInt64() & int.MaxValue);
        return ((short)(i & ushort.MaxValue), (short)((i >> 16) & ushort.MaxValue));
    }

    public static Vector3 Xyz (this Vector4 self) => new(self.X, self.Y, self.Z);
}
