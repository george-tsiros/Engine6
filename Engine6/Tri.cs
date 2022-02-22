namespace Engine;
using System.Numerics;
using System;
readonly struct Tri {
    public readonly Vector3 Origin, Va, Vb;
    public Tri (Vector3 origin, Vector3 a, Vector3 b) => (Origin, Va, Vb) = (origin, a, b);
    public static Tri FromPoints (Vector3 a, Vector3 b, Vector3 c) => new(a, b - a, c - a);
    public static bool Intersects (in Tri tri, in Ray r, out float distance) {
        var intersects = TrySolve(tri, r, out Vector3 s);
        distance = s.X;
        return intersects && 0 < s.X && 0 < s.Y && s.Y + s.Z < 0.5f && 0 < s.Z;
    }
    
    public static float Distance (in Tri tri, in Ray r) => TrySolve(tri, r, out var s) && 0 < s.X && 0 < s.Y && s.Y + s.Z <= 0.5f && 0 < s.Z ? s.X : float.MaxValue;
    //public static float Distance (in Tri tri,in Vector3 r) => TrySolve(tri, r, out var s) && 0 < s.X && 0 < s.Y && s.Y + s.Z <= 0.5f && 0 < s.Z ? s.X : float.MaxValue;

    static bool TrySolve (Tri tri, in Vector3 ray, out Vector3 solution) {
        solution = Vector3.Zero;
        var det = Det(ray, tri.Va, tri.Vb);
        if (Math.Abs(det) < 1e-10f)
            return false;
        var x = Det(tri.Origin, tri.Va, tri.Vb);
        var y = Det(ray, tri.Origin, tri.Vb);
        var z = Det(ray, tri.Va, tri.Origin);
        solution = new Vector3(x, y, z) / det;
        return true;
    }
    static bool TrySolve (Tri tri, in Ray ray, out Vector3 solution) {
        solution = Vector3.Zero;
        var det = Det(ray.Direction, tri.Va, tri.Vb);
        if (Math.Abs(det) < 1e-10f)
            return false;
        var origin = tri.Origin - ray.Origin;
        var x = Det(origin, tri.Va, tri.Vb);
        var y = Det(ray.Direction, origin, tri.Vb);
        var z = Det(ray.Direction, tri.Va, origin);
        solution = new Vector3(x, y, z) / det;
        return true;
    }
    static float Det (in Vector3 d, in Vector3 a, in Vector3 b) {
        var d0 = d.X * ((double)a.Y * b.Z - b.Y * a.Z);
        var d1 = a.X * ((double)d.Y * b.Z - b.Y * d.Z);
        var d2 = b.X * ((double)d.Y * a.Z - a.Y * d.Z);
        return (float)(d0 - d1 + d2);
    }
}
