namespace Engine;

using System;
using System.IO;

class Engine {

    [STAThread]
    static void Main () {

        using var f = new NoiseTest(Gl.PixelFormatDescriptor.Typical, 512, 512);
        f.Run();
    }
}
