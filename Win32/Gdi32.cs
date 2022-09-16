namespace Win32;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

//struct GdiplusStartupInput { }
//struct GdiplusStartupOutput { }

//internal static class Gdiplus {
//    private const string dll = nameof(Gdiplus) + ".dll";
//    public static void Thing () { }
//    private static readonly nint GdiPlusToken;
//    static Gdiplus () {
//    }

//    [DllImport(dll, CallingConvention = CallingConvention.Winapi)]
//    private static extern void GdiplusShutdown (nint token);

//    [DllImport(dll, CallingConvention = CallingConvention.Winapi)]
//    private static extern int GdiplusStartup (ref nint token, ref GdiplusStartupInput i, ref GdiplusStartupOutput o);
//}

public static class Gdi32 {
    private const string dll = nameof(Gdi32) + ".dll";

    [DllImport(dll, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    private unsafe static extern nint CreateDIBSection ([In] nint dc, BitmapInfo* info, int use, ref void* p, nint section, uint offset);

    public unsafe static nint CreateDIBSection (DeviceContext dc, ref BitmapInfo info, ref void* bits) {
        fixed (BitmapInfo* p = &info) {
            var dib = CreateDIBSection((nint)dc, p, 0, ref bits, nint.Zero, 0);
            return nint.Zero != dib && null != bits ? dib : throw new WinApiException(nameof(CreateDIBSection));
        }
    }

    [DllImport(dll, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GdiFlush ();

    [DllImport(dll, CallingConvention = CallingConvention.Winapi)]
    public unsafe static extern int StretchDIBits (nint dc, int xT, int yT, int wT, int hT, int xS, int yS, int wS, int hS, void* p, in BitmapInfo info, int use, int op);

    //[DllImport(dll, CallingConvention = CallingConvention.Winapi)]
    //public unsafe static extern int SetDIBits (nint dc, nint bitmapHandle, uint start, uint count, void* p, in BitmapInfo info, int use);

    [DllImport(dll, CallingConvention = CallingConvention.Winapi)]
    internal unsafe static extern nint CreateDIBitmap (nint dc, [In] ref BitmapInfoHeader header, int init, void* bits, BitmapInfo* info, int usage);

    /// <summary>
    /// </summary>
    /// <param name="hdc">Specifies the device context.</param>
    /// <param name="pixelFormat">Index that specifies the pixel format. The pixel formats that a device context supports are identified by positive one-based integer indexes.</param>
    /// <param name="bytes">The size, in bytes, of the structure pointed to by ppfd. The DescribePixelFormat function stores no more than nBytes bytes of data to that structure. Set this value to sizeof(PIXELFORMATDESCRIPTOR).</param>
    /// <param name="ppfd">the function sets the members of the PIXELFORMATDESCRIPTOR structure pointed to by ppfd according to the specified pixel format.</param>
    /// <returns>If the function succeeds, the return value is the maximum pixel format index of the device context.If the function fails, the return value is zero. To get extended error information, call <see cref="Kernel32.GetLastError"/>.</returns>
    [DllImport(dll, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    internal static unsafe extern int DescribePixelFormat (nint dc, int iPixelFormat, uint bytes, PixelFormatDescriptor* p);

    public unsafe static int GetPixelFormatCount (DeviceContext hdc) {
        var count = DescribePixelFormat((nint)hdc, 0, 0, null);
        return 0 < count ? count : throw new WinApiException(nameof(DescribePixelFormat));
    }

    public unsafe static void DescribePixelFormat (DeviceContext hdc, int pixelFormat, ref PixelFormatDescriptor ppfd) {
        fixed (PixelFormatDescriptor* p = &ppfd)
            if (0 == DescribePixelFormat((nint)hdc, pixelFormat, PixelFormatDescriptor.Size, p))
                throw new WinApiException(nameof(DescribePixelFormat));
    }

    [DllImport(dll, EntryPoint = "GetPixelFormat", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    private static extern int GetPixelFormat_ (nint dc);

    public static int GetPixelFormat (DeviceContext dc) {
        var pf = GetPixelFormat_((nint)dc);
        return 0 != pf ? pf : throw new WinApiException(nameof(GetPixelFormat));
    }


    /// <summary>
    /// </summary>
    /// <param name="dc"></param>
    /// <param name="format"></param>
    /// <param name="pfd"></param>
    /// <returns>If the function succeeds, the return value is TRUE.If the function fails, the return value is FALSE. To get extended error information, call <see cref="Kernel32.GetLastError"/>.</returns>
    [DllImport(dll, EntryPoint = "SetPixelFormat", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetPixelFormat_ (nint dc, int format, ref PixelFormatDescriptor pfd);

    public static void SetPixelFormat (DeviceContext dc, int format, ref PixelFormatDescriptor pfd) {
        if (!SetPixelFormat_((nint)dc, format, ref pfd))
            throw new WinApiException(nameof(SetPixelFormat));
    }

    [DllImport(dll, EntryPoint = "SwapBuffers", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SwapBuffers_ (nint dc);

    public static void SwapBuffers (DeviceContext dc) {
        if (!SwapBuffers_((nint)dc))
            throw new WinApiException(nameof(SwapBuffers));
    }

    [DllImport(dll, EntryPoint = "DeleteObject", CallingConvention = CallingConvention.Winapi, ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject_ (nint handle);

    public static void DeleteObject (nint handle) {
        if (!DeleteObject_(handle))
            throw new WinApiException(nameof(DeleteObject));
    }


    [DllImport(dll, EntryPoint = "DeleteDC", ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteDC_ (nint dc);

    public static void DeleteDC (nint dc) {
        if (!DeleteDC_(dc))
            throw new WinApiException(nameof(DeleteDC));
    }
}
