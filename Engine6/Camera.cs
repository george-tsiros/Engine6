namespace Engine;

using System;
using System.Collections.Generic;
using System.Numerics;
using static Extra;
using Gl;

sealed class Camera {

    public Vector3 Location;
    private Vector3 direction = Vector3.Zero;
    private float yaw = 0f, pitch = 0f;
    public Camera (Vector3 location) => Location = location;
    public Matrix4x4 RotationOnly => CreateLookAt(Vector3.Zero, yaw, pitch);

    private static Matrix4x4 CreateLookAt (Vector3 location, float yaw, float pitch) {
        var rotationAboutY = Matrix4x4.CreateRotationY(yaw);
        var straightForward = Vector3.Transform(-Vector3.UnitZ, rotationAboutY);
        var right = Vector3.Transform(Vector3.UnitX, rotationAboutY);
        var rotationAboutRight = Matrix4x4.CreateFromAxisAngle(right, pitch);
        var forward = Vector3.Transform(straightForward, rotationAboutRight);
        var up = Vector3.Transform(Vector3.UnitY, rotationAboutRight);
        return Matrix4x4.CreateLookAt(location, location + forward, up);
    }

    public Matrix4x4 LookAtMatrix => CreateLookAt(Location, yaw, pitch);

    public void Move (float dt) {
        if (dt > 0f && direction.LengthSquared() > .1f)
            Location += Vector3.Transform(Vector3.Normalize(direction) * dt, Matrix4x4.CreateRotationY(yaw));
    }
    public void Mouse (Vector2 v) {
        _ = ModuloTwoPi(ref yaw, -v.X);
        _ = Clamp(ref pitch, v.Y, (float)(-.4 * Math.PI), (float)(.4 * Math.PI));
    }
}
