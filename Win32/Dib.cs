namespace Win32;

using Common;
using System;
using System.Runtime.InteropServices;

unsafe public class Dib:IDisposable {

    private const int MaxBitmapDimension = 8192;

    public Dib (DeviceContext dc, Vector2i size) {
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
        if (IntPtr.Zero == Handle)
            throw new WinApiException($"{nameof(Gdi32.CreateDIBSection)} failed (IntPtr.Zero == Handle)");
        if (null == pixels)
            throw new WinApiException($"{nameof(Gdi32.CreateDIBSection)} failed (IntPtr.Zero == Bits)");
        raw = (uint*)pixels;

        Stride = Width;
        pixelCount = Width * Height;
    }

    BitmapInfo info;
    public BitmapInfo Info => info;
    // AARRGGBB
    private readonly uint* raw;
    public uint* Pixels => !disposed ? raw : throw new ObjectDisposedException(nameof(Dib));
    public readonly IntPtr Handle;
    public readonly int Width;
    public readonly int Height;
    public readonly int Stride;
    readonly int pixelCount;

    /// <summary><paramref name="y"/> y=0 is top of screen</summary>
    public void DrawString (ReadOnlySpan<char> str, Font font, int x, int y, uint color = ~0u) {
        if (disposed)
            throw new ObjectDisposedException(nameof(Dib));
        var textWidth = font.WidthOf(str);
        if (x < 0 || Width <= x + textWidth)
            return;
        if (y < 0 || Height <= y + font.Height)
            return;
        // bottom row starts at i = 0
        // second row (from bottom) starts at i = width
        // top row starts at Width * (Height - 1)
        // row (from bottom)    | y(from top)   | offset (as above)
        // 0                    | Height - 1    | 0
        // 1                    | Height - 2    | Width
        // Height - 1           | 0             | (Height - 1) * Width
        //                      | y             | (Height - 1) * Width - y * Width = (Height - y - 1) * Width
        foreach (var c in str) {
            if (Width <= x)
                return;
            var charWidth = font.Width[c];
            Blit(c, font, x, y, charWidth, color);
            x += charWidth;
        }
    }

    private unsafe void Blit (char ascii, Font font, int x, int y, int charWidth, uint color) {
        var rowStart = (Height - y - 1) * Width;
        var source = font.Offset[ascii];
        var offset = rowStart + x;
        for (var row = 0; row < font.Height; ++row, offset -= Width, source += charWidth) {
            var xpos = x;
            for (var column = 0; xpos < Width && column < charWidth; ++column, ++xpos) {
                raw[offset + column] = font.Pixels[source + column] != 0 ? color : 0xff000000u;
            }
        }
    }

    public void ClearU32 (Color color) {
        if (disposed)
            throw new ObjectDisposedException(nameof(Dib));
        ClearU32Internal(color.Argb);
    }

    unsafe private void ClearU32Internal (uint color) {
        var ulongCount = pixelCount >> 1;
        var p = (ulong*)raw;
        var ul = ((ulong)color << 32) | color;
        for (var i = 0; i < ulongCount; ++i)
            p[i] = ul;
        if ((pixelCount & 1) != 0)
            raw[pixelCount - 1] = color;
    }

    public void FillRectU32 (Rectangle r, Color color) {
        if (disposed)
            throw new ObjectDisposedException(nameof(Dib));
        var clipped = r.Clip(new(Vector2i.Zero, new(Width, Height)));
        if (clipped.Width <= 0 || clipped.Height <= 0)
            return;
        FillRectU32Internal(clipped, color.Argb);
    }

    unsafe private void FillRectU32Internal (Rectangle clipped, uint color) {
        var y = clipped.Top;
        var h = clipped.Height;
        var w = clipped.Width;
        var offset = (Height - y - 1) * Width + clipped.Left;

        while (--h >= 0) {
            var x = offset;
            for (var i = 0; i < w; ++i)
                raw[x++] = color;
            offset -= Width;
        }
    }
    bool disposed;
    public void Dispose (bool dispose) {
        if (disposed)
            return;
        if (dispose) {
            Gdi32.DeleteObject(Handle);
            disposed = true;
        }
    }

    public void Dispose () {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    //private static bool IsTopLeft (in Vector2i a, in Vector2i b) =>
    //    a.Y == b.Y && b.X < a.X || b.Y < a.Y;

    //private static int Orient2D (in Vector2i a, in Vector2i b, in Vector2i c) => (b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y);
}
