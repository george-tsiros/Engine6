namespace Win32;

using System.Runtime.InteropServices;

public static class Kernel32 {
    private const string dll = nameof(Kernel32) + ".dll";

    [DllImport(dll, EntryPoint = "GetProcAddress", ExactSpelling = true, SetLastError = true)]
    private extern static nint GetProcAddress_ (nint module, nint name);

    [DllImport(dll, CallingConvention = CallingConvention.Winapi)]
    public extern static uint GetLastError ();

    [DllImport(dll, EntryPoint = "GetModuleHandleA", ExactSpelling = true, SetLastError = true)]
    private extern static nint GetModuleHandle_ (nint moduleName);

    [DllImport(dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetModuleHandleExA (uint dwFlags, nint lpModuleName, ref nint module);

    public static nint GetModuleHandle (string name) {
        if (name is null)
            return GetModuleHandle_(0);
        using AnsiString n = new(name);
        return GetModuleHandle_(n);
    }

    public static bool GetModuleHandleEx (uint flags, string moduleName, ref nint module) {
        using AnsiString name = new(moduleName);
        return GetModuleHandleExA(flags, (nint)name, ref module);
    }

    public static nint GetProcAddress (nint module, string name) {
        using AnsiString n = new(name);
        return GetProcAddress_(module, n);
    }
}
