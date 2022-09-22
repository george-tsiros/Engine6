namespace Engine6;
using Common;
using System.Diagnostics;
using System.Numerics;
using static Common.Maths;

interface ICamera {
    Vector3 Location { get; }
    Matrix4x4 GetViewMatrix ();
    Matrix4x4 GetRotationMatrix ();
    /// <summary>forward is -Z, +Z is backward, right is +X, up is +Y</summary>
    void Walk (float dx, float dy, float dz);
    /// <summary>pitch is about x axis, yaw is about y and roll is about z</summary>
    void Rotate (float yaw, float pitch, float roll);
}

sealed class Camera:ICamera {
    public Vector3 Location { get; private set; }
    private Quaternion quaternion = Quaternion.Identity;
    private Matrix4x4 rotationMatrix;
    private bool needsUpdate = false;
    public Camera (Vector3 location) {
        Location = location;
    }

    public Matrix4x4 GetRotationMatrix () {
        if (needsUpdate) {
            rotationMatrix = Matrix4x4.CreateFromQuaternion(quaternion);
            needsUpdate = false;
        }
        return rotationMatrix;
    }

    public Matrix4x4 GetViewMatrix () =>
        Matrix4x4.CreateTranslation(-Location) * GetRotationMatrix();

    public void Rotate (float yaw, float pitch, float roll) {
        var r = Quaternion.Concatenate(quaternion, Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll));
        if (quaternion != r) {
            needsUpdate = true;
            quaternion = r;
        }
    }
    public void Walk (float dx, float dy, float dz) {
        Location += Vector3.Transform(new Vector3(-dx, -dy, dz), GetRotationMatrix());
    }
}
