namespace Gl;
using System.Runtime.InteropServices;

public static class Glu {
    private const string glu32 = nameof(glu32) + ".dll";
    [DllImport(glu32, EntryPoint = "gluErrorString", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.LPStr)]
    public static extern string ErrorString (int error);
}
