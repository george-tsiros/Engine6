namespace Engine6;
using Common;
using System.Numerics;

public sealed class Camera {

    public Camera () : this(Vector3.Zero) { }

    public Camera (Vector3 position) => Position = position;

    public Quaterniond Orientation { get; private set; } = Quaterniond.Identity;
    public Vector3d Position { get; private set; }

    public void Rotate (double pitch, double yaw, double roll) {
        var qPitch = Quaterniond.CreateFromAxisAngle(Vector3.UnitX, pitch);
        var q = Quaterniond.Concatenate(Orientation, qPitch);
        q = Quaterniond.Concatenate(q, Quaterniond.CreateFromAxisAngle(Vector3d.Transform(Vector3d.UnitY, qPitch), yaw));
        Orientation = Quaterniond.Concatenate(q, Quaterniond.CreateFromAxisAngle(Vector3d.Transform(Vector3.UnitZ, qPitch), roll));
    }

    public void Translate (Vector4d vector) {
        Position += vector.Xyz();
    }

    public Matrix4d CreateView () =>
        Matrix4d.CreateTranslation(-Position) * Matrix4d.CreateFromQuaternion(Orientation);
}

public sealed class Cameraf {
    public Cameraf () : this(Vector3.Zero) { }
    public Cameraf (Vector3 position) {
        Position = position;
    }
    public Quaternion Orientation { get; private set; } = Quaternion.Identity;
    public Vector3 Position { get; private set; }
    public void Rotate (float pitch, float yaw, float roll) {
        var qPitch = Quaternion.CreateFromAxisAngle(Vector3.UnitX, pitch);
        var q = Quaternion.Concatenate(Orientation, qPitch);
        q = Quaternion.Concatenate(q, Quaternion.CreateFromAxisAngle(Vector3.Transform(Vector3.UnitY, qPitch), yaw));
        Orientation = Quaternion.Concatenate(q, Quaternion.CreateFromAxisAngle(Vector3.Transform(Vector3.UnitZ, qPitch), roll));
    }

    public void Translate (Vector4 vector) {
        Position += vector.Xyz();
    }

    public Matrix4x4 CreateView () =>
        Matrix4x4.CreateTranslation(-Position) * Matrix4x4.CreateFromQuaternion(Orientation);

}