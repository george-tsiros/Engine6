namespace Engine;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Win32;
using Gl;
class Engine {

    [STAThread]
    static void Main (string[] args) {
        var size = args.Length == 2 && int.TryParse(args[0], out var width) && int.TryParse(args[1], out var height) && 0 < width && width < 16536 && 0 < height && height < 16536 ? new Vector2i(width, height) : new Vector2i(640, 480);
        using var eh = new TextureTest(size);
        eh.Run();
    }
}
