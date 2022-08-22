namespace Win32;

using System.Runtime.InteropServices;
using System;

public static class Kernel32 {
    private const string dll = nameof(Kernel32) + ".dll";
    
    [DllImport(dll, CallingConvention = CallingConvention.Winapi)]
    public extern static uint GetLastError ();

    [DllImport(dll, SetLastError = true, CallingConvention = CallingConvention.Winapi)]
    public extern static IntPtr GetModuleHandleW ([In, MarshalAs(UnmanagedType.LPWStr)] string moduleName);
}
