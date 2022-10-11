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
    public Matrix4x4 CreateModelMatrix() => Matrix4x4.CreateFromQuaternion(Orientation) * Matrix4x4.CreateTranslation((Vector3)Position);
}

