using System.Diagnostics;
using System.Runtime.InteropServices;
using Win32;

namespace Engine6;

class Engine6 {
    static void Main () {
        //new BlitTest(new("data/teapot.obj", true),new(1280,720)).Run();
        new GdiWindow(new(1280,720)).Run();
    }
}

