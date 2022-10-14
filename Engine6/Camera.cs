namespace Engine6;
using Common;
using System.Numerics;

public sealed class Camera {
    
    public Camera () : this(Vector3.Zero) {
    }
    public Camera (Vector3 position) => Position = position;

    public Quaterniond Orientation { get; private set; } = Quaterniond.Identity;
    public Vector3d Position { get; private set; }

    public void Rotate (double pitch, double yaw, double roll) {
        var qPitch = Quaterniond.CreateFromAxisAngle(Vector3.UnitX, pitch);
        var q = Quaterniond.Concatenate(Orientation, qPitch);
        q = Quaterniond.Concatenate(q, Quaterniond.CreateFromAxisAngle(Vector3d.Transform(Vector3d.UnitY, qPitch), yaw));
        Orientation = Quaterniond.Concatenate(q, Quaterniond.CreateFromAxisAngle(Vector3d.Transform(Vector3.UnitZ, qPitch), roll));
    }

    public void Move (double distance) {
        var forward = Vector3d.Transform(-Vector3d.UnitZ, Orientation);
        Position += distance * forward;
    }
    public Matrix4d CreateView () =>
        Matrix4d.CreateTranslation(-Position) * Matrix4d.CreateFromQuaternion(Orientation);
}
