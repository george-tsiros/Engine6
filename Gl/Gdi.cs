namespace Gl;

using System;
using System.Runtime.InteropServices;
using Win32;


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
    unsafe public static extern int DescribePixelFormat (IntPtr hdc, int pixelFormat, uint bytes, PixelFormatDescriptor* ppfd);
    /// <summary>
    /// </summary>
    /// <param name="dc"></param>
    /// <param name="format"></param>
    /// <param name="pfd"></param>
    /// <returns>If the function succeeds, the return value is TRUE.If the function fails, the return value is FALSE. To get extended error information, call <see cref="Kernel.GetLastError"/>.</returns>
    [DllImport(gdi32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetPixelFormat (IntPtr dc, int format, ref PixelFormatDescriptor pfd);
    [DllImport(gdi32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern int ChoosePixelFormat (IntPtr dc, ref PixelFormatDescriptor pfd);
    [DllImport(gdi32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SwapBuffers (IntPtr dc);
    [DllImport(gdi32, CallingConvention = CallingConvention.Winapi)]
    public static extern bool DeleteDC (IntPtr dc);

    unsafe public static IntPtr CreateContext (IntPtr dc, Predicate<PixelFormatDescriptor> isGood) {
        var pfd = PixelFormatDescriptor.Create();
        var i = Gdi.GetPixelFormat(dc, isGood, &pfd);
        Kernel.Win32Assert(Gdi.SetPixelFormat(dc, i, ref pfd));
        var rc = Opengl.CreateContext(dc);
        Kernel.Win32Assert(rc);
        return rc;
    }

   unsafe public static int GetPixelFormat (IntPtr dc, Predicate<PixelFormatDescriptor> isGood, PixelFormatDescriptor* pfd) {
        var pixelFormatIndex = 1;
        while (0 != Gdi.DescribePixelFormat(dc, pixelFormatIndex, pfd->structSize, pfd)) {
            if (isGood(*pfd))
                return pixelFormatIndex;
            pixelFormatIndex++;
        }
        throw new ContextCreationException("no pfd");
    }
}
