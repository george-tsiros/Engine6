namespace Engine;

using System;

class Engine {
    [STAThread]
    static void Main () {
        using var bt = new BlitTest(new(1280, 720), Model.Cube(1, 1, 1)) { Font = new("data\\IBM_3270.txt") };
        bt.Run();
    }
}
