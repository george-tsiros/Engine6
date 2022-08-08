namespace Engine;

using System;

class Engine {

    [STAThread]
    static void Main () {
        var model = new Model("data/teapot.obj",true);
        using var f = new BlitTest(new(1280, 720), model) { Font = new("data\\IBM_3270.txt") };
        //using var f = new HighlightTriangle(new(1280, 720), model );
        //using var f = new NoiseTest(new(512, 512));
        f.Run();
    }
}
