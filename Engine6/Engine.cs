namespace Engine;

using Linear;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using static Linear.Maths;
class Engine {

    [STAThread]
    static void Main () {
        //var model =  new Model("data/teapot.obj", true);
        using var f = new BlitTest(new(1280, 720), Model.Sphere(50, 25, 1));
        f.Run();
    }
}
