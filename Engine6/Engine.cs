namespace Engine6;

using System;

class Engine6 {

    static void Main () {
        //var model = Model.Sphere(100, 50, 6.371e6);
        using var f = new MovementTest();
        f.Run();
    }
}

