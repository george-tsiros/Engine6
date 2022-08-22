namespace Win32;

using System;
using System.Runtime.InteropServices;

unsafe public class Dib {
    private const int MaxBitmapDimension = 8192;

    public Dib (DeviceContext dc, int w, int h) {
        if (w < 1 || MaxBitmapDimension < w)
            throw new ArgumentOutOfRangeException(nameof(w));
        if (h < 1 || MaxBitmapDimension < h)
            throw new ArgumentOutOfRangeException(nameof(h));
        Info = new() {
            header = new() {
                size = (uint)Marshal.SizeOf<BitmapInfoHeader>(),
                width = Width = w,
                height = Height = h,
                planes = 1,
                bitCount = BitCount.ColorBits32,
                compression = BitmapCompression.Rgb,
                sizeImage = 0,
                xPelsPerMeter = 0,
                yPelsPerMeter = 0,
                colorsUsed = 0,
                colorsImportant = 0,
            }
        };
        void* pixels = null;
        Handle = Gdi32.CreateDIBSection(dc, ref Info, ref pixels);
        if (IntPtr.Zero == Handle)
            throw new WinApiException($"{nameof(Gdi32.CreateDIBSection)} failed (IntPtr.Zero == Handle)");
        if (null == pixels)
            throw new WinApiException($"{nameof(Gdi32.CreateDIBSection)} failed (IntPtr.Zero == Bits)");
        Raw = (uint*)pixels;

        // for now BitCount is fixed and equal to 32
        Stride = Width;
    }

    public readonly BitmapInfo Info;
    // AARRGGBB
    public readonly uint* Raw;
    public IntPtr Pixels => (IntPtr)Raw;
    public readonly IntPtr Handle;
    public readonly int Width;
    public readonly int Height;
    public readonly int Stride;
}
