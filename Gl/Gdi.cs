namespace Gl;

using System;
using System.Runtime.InteropServices;


public static class Gdi {
    private const string gdi32 = nameof(gdi32) + ".dll";

    /// <summary>
    /// </summary>
    /// <param name="hdc">Specifies the device context.</param>
    /// <param name="pixelFormat">Index that specifies the pixel format. The pixel formats that a device context supports are identified by positive one-based integer indexes.</param>
    /// <param name="bytes">The size, in bytes, of the structure pointed to by ppfd. The DescribePixelFormat function stores no more than nBytes bytes of data to that structure. Set this value to sizeof(PIXELFORMATDESCRIPTOR).</param>
    /// <param name="ppfd">the function sets the members of the PIXELFORMATDESCRIPTOR structure pointed to by ppfd according to the specified pixel format.</param>
    /// <returns>If the function succeeds, the return value is the maximum pixel format index of the device context.If the function fails, the return value is zero. To get extended error information, call <see cref="Kernel.GetLastError"/>.</returns>
    [DllImport(gdi32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern int DescribePixelFormat (IntPtr hdc, int pixelFormat, uint bytes, ref PixelFormatDescriptor ppfd);
    /// <summary>
    /// </summary>
    /// <param name="dc"></param>
    /// <param name="format"></param>
    /// <param name="pfd"></param>
    /// <returns>If the function succeeds, the return value is TRUE.If the function fails, the return value is FALSE. To get extended error information, call <see cref="Kernel.GetLastError"/>.</returns>
    [DllImport(gdi32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern bool SetPixelFormat (IntPtr dc, int format, ref PixelFormatDescriptor pfd);
    [DllImport(gdi32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern int ChoosePixelFormat (IntPtr dc, ref PixelFormatDescriptor pfd);
    [DllImport(gdi32, CallingConvention = CallingConvention.Winapi)]
    public static extern bool SwapBuffers (IntPtr dc);
    [DllImport(gdi32, CallingConvention = CallingConvention.Winapi)]
    public static extern bool DeleteDC (IntPtr dc);

}
