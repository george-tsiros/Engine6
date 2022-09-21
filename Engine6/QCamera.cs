namespace Engine6;

using System.Numerics;

interface ICamera {
    Vector3 Location { get; }
    Matrix4x4 LookAtMatrix { get; }
    /// <summary>forward is -Z, +Z is backward, right is +X, up is +Y</summary>
    void Walk (float dx, float dy, float dz);
    /// <summary>pitch is x, yaw is y, roll is z</summary>
    void Rotate (float pitch, float yaw, float roll);
}

sealed class QCamera:ICamera {
    public Vector3 Location { get; private set; }
    private Quaternion q = Quaternion.Identity;

    public QCamera (Vector3 location) {
        Location = location;
    }

    public Matrix4x4 LookAtMatrix =>
        Matrix4x4.Transform(Matrix4x4.CreateTranslation(-Location), q);

    public void Rotate (float pitch, float yaw, float roll) {
        q = Quaternion.Concatenate(q, Quaternion.CreateFromAxisAngle(Vector3.UnitX, pitch));
        q = Quaternion.Concatenate(q, Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw));
        q = Quaternion.Concatenate(q, Quaternion.CreateFromAxisAngle(-Vector3.UnitZ, roll));
    }

    public void Walk (float dx, float dy, float dz) {
        //Location += new Vector3(dx, dy, dz);
        Location += Vector3.Transform(new Vector3(dx, dy, dz), q);
    }
}
