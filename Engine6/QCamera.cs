namespace Engine6;

using System;
using System.Diagnostics;
using System.Numerics;
using static Common.Maths;

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

    public Vector3 Forward =>
        Vector3.Transform(-Vector3.UnitZ, q);

    public Vector3 Up =>
        Vector3.Transform(Vector3.UnitY, q);

    public Vector3 Right =>
        Vector3.Transform(Vector3.UnitX, q);

    public Matrix4x4 LookAtMatrix =>
        Matrix4x4.CreateTranslation(-Location) * Matrix4x4.CreateFromQuaternion(q);

    public void Rotate (float pitch, float yaw, float roll) {
        q *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, pitch);
        q *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw);
        q *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, roll);
    }

    public void Walk (float dx, float dy, float dz) {
    }
}
