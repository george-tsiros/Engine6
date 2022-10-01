namespace Engine6;
using Win32;
using Common;
using Gl;
using static Gl.GlContext;
using System.Numerics;
using Shaders;

public class MatrixTests:GlWindow {
    public MatrixTests () {
        ClientSize = new(1600, 1200);
    }

    protected override Key[] AxisKeys { get; } = { Key.Z, Key.X, Key.C, Key.D, Key.Q, Key.A, Key.Insert, Key.Home, Key.PageUp, Key.Delete, Key.End, Key.PageDown };

    Diy diy;
    VertexArray va;
    BufferObject<Vector4> vertices;
    static readonly Vector4[] lineSegments = { new(Vector3.Zero, 1), new(Vector3.UnitX, 1), new(Vector3.Zero, 1), new(Vector3.UnitY, 1), new(Vector3.Zero, 1), new(Vector3.UnitZ, 1), };
    static readonly Vector4[] Colors = { new(1, 0, 0, 1), new(0, 1, 0, 1), new(0, 0, 1, 1), };
    protected override void OnLoad () {
        base.OnLoad();

        Recyclables.Add(va = new());
        Recyclables.Add(diy = new());
        Recyclables.Add(vertices = new(lineSegments));
        va.Assign(vertices, diy.VertexPosition);
    }

    static void Update0 (ref Vector4 p, ref Quaternion q, in Vector3 dr, in Vector3 rot) {
        q = Quaternion.Concatenate(q, Quaternion.CreateFromYawPitchRoll(rot.Y, rot.X, rot.Z));
        var dx = dr.X * Vector3.Transform(Vector3.UnitX, q);
        var dy = dr.Y * Vector3.Transform(Vector3.UnitY, q);
        var dz = dr.Z * Vector3.Transform(Vector3.UnitZ, q);
        p += new Vector4(dx + dy + dz, 0);
    }

    static void Update1 (ref Vector4 p, ref Quaternion q, in Vector3 dr, in Vector3 rot) {
        var newX = Vector3.Transform(Vector3.UnitX, q);
        q = Quaternion.Concatenate(q, Quaternion.CreateFromAxisAngle(newX, rot.X));
        var newY = Vector3.Transform(Vector3.UnitY, q);
        q = Quaternion.Concatenate(q, Quaternion.CreateFromAxisAngle(newY, rot.Y));
        var dx = dr.X * Vector3.Transform(Vector3.UnitX, q);
        var dy = dr.Y * Vector3.Transform(Vector3.UnitY, q);
        var dz = dr.Z * Vector3.Transform(Vector3.UnitZ, q);
        p += new Vector4(dx + dy + dz, 0);

    }

    Vector4 pos0 = new(0, 0, -2, 1);
    Vector4 pos1 = new(0, 0, -2, 1);
    Quaternion ori0 = Quaternion.Identity;
    Quaternion ori1 = Quaternion.Identity;
    const float AngularVelocity = Maths.fTau / 8;
    protected override void Render () {
        var size = ClientSize;

        Vector3 dr = new((float)Axis(Key.C, Key.Z), (float)Axis(Key.Q, Key.A), (float)Axis(Key.X, Key.D));
        var rot = AngularVelocity * new Vector3((float)Axis(Key.Insert, Key.Delete), (float)Axis(Key.Home, Key.End), (float)Axis(Key.PageUp, Key.PageDown));

        Update0(ref pos0, ref ori0, in dr, in rot);
        Update1(ref pos1, ref ori1, in dr, in rot);

        var translation0 = Matrix4x4.CreateTranslation(pos0.Xyz());
        var translation1 = Matrix4x4.CreateTranslation(pos1.Xyz());
        var rotation0 = Matrix4x4.CreateFromQuaternion(ori0);
        var rotation1 = Matrix4x4.CreateFromQuaternion(ori1);
        var model0 = rotation0 * translation0;
        var model1 = rotation1 * translation1;
        var view = Matrix4x4.CreateTranslation(0, 0, -2);
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(Maths.fPi / 2, (float)size.X / size.Y, 1f, 100f);

        Vector2i sectionSize = new(size.X / 2, size.Y / 2);
        Viewport(Vector2i.Zero, size);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);

        UseProgram(diy);
        BindVertexArray(va);

        //clockwise from topleft
        //Viewport(new(0, sectionSize.Y), sectionSize);

        //diy.Matrix(view * model1 * projection);
        //for (var i = 0; i < 3; ++i) {
        //    diy.Color(Colors[i]);
        //    DrawArrays(Primitive.Lines, 2 * i, 2);
        //}

        Viewport(sectionSize, sectionSize);

        diy.Matrix(model0 * view * projection);
        for (var i = 0; i < 3; ++i) {
            diy.Color(Colors[i]);
            DrawArrays(Primitive.Lines, 2 * i, 2);
        }


        Viewport(new(sectionSize.X, 0), sectionSize);

        diy.Matrix(model1 * view * projection);
        for (var i = 0; i < 3; ++i) {
            diy.Color(Colors[i]);
            DrawArrays(Primitive.Lines, 2 * i, 2);
        }


        //Viewport(Vector2i.Zero, sectionSize);

        //diy.Matrix(view * model0 * projection);
        //for (var i = 0; i < 3; ++i) {
        //    diy.Color(Colors[i]);
        //    DrawArrays(Primitive.Lines, 2 * i, 2);
        //}
    }
}
