
using System.Numerics;

namespace Engine6;
readonly struct Ray {
    public readonly Vector3 Origin, Direction;
    public Ray (Vector3 origin, Vector3 direction) {
        Origin = origin;
        Direction = Vector3.Normalize(direction);
    }
}
