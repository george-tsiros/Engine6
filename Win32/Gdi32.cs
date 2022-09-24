namespace Win32;
using Common;
using System.Diagnostics;
using System.Runtime.InteropServices;
public static class Gdi32 {
    private const string dll = nameof(Gdi32) + ".dll";
    [DllImport(dll, EntryPoint = "DeleteDC", ExactSpelling = true)][return: MarshalAs(UnmanagedType.Bool)] private static extern bool DeleteDC_ (nint dc);
    [DllImport(dll, EntryPoint = "DeleteObject", ExactSpelling = true, SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] private static extern bool DeleteObject_ (nint handle);
    [DllImport(dll, SetLastError = true)] private unsafe static extern nint CreateDIBSection ([In] nint dc, BitmapInfo* info, int use, ref void* p, nint section, uint offset);
    [DllImport(dll)] private unsafe static extern int StretchDIBits (nint dc, int xT, int yT, int wT, int hT, int xS, int yS, int wS, int hS, void* p, in BitmapInfo info, int use, uint op);
    [DllImport(dll, SetLastError = true)] internal static unsafe extern int DescribePixelFormat (nint dc, int iPixelFormat, uint bytes, PixelFormatDescriptor* p);
    [DllImport(dll, EntryPoint = "GetPixelFormat", ExactSpelling = true, SetLastError = true)] private static extern int GetPixelFormat_ (nint dc);
    [DllImport(dll, EntryPoint = "SetPixelFormat", ExactSpelling = true, SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] private static extern bool SetPixelFormat_ (nint dc, int format, ref PixelFormatDescriptor pfd);
    [DllImport(dll, EntryPoint = "SwapBuffers", ExactSpelling = true, SetLastError = true)][return: MarshalAs(UnmanagedType.Bool)] private static extern bool SwapBuffers_ (nint dc);
    public static void SwapBuffers (DeviceContext dc) {
        if (!SwapBuffers_((nint)dc))
            throw new WinApiException(nameof(SwapBuffers));
    }
    public static void DeleteObject (nint handle) {
        if (!DeleteObject_(handle))
            throw new WinApiException(nameof(DeleteObject));
    }
    public static void DeleteDC (nint dc) {
        if (!DeleteDC_(dc))
            throw new WinApiException(nameof(DeleteDC));
    }
    public unsafe static int GetPixelFormatCount (DeviceContext hdc) {
        var count = DescribePixelFormat((nint)hdc, 0, 0, null);
        return 0 < count ? count : throw new WinApiException(nameof(DescribePixelFormat));
    }
    public unsafe static void DescribePixelFormat (DeviceContext hdc, int pixelFormat, ref PixelFormatDescriptor ppfd) {
        fixed (PixelFormatDescriptor* p = &ppfd)
            if (0 == DescribePixelFormat((nint)hdc, pixelFormat, PixelFormatDescriptor.Size, p))
                throw new WinApiException(nameof(DescribePixelFormat));
    }
    public unsafe static nint CreateDIBSection (DeviceContext dc, ref BitmapInfo info, ref void* bits) {
        fixed (BitmapInfo* p = &info) {
            var dib = CreateDIBSection((nint)dc, p, 0, ref bits, 0, 0);
            return 0 != dib && null != bits ? dib : throw new WinApiException(nameof(CreateDIBSection));
        }
    }
    public static int GetPixelFormat (DeviceContext dc) {
        var pf = GetPixelFormat_((nint)dc);
        return 0 != pf ? pf : throw new WinApiException(nameof(GetPixelFormat));
    }
    public static void SetPixelFormat (DeviceContext dc, int format, ref PixelFormatDescriptor pfd) {
        if (!SetPixelFormat_((nint)dc, format, ref pfd))
            throw new WinApiException(nameof(SetPixelFormat));
    }
    public unsafe static int StretchDIBits (DeviceContext dc, in Rectangle to, in Rectangle from, Dib source, RasterOperation op) =>
        StretchDIBits((nint)dc, to.Left, to.Top, to.Width, to.Height, from.Left, from.Top, from.Width, from.Height, source.Pixels, source.Info, 0, (uint)op);
}
