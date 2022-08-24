namespace Win32;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Channels;

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
        pixelCount = Width * Height;
    }

    public readonly BitmapInfo Info;
    // AARRGGBB
    public readonly uint* Raw;
    public IntPtr Pixels => (IntPtr)Raw;
    public readonly IntPtr Handle;
    public readonly int Width;
    public readonly int Height;
    public readonly int Stride;
    readonly int pixelCount;

    /// <summary><paramref name="y"/> y=0 is top of screen</summary>
    public void DrawString (ReadOnlySpan<char> str, Font font, int x, int y, uint color = ~0u) {
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
                Raw[offset + column] = font.Pixels[source + column] != 0 ? color : 0xff000000u;
            }
        }
    }
    public void ClearU32 (Color color) {
        ClearU32Internal(color.Argb);
    }
    unsafe private void ClearU32Internal (uint color) {
        Debug.Assert(((nint)Raw & 1l) == 0);
        var ulongCount = pixelCount >> 1;
        var p = (ulong*)Raw;
        var ul = ((ulong)color << 32) | color;
        for (var i = 0; i < ulongCount; ++i)
            p[i] = ul;
        if ((pixelCount & 1) != 0)
            Raw[pixelCount - 1] = color;
    }
}
