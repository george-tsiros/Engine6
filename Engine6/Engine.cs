namespace Engine6;

using System;
using System.Reflection;
using System.Diagnostics;

public class Engine {
    public static int Main (string[] arguments) {
        using Foo w = new();
        w.Run();
        return 0;
    }
}
