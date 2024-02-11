namespace Engine6;

using System;
using System.Reflection;
using Win32;

public class Engine {
    public static int Main (string[] arguments) {
        using GlWindow w = new();
        w.Run();
        return 0;
    }
}
