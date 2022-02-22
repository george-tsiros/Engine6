namespace Engine;
using System.Numerics;
using System;
readonly struct Tri {
    public readonly Vector3 Origin, Va, Vb;
    public Tri (Vector3 origin, Vector3 a, Vector3 b) => (Origin, Va, Vb) = (origin, a, b);
    public static Tri FromPoints (Vector3 a, Vector3 b, Vector3 c) => new(a, b - a, c - a);
}
