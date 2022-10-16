namespace Win32;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using System.Text;
using Common;

public static class Kernel32 {
    private const string dll = nameof(Kernel32) + ".dll";

    [DllImport(dll, EntryPoint = "GetProcAddress", ExactSpelling = true, SetLastError = true)]
    private extern static nint GetProcAddress_ (nint module, nint name);

    [DllImport(dll)]
    public extern static uint GetLastError ();

    [DllImport(dll, EntryPoint = "GetModuleHandleA", ExactSpelling = true, SetLastError = true)]
    private extern static nint GetModuleHandle_ (nint moduleName);

    [DllImport(dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetModuleHandleExA (uint dwFlags, nint lpModuleName, ref nint module);

    //[DllImport(dll)]
    //private unsafe extern static void OutputDebugStringA (byte* p);

    //public unsafe static void OutputDebugString (Ascii ascii) =>
    //    OutputDebugStringA((byte*)ascii.Handle);

    public static nint GetModuleHandle (string name = null) {
        if (name is null)
            return GetModuleHandle_(0);
        using Ascii n = new(name);
        return GetModuleHandle_(n);
    }

    public static void GetModuleHandleEx (uint flags, string moduleName, out nint module) {
        module = 0;
        using Ascii handle = new(moduleName);
        if (!GetModuleHandleExA(flags, handle, ref module))
            throw new WinApiException(nameof(GetModuleHandleEx));
    }

    //not performance critical
    public unsafe static nint GetProcAddress (nint module, string name) {
        using Ascii handle = new(name);
        return GetProcAddress_(module, handle);
    }
}
