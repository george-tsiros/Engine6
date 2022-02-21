namespace Engine;
using System.Numerics;
using System;
readonly struct Tri {
    public readonly Vector3 Origin, Va, Vb;
    //public Vector3 Normal => Vector3.Normalize(Vector3.Cross(Va, Vb));
    //public Vector3 A => Origin + Va;
    //public Vector3 B => Origin + Vb;
    //public Vector3 Center => Origin + (Va + Vb) * 0.5f;
    public Tri (Vector3 origin, Vector3 a, Vector3 b) => (Origin, Va, Vb) = (origin, a, b);
    public static Tri FromPoints (Vector3 a, Vector3 b, Vector3 c) => new(a, b - a, c - a);
    public bool Intersects (in Ray r, out float distance) {
        var intersects = TrySolve(r, out Vector3 s);
        distance = s.X;
        return intersects && 0 < s.X && 0 < s.Y && s.Y + s.Z < 0.5f && 0 < s.Z;
    }
    
    public float Distance (in Ray r) => TrySolve(r, out var s) && 0 < s.X && 0 < s.Y && s.Y + s.Z <= 0.5f && 0 < s.Z ? s.X : float.MaxValue;
    public float Distance (in Vector3 r) => TrySolve(r, out var s) && 0 < s.X && 0 < s.Y && s.Y + s.Z <= 0.5f && 0 < s.Z ? s.X : float.MaxValue;

    bool TrySolve (in Vector3 ray, out Vector3 solution) {
        solution = Vector3.Zero;
        var det = Det(ray, Va, Vb);
        if (Math.Abs(det) < 1e-10f)
            return false;
        var x = Det(Origin, Va, Vb);
        var y = Det(ray, Origin, Vb);
        var z = Det(ray, Va, Origin);
        solution = new Vector3(x, y, z) / det;
        return true;
    }
    bool TrySolve (in Ray ray, out Vector3 solution) {
        solution = Vector3.Zero;
        var det = Det(ray.Direction, Va, Vb);
        if (Math.Abs(det) < 1e-10f)
            return false;
        var origin = Origin - ray.Origin;
        var x = Det(origin, Va, Vb);
        var y = Det(ray.Direction, origin, Vb);
        var z = Det(ray.Direction, Va, origin);
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
