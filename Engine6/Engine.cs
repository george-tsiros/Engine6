namespace Engine;

using System;

class Engine {

    [STAThread]
    static void Main () {
        var model = new Model("data/teapot.obj", true);
        using var f = new BlitTest(new(1280, 720), model) { Font = new("ubuntu mono", 15f) };
        f.Run();
    }
}
