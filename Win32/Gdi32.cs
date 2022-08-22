namespace Win32;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class Dib {
    public readonly BitmapInfo Info;
    public readonly IntPtr Bits;
    public readonly IntPtr Handle;
    public int Width =>
        Info.header.Width;

    public int Height =>
        Info.header.Height;

    public Dib (DeviceContext dc, int w, int h) {
        Info = new BitmapInfo() {
            header = new() {
                Size = Marshal.SizeOf<BitmapInfoHeader>(),
                Width = w,
                Height = h,
                Planes = 1,
                BitCount = BitCount.ColorBits32,
                Compression = BitmapCompression.Rgb,
                SizeImage = 0,
                XPelsPerMeter = 0,
                YPelsPerMeter = 0,
                ClrUsed = 0,
                ClrImportant = 0,
            }
        };
        Debug.Assert(40 == Info.header.Size);
        Bits = IntPtr.Zero;
        Handle = Gdi32.CreateDIBSection((IntPtr)dc, ref Info, 0, ref Bits, IntPtr.Zero, 0);
    }
}

public struct BitmapInfo {
    public BitmapInfoHeader header;
    public RgbQuad colors;
}

public struct RgbQuad {
    public byte Blue, Green, Red, Reserved;
}

public enum BitCount:short {
    Unspecified = 0,
    Monochrome = 1,
    Colors16 = 4,
    Colors256 = 8,
    ColorBits16 = 16,
    ColorBits24 = 24,
    ColorBits32 = 32,
}
public enum BitmapCompression {
    Rgb = 0,
    Rle8,
    Rle4,
    BitFields,
    Jpeg,
    Png,
}
public struct BitmapInfoHeader {
    /// <summary>The number of bytes required by the structure</summary>
    public int Size;
    /// <summary>The width of the bitmap, in pixels</summary>
    public int Width;
    /// <summary>The height of the bitmap, in pixels. If positive, the bitmap is a bottom-up DIB and its origin is the lower-left corner. If negative, the bitmap is a top-down DIB and its origin is the upper-left corner</summary>
    public int Height;
    /// <summary>const 1</summary>
    public short Planes;
    /// <summary> </summary>
    public BitCount BitCount;
    /// <summary> </summary>
    public BitmapCompression Compression;
    /// <summary>The size, in bytes, of the image. This may be set to zero for BI_RGB bitmaps.</summary>
    public int SizeImage;
    /// <summary> </summary>
    public int XPelsPerMeter;
    /// <summary> </summary>
    public int YPelsPerMeter;
    /// <summary> </summary>
    public int ClrUsed;
    /// <summary> </summary>
    public int ClrImportant;
}

public static class Gdi32 {
    private const string dll = nameof(Gdi32) + ".dll";

    [DllImport(dll, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    internal static extern IntPtr CreateDIBSection ([In] IntPtr hdc, ref BitmapInfo info, int usage, ref IntPtr bits, IntPtr section, uint offset);

    [DllImport(dll, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GdiFlush ();

    [DllImport(dll, CallingConvention = CallingConvention.Winapi)]
    public static extern int StretchDIBits (IntPtr dc, int xDest, int yDest, int wDest, int hDest, int xSrc, int ySrc, int wSrc, int hSrc, IntPtr bits, [In] in BitmapInfo info, int usage, int operation);

    [DllImport(dll, CallingConvention = CallingConvention.Winapi)]
    internal unsafe static extern IntPtr CreateDIBitmap (IntPtr dc, [In] ref BitmapInfoHeader header, int init, void* bits, [In] ref BitmapInfo info, int usage);

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
