namespace Gl;
using System.Runtime.InteropServices;

public static class Glu {
    private const string GetErrorStringFailed = "";
    private const string glu32 = nameof(glu32) + ".dll";

    [DllImport(glu32)]
    private static extern nint gluErrorString (int error);

    public static string GetErrorString (GlErrorCode code) {
        var p = gluErrorString((int)code);
        if (0 == p)
            return GetErrorStringFailed;
        return Marshal.PtrToStringAnsi(p);
    }
}
