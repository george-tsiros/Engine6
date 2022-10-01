namespace Engine6;
using System;
using System.Numerics;
class Engine {

    static void Q4 () { 
        Console.WriteLine("quatern vector4");
        Vector4 forward= new(0, 0, -1, 1);
        var qpitch = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)(Math.PI / 2));
        var fw1 = Vector4.Transform(forward, qpitch);
        Console.WriteLine(fw1);
    }

    static void Q () {
        Console.WriteLine("quatern");
        var forward = -Vector3.UnitZ;
        var qpitch = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)(Math.PI / 2));
        var fw1 = Vector3.Transform(forward, qpitch);
        Console.WriteLine(fw1);
    }

    static void M () {
        Console.WriteLine("matr");
        var forward = -Vector3.UnitZ;
        var mpitch = Matrix4x4.CreateFromAxisAngle(Vector3.UnitX, (float)(Math.PI / 2));
        var fw1 = Vector3.Transform(forward, mpitch);
        Console.WriteLine(fw1);
        var mroll = Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, (float)(Math.PI / 2));
        var fw2 = Vector3.Transform(fw1, mroll);
        Console.WriteLine(fw2);
    }

    static void Main () {
        //Q();
        //Q4();
        //M();


        using MatrixTests window = new();
        window.Run();

        //_ = Console.ReadLine();
    }
}