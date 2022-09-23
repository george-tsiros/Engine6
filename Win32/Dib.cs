namespace Win32;

using Common;
using System;
using System.Runtime.InteropServices;

unsafe sealed public class Dib:IDisposable {

    public Dib (DeviceContext dc, in Vector2i size) {
        var (w, h) = size;
        if (w < 1 || MaxBitmapDimension < w)
            throw new ArgumentOutOfRangeException(nameof(w));
        if (h < 1 || MaxBitmapDimension < h)
            throw new ArgumentOutOfRangeException(nameof(h));
        info = new() {
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
        Handle = Gdi32.CreateDIBSection(dc, ref info, ref pixels);
        if (0 == Handle)
            throw new WinApiException($"{nameof(Gdi32.CreateDIBSection)} failed (0 == Handle)");
        if (null == pixels)
            throw new WinApiException($"{nameof(Gdi32.CreateDIBSection)} failed (0 == Bits)");
        raw = (uint*)pixels;

        Stride = Width;
        pixelCount = Width * Height;
    }

    public readonly nint Handle;
    public readonly int Width;
    public readonly int Height;
    public readonly int Stride;

    public BitmapInfo Info =>
        info;

    public Vector2i Size =>
        new(Width, Height);

    /// <summary>AARRGGBB</summary>
    public uint* Pixels =>
        !disposed ? raw : throw new ObjectDisposedException(nameof(Dib));


    public void ClearU32 (in Color color) {
        NotDisposed();
        ClearU32Internal(color.Argb);
    }

    private const int MaxBitmapDimension = 8192;

    private BitmapInfo info;
    private readonly uint* raw;
    private readonly int pixelCount;
    private bool disposed;

    private unsafe void ClearU32Internal (uint color) {
        var ulongCount = pixelCount >> 1;
        var p = (ulong*)raw;
        var ul = ((ulong)color << 32) | color;
        for (var i = 0; i < ulongCount; ++i)
            p[i] = ul;
        if ((pixelCount & 1) != 0)
            raw[pixelCount - 1] = color;
    }

    private void NotDisposed () {
        if (disposed)
            throw new ObjectDisposedException(nameof(Dib));
    }

    public void Dispose () {
        if (!disposed) {
            Gdi32.DeleteObject(Handle);
            disposed = true;
        }
    }
}
