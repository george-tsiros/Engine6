namespace Engine6;

using Gl;

class Engine6 {

    static void Main () {

        ContextConfiguration c = new() {
            ColorBits = 32,
            DepthBits = 24,
            DoubleBuffer = true,
            Profile = ProfileMask.Core,
            Flags = ContextFlag.Debug | ContextFlag.ForwardCompatible,
            Version = new(4, 5),
        };


            
        using var f = new GdiWindow();
        f.Run();
    }
}
