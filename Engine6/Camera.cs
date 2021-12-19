namespace Engine;

using System;
using System.Numerics;
using static Extra;

sealed class Camera {
    private static bool WasInvalid (ref bool isValid) {
        var eh = isValid;
        isValid = true;
        return !eh;
    }
    private Vector3 location;
    public Vector3 Location {
        get => location;
        set {
            lookAtIsValid = false;
            rotationOnlyIsValid = false;
            location = value;
        }
    }
    private float yaw = 0f, pitch = 0f;
    public Camera (Vector3 location) => Location = location;
    
    public Matrix4x4 RotationOnly => WasInvalid(ref rotationOnlyIsValid) ? (rotationOnly = CreateLookAt(Vector3.Zero, yaw, pitch)) : rotationOnly;
    public Matrix4x4 LookAtMatrix => WasInvalid(ref lookAtIsValid) ? (lookAt = CreateLookAt(location, yaw, pitch)) : lookAt;
    
    private bool rotationOnlyIsValid = false;
    private bool lookAtIsValid = false;
    private Matrix4x4 rotationOnly, lookAt;

    private static Matrix4x4 CreateLookAt (Vector3 location, float yaw, float pitch) {
        var rotationAboutY = Matrix4x4.CreateRotationY(yaw);
        var straightForward = Vector3.Transform(-Vector3.UnitZ, rotationAboutY);
        var right = Vector3.Transform(Vector3.UnitX, rotationAboutY);
        var rotationAboutRight = Matrix4x4.CreateFromAxisAngle(right, pitch);
        var forward = Vector3.Transform(straightForward, rotationAboutRight);
        var up = Vector3.Transform(Vector3.UnitY, rotationAboutRight);
        return Matrix4x4.CreateLookAt(location, location + forward, up);
    }

    public void Rotate (Vector2 v) {
        if (v.X != 0 || v.Y != 0) {
            lookAtIsValid = false;
            rotationOnlyIsValid = false;
            _ = ModuloTwoPi(ref yaw, -v.X);
            _ = Clamp(ref pitch, v.Y, (float)(-.4 * Math.PI), (float)(.4 * Math.PI));
        }
    }
}
