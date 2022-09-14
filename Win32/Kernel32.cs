namespace Win32;

using System.Runtime.InteropServices;

public static class Kernel32 {
    private const string dll = nameof(Kernel32) + ".dll";

    [DllImport(dll, EntryPoint = "GetProcAddress", ExactSpelling = true, SetLastError = true)]
    private extern static nint GetProcAddress_ (nint module, nint name);

    public static nint GetProcAddress (nint module, AnsiString name) => 
        GetProcAddress_(module, name);

    [DllImport(dll, CallingConvention = CallingConvention.Winapi)]
    public extern static uint GetLastError ();

    [DllImport(dll, EntryPoint = "GetModuleHandleW", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, SetLastError = true)]
    public extern static nint GetModuleHandle (string moduleName);

    [DllImport(dll, EntryPoint = "GetModuleHandleExW", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetModuleHandleEx (uint dwFlags, string lpModuleName, ref nint module);
}
