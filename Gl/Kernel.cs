namespace Gl;

using System.Runtime.InteropServices;
using System;

public static class Kernel {
    private const string kernel32 = nameof(kernel32) + ".dll";
    [DllImport(kernel32, CallingConvention = CallingConvention.Winapi)]
    public extern static ulong GetLastError ();
    [DllImport(kernel32, SetLastError = true, CallingConvention = CallingConvention.Winapi)]
    public extern static IntPtr GetProcAddress (IntPtr module, [MarshalAs(UnmanagedType.LPStr)] string name);
    [DllImport(kernel32, SetLastError = true, CallingConvention = CallingConvention.Winapi)]
    public extern static IntPtr LoadLibraryW (string filename);
    [DllImport(kernel32, SetLastError = true, CallingConvention = CallingConvention.Winapi)]
    public extern static IntPtr LoadLibraryA (string filename);
    [DllImport(kernel32, SetLastError = true, CallingConvention = CallingConvention.Winapi)]
    public extern static IntPtr GetModuleHandleW (string moduleName);

    public static void Win32Assert (IntPtr p) => Win32Assert(p != IntPtr.Zero);
    public static void Win32Assert (bool b) {
        if (!b) {
            var message = GetLastError().ToString("x16");
            Utilities.Trace(message);
            //_ = User.MessageBox(IntPtr.Zero, message, null, 0x1000);
            Environment.Exit(-1);
        }
    }
}
