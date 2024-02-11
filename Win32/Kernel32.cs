namespace Win32;

using System.Runtime.InteropServices;
using Common;

public static partial class Kernel32 {
    private const string dll = nameof(Kernel32) + ".dll";

    [DllImport(dll, EntryPoint = "GetProcAddress", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private extern static nint GetProcAddress_ (nint module, string name);

    [DllImport(dll)]
    public extern static uint GetLastError ();

    [DllImport(dll, EntryPoint = "GetModuleHandleA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
    private extern static nint GetModuleHandle_ (string moduleName);

    [DllImport(dll, EntryPoint = "GetModuleHandleExA", CharSet = CharSet.Ansi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetModuleHandleEx_ (uint dwFlags, string lpModuleName, ref nint module);

    public static nint GetModuleHandle (string name) {
        return GetModuleHandle_(name);
    }

    public static nint GetModuleHandle () => GetModuleHandle_(null);

    public static void GetModuleHandleEx (uint flags, string moduleName, out nint module) {
        module = 0;
        if (!GetModuleHandleEx_(flags, moduleName, ref module))
            throw new WinApiException(nameof(GetModuleHandleEx));
    }

    //not performance critical
    public unsafe static nint GetProcAddress (nint module, string name) {
        return GetProcAddress_(module, name);
    }
}
