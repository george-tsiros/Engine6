namespace CsFwExample;

using System.Runtime.InteropServices;

internal static class NativeMethods {
    [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi)]
    unsafe internal static extern int RegisterRawInputDevices (RawInputDevice* device, uint count, uint size);
}
