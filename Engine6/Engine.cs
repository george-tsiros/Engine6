namespace Engine;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

class Engine {
    [STAThread]
    static void Main () {
        using var f = new BlitTest(Gl.PixelFormatDescriptor.Typical, 1024, 1024);
        f.Run();
    }
}