namespace Engine6;
using Common;
using System.Numerics;

public class Pose {
    public Vector3d Position;
    public Quaternion Orientation;
    public Vector3d LinearVelocity;
    public Vector3 AngularVelocity;
    public Pose () {
        Orientation = Quaternion.Identity;
    }
}

