namespace Engine6;

using System.Numerics;
using static Common.Maths;

interface ICamera {
    Vector3 Location { get; set; }
    Matrix4x4 LookAtMatrix { get; }
    void Walk (Vector3 d);
    void Rotate (Vector2 d);

}

sealed class QCamera:ICamera {
    public Vector3 Location { get; set; }
    public QCamera () { }
    private Quaternion orientation;

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

    public void Walk (Vector3 d) =>
        Location += Vector3.Transform(d, Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw));

    public void Rotate (Vector2 v) {
        if (v.X != 0 || v.Y != 0) {
            lookAtIsValid = false;
            yaw = (yaw - v.X) % fTau;
            pitch = FloatClamp(pitch + v.Y, -.4f * fPi, .4f * fPi);
        }
    }
}
