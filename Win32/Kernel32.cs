namespace Win32;

using System.Runtime.InteropServices;
using System;

public static class Kernel32 {
    private const string dll = nameof(Kernel32) + ".dll";

    [DllImport(dll, SetLastError = true)]
    public extern static IntPtr GetProcAddress (IntPtr module, string name);

    [DllImport(dll, CallingConvention = CallingConvention.Winapi)]
    public extern static uint GetLastError ();

    [DllImport(dll, EntryPoint = "GetModuleHandleW", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, SetLastError = true)]
    public extern static IntPtr GetModuleHandle (string moduleName);
  
    [DllImport(dll, EntryPoint = "GetModuleHandleExW", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetModuleHandleEx (uint dwFlags, string lpModuleName, ref nint module);
}
