namespace Engine6;

using System;

class Engine6 {

    static void Main () {
        var model = Model.Sphere(100, 50, 1);

        using (var f = new Win32.GdiWindow())
            f.Run();
    }
}

