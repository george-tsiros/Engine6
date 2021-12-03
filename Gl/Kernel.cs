namespace Gl;

using System.Runtime.InteropServices;
using System;

internal static class Kernel {
    private const string kernel32 = nameof(kernel32) + ".dll";
    [DllImport(kernel32, CallingConvention = CallingConvention.Winapi)]
    internal extern static ulong GetLastError ();
    [DllImport(kernel32, CallingConvention = CallingConvention.Winapi)]
    internal extern static IntPtr GetProcAddress(IntPtr module, [MarshalAs(UnmanagedType.LPWStr)] string name);
    [DllImport(kernel32)]
    internal extern static IntPtr LoadLibraryW ([MarshalAs(UnmanagedType.LPWStr)] string filename);
}
