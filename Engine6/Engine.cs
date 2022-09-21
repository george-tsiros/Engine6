namespace Engine6;

using System.Numerics;

class Program {
    static void Main () {
        //var (u3x, u3y, y3z) = (Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ);
        //var q = Quaternion.CreateFromYawPitchRoll(0, 0, 0);

        using MatrixTests window = new();
        window.Run();
    }
}