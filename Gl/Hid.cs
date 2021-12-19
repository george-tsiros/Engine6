namespace Gl;

using System;
using System.Runtime.InteropServices;

public static class Hid {
    private const string hid = nameof(Hid) + ".dll";

    [DllImport(hid, EntryPoint = "HidD_GetPreparsedData", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
    public static extern byte GetPreparsedData (IntPtr deviceObject, out IntPtr data);
    [DllImport(hid, EntryPoint = "HidD_FreePreparsedData", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, SetLastError = true)]
    public static extern byte FreePreparsedData (IntPtr data);
}
