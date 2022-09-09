namespace Engine6;

using System;
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
    public QCamera (Vector3 location) {
        Location = location;
        orientation = Quaternion.Identity;
    }

    private Quaternion orientation;
    public Matrix4x4 LookAtMatrix {
        get {
            var target = Vector3.Transform(-Vector3.UnitZ, orientation);
            var up = Vector3.Transform(Vector3.UnitY, orientation);
            return Matrix4x4.CreateLookAt(Location, target, up);
        }
    }
    public void Rotate (float pitch, float yaw, float roll) {
        orientation += Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
        
    }
    public void Walk (float dx, float dy, float dz) { }
}

sealed class Camera:ICamera {
    private static bool WasInvalid (ref bool isValid) {
        var eh = isValid;
        isValid = true;
        return !eh;
    }
    private Vector3 location;
    public Vector3 Location {
        get =>
            location;
        set {
            if (location == value)
                return;
            lookAtIsValid = false;
            location = value;
        }
    }

    private float yaw = 0f, pitch = 0f;
    public Camera (Vector3 location) =>
        Location = location;

    public Matrix4x4 LookAtMatrix =>
        WasInvalid(ref lookAtIsValid) ? (lookAt = CreateLookAt(location, yaw, pitch)) : lookAt;

    private bool lookAtIsValid = false;
    private Matrix4x4 lookAt;

    private static Matrix4x4 CreateLookAt (Vector3 location, float yaw, float pitch) {
        var rotationAboutY = Matrix4x4.CreateRotationY(yaw);
        var straightForward = Vector3.Transform(-Vector3.UnitZ, rotationAboutY);
        var right = Vector3.Transform(Vector3.UnitX, rotationAboutY);
        var rotationAboutRight = Matrix4x4.CreateFromAxisAngle(right, pitch);
        var forward = Vector3.Transform(straightForward, rotationAboutRight);
        var up = Vector3.Transform(Vector3.UnitY, rotationAboutRight);
        return Matrix4x4.CreateLookAt(location, location + forward, up);
    }

    public void Walk (float dx, float dy, float dz) =>
        Location += Vector3.Transform(new(dx, dy, dz), Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw));

    public void Rotate (float pitch, float yaw, float roll) {
        if (0 != pitch || 0 != yaw || 0 != roll) {
            lookAtIsValid = false;
            this.yaw = (this.yaw - yaw) % fTau;
            this.pitch = FloatClamp(this.pitch + pitch, -.4f * fPi, .4f * fPi);
        }
    }
}
