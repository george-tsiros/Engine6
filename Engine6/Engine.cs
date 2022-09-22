namespace Engine6;

using static Common.Maths;
using System.Numerics;
using System.Diagnostics;
using System;

class Program {
    static void Main () {
        //const float d2r = fTau/360f;
        //for (var i = 0; i < 360; i+=4) {
        //    var theta = i * d2r;
        //    var m = Matrix4x4.CreateRotationY(theta);
        //    var v = Vector4.Transform(Vector4.UnitX, m);
        //    Console.WriteLine(v);
        //}

        //if (Debugger.IsAttached)
        //    _ = Console.ReadLine();
        using CubeTest window = new();
        window.Run();
    }
}