namespace Win32;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

//struct GdiplusStartupInput { }
//struct GdiplusStartupOutput { }

//internal static class Gdiplus {
//    private const string dll = nameof(Gdiplus) + ".dll";
//    public static void Thing () { }
//    private static readonly IntPtr GdiPlusToken;
//    static Gdiplus () {
//    }

//    [DllImport(dll, CallingConvention = CallingConvention.Winapi)]
//    private static extern void GdiplusShutdown (IntPtr token);

//    [DllImport(dll, CallingConvention = CallingConvention.Winapi)]
//    private static extern int GdiplusStartup (ref IntPtr token, ref GdiplusStartupInput i, ref GdiplusStartupOutput o);
//}

public static class Gdi32 {
    private const string dll = nameof(Gdi32) + ".dll";

    [DllImport(dll, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    private unsafe static extern IntPtr CreateDIBSection ([In] IntPtr dc, BitmapInfo* info, int use, ref void* p, IntPtr section, uint offset);

    public unsafe static IntPtr CreateDIBSection (DeviceContext dc, ref BitmapInfo info, ref void* bits) {
        fixed (BitmapInfo* p = &info) {
            var dib = CreateDIBSection((IntPtr)dc, p, 0, ref bits, IntPtr.Zero, 0);
            return IntPtr.Zero != dib && null != bits ? dib : throw new WinApiException(nameof(CreateDIBSection));
        }
    }

    [DllImport(dll, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GdiFlush ();

    [DllImport(dll, CallingConvention = CallingConvention.Winapi)]
    public unsafe static extern int StretchDIBits (IntPtr dc, int xT, int yT, int wT, int hT, int xS, int yS, int wS, int hS, void* p, BitmapInfo* info, int use, int op);

    [DllImport(dll, CallingConvention = CallingConvention.Winapi)]
    internal unsafe static extern IntPtr CreateDIBitmap (IntPtr dc, [In] ref BitmapInfoHeader header, int init, void* bits, BitmapInfo* info, int usage);

    /// <summary>
    /// </summary>
    /// <param name="hdc">Specifies the device context.</param>
    /// <param name="pixelFormat">Index that specifies the pixel format. The pixel formats that a device context supports are identified by positive one-based integer indexes.</param>
    /// <param name="bytes">The size, in bytes, of the structure pointed to by ppfd. The DescribePixelFormat function stores no more than nBytes bytes of data to that structure. Set this value to sizeof(PIXELFORMATDESCRIPTOR).</param>
    /// <param name="ppfd">the function sets the members of the PIXELFORMATDESCRIPTOR structure pointed to by ppfd according to the specified pixel format.</param>
    /// <returns>If the function succeeds, the return value is the maximum pixel format index of the device context.If the function fails, the return value is zero. To get extended error information, call <see cref="Kernel32.GetLastError"/>.</returns>
    [DllImport(dll, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    private unsafe static extern int DescribePixelFormat (IntPtr hdc, int pixelFormat, int bytes, PixelFormatDescriptor* ppfd);

    public unsafe static int GetPixelFormatCount (DeviceContext hdc) {
        var count = DescribePixelFormat((IntPtr)hdc, 0, PixelFormatDescriptor.Size, null);
        return 0 < count ? count : throw new WinApiException(nameof(DescribePixelFormat));
    }

    public unsafe static void DescribePixelFormat (DeviceContext hdc, int pixelFormat, ref PixelFormatDescriptor ppfd) {
        fixed (PixelFormatDescriptor* p = &ppfd)
            if (0 == DescribePixelFormat((IntPtr)hdc, pixelFormat, PixelFormatDescriptor.Size, p))
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

    public static void SetPixelFormat (DeviceContext dc, int format, ref PixelFormatDescriptor pfd) {
        if (!SetPixelFormat_((IntPtr)dc, format, ref pfd))
            throw new WinApiException(nameof(SetPixelFormat));
    }

    [DllImport(dll, EntryPoint = "SwapBuffers", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SwapBuffers_ (IntPtr dc);

    public static void SwapBuffers (IntPtr dc) {
        if (!SwapBuffers_(dc))
            throw new WinApiException(nameof(SwapBuffers));
    }

    [DllImport(dll, EntryPoint = "DeleteObject", CallingConvention = CallingConvention.Winapi, ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject_ (IntPtr handle);

    public static void DeleteObject (IntPtr handle) {
        if (!DeleteObject_(handle))
            throw new WinApiException(nameof(DeleteObject));
    }


    [DllImport(dll, EntryPoint = "DeleteDC", ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteDC_ (IntPtr dc);

    public static void DeleteDC (IntPtr dc) {
        if (!DeleteDC_(dc))
            throw new WinApiException(nameof(DeleteDC));
    }
}
