namespace Engine;

using System;

class Engine {

    [STAThread]
    static void Main () {
        var model = Model.Cube(20, 20, 20);// new Model("data/teapot.obj", true);
        Model.InvertFaces(model);
        using var f = new BlitTest(new(1280, 720), model) { Font = new("ubuntu mono", 15f) };
        f.Run();
    }
}
