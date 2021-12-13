namespace Engine;

using System;
using Gl;

class Engine {
    [STAThread]
    static void Main (string[] args) {
        var size = args.Length == 2 && Array.TrueForAll(args, x => int.TryParse(x, out _)) ? new Vector2i(int.Parse(args[0]), int.Parse(args[1])) : new Vector2i(320, 240);
        using var gl = new DepthFuncTest(size);
        gl.Run();
    }
}
