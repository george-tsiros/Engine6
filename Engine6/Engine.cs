namespace Engine6;

using System;
using System.Reflection;
using Win32;

public class Engine {


    public static int Main (string[] arguments) {
        using var window = (Window)Assembly.GetExecutingAssembly().GetType("Engine6." + arguments[0]).GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());
        window.Run();
        return 0;
    }
}
