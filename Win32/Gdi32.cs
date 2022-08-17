namespace Win32;

using System;
using System.Runtime.InteropServices;

public static class Gdi32 {
    private const string dll = nameof(Gdi32) + ".dll";

    /// <summary>
    /// </summary>
    /// <param name="hdc">Specifies the device context.</param>
    /// <param name="pixelFormat">Index that specifies the pixel format. The pixel formats that a device context supports are identified by positive one-based integer indexes.</param>
    /// <param name="bytes">The size, in bytes, of the structure pointed to by ppfd. The DescribePixelFormat function stores no more than nBytes bytes of data to that structure. Set this value to sizeof(PIXELFORMATDESCRIPTOR).</param>
    /// <param name="ppfd">the function sets the members of the PIXELFORMATDESCRIPTOR structure pointed to by ppfd according to the specified pixel format.</param>
    /// <returns>If the function succeeds, the return value is the maximum pixel format index of the device context.If the function fails, the return value is zero. To get extended error information, call <see cref="Kernel32.GetLastError"/>.</returns>
    [DllImport(dll, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    private unsafe static extern int DescribePixelFormat (IntPtr hdc, int pixelFormat, int bytes, PixelFormatDescriptor* ppfd);

    public unsafe static int GetPixelFormatCount (IntPtr hdc) {
        var count = DescribePixelFormat(hdc, 0, PixelFormatDescriptor.Size, null);
        return count > 0 ? count : throw new WinApiException(nameof(DescribePixelFormat));
    }
    public unsafe static void DescribePixelFormat (IntPtr hdc, int pixelFormat, ref PixelFormatDescriptor ppfd) {
        fixed (PixelFormatDescriptor* p = &ppfd)
            if (0 == DescribePixelFormat(hdc, pixelFormat, PixelFormatDescriptor.Size, p))
                throw new WinApiException(nameof(DescribePixelFormat));
    }
    /// <summary>
    /// </summary>
    /// <param name="dc"></param>
    /// <param name="format"></param>
    /// <param name="pfd"></param>
    /// <returns>If the function succeeds, the return value is TRUE.If the function fails, the return value is FALSE. To get extended error information, call <see cref="Kernel32.GetLastError"/>.</returns>
    [DllImport(dll, EntryPoint = "SetPixelFormat", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetPixelFormat_ (IntPtr dc, int format, ref PixelFormatDescriptor pfd);

    public static void SetPixelFormat (IntPtr dc, int format, ref PixelFormatDescriptor pfd) {
        if (!SetPixelFormat_(dc, format, ref pfd))
            throw new WinApiException(nameof(SetPixelFormat));
    }

    [DllImport(dll, EntryPoint = "SwapBuffers", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SwapBuffers_ (IntPtr dc);

    public static void SwapBuffers (IntPtr dc) {
        if (!SwapBuffers_(dc))
            throw new WinApiException(nameof(SwapBuffers));
    }

    [DllImport(dll, EntryPoint = "DeleteDC", ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteDC_ (IntPtr dc);

    public static void DeleteDC (IntPtr dc) {
        if (!DeleteDC_(dc))
            throw new WinApiException(nameof(DeleteDC));
    }
}
