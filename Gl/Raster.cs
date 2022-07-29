namespace Gl;

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Win32;

public class Raster:IDisposable {
    public readonly Vector2i Size;
    public int Width => Size.X;
    public int Height => Size.Y;
    public readonly int Channels, BytesPerChannel, Stride;

    public byte[] Pixels;

    public Raster (Vector2i size, int channels, int bytesPerChannel) {
        if (size.X < 1 || size.Y < 1)
            throw new ArgumentException("invalid value", nameof(size));
        if (channels < 1 || 8 < channels)
            throw new ArgumentException("invalid value", nameof(channels));
        if (bytesPerChannel < 1 || 8 < bytesPerChannel)
            throw new ArgumentException("invalid value", nameof(bytesPerChannel));

        (Size, Channels, BytesPerChannel) = (size, channels, bytesPerChannel);
        Pixels = new byte[Width * Height * Channels * BytesPerChannel];
        Stride = Width * Channels * BytesPerChannel;
    }

    public void FillRectU32 (Rectangle r, uint color = ~0u) {
        if (Channels != 4)
            throw new InvalidOperationException($"{nameof(FillRectU32)} only works with 4 channels, not {Channels}");
        var clipped = r.Clip(new(Vector2i.Zero, Size));
        if (clipped.Width <= 0 || clipped.Height <= 0)
            return;
        FillRectU32Internal(clipped, color);
    }

    unsafe private void FillRectU32Internal (Rectangle clipped, uint color) {
        var y = clipped.Top;
        var h = clipped.Height;
        var w = clipped.Width;
        var offset = (Height - y - 1) * Width + clipped.Left;

        fixed (byte* bp = Pixels) {
            uint* p = (uint*)bp;
            while (--h >= 0) {
                var x = offset;
                for (var i = 0; i < w; ++i)
                    p[x++] = color;
                offset -= Width;
            }
        }
    }
    public void HorizontalU32 (int x, int y, int length, uint color = ~0u) {
        if (Channels != 4)
            throw new InvalidOperationException($"{nameof(HorizontalU32)} only works with 4 channels, not {Channels}");
        if (y < 0 || Height <= y)
            return;
        if (length <= 0)
            return;
        var from = int.Clamp(x, 0, Width);
        var to = int.Clamp(x + length, 0, Width);
        if (to <= from)
            return;
        HorizontalU32Internal(from, to, y, color);
    }
    unsafe void HorizontalU32Internal (int x0, int x1, int y, uint color) {
        fixed (byte* bytes = Pixels) {
            uint* p = (uint*)bytes;
            var line = (Height - y - 1) * Width;
            var start = line + x0;
            var end = line + x1;
            while (start < end)
                p[start++] = color;
        }
    }

    public void VerticalU32 (int x, int y, int height, uint color = ~0u) {
        if (Channels != 4)
            throw new InvalidOperationException($"{nameof(VerticalU32)} only works with 4 channels, not {Channels}");
        if (x < 0 || Width <= x)
            return;
        if (height <= 0)
            return;
        var from = int.Clamp(y, 0, Height);
        var to = int.Clamp(y + height, 0, Height);
        VerticalU32Internal(from, to, x, color);
    }

    unsafe void VerticalU32Internal (int y0, int y1, int x, uint color) {
        fixed (byte* bytes = Pixels) {
            uint* p = (uint*)bytes;
            var start = (Height - y0 - 1) * Width + x;
            while (y0 < y1) {
                p[start] = color;
                ++y0;
                start -= Width;
            }
        }
    }
    private static Raster FromStream (FileStream f) {
        using var r = new BinaryReader(f, System.Text.Encoding.UTF8, true);
        var width = r.ReadInt32();
        var height = r.ReadInt32();
        var channels = r.ReadInt32();
        var bytesPerChannel = r.ReadInt32();
        if (bytesPerChannel != 1)
            throw new ArgumentOutOfRangeException(nameof(bytesPerChannel), "only 1Bpp bitmaps are currently supported");
        var length = r.ReadInt32();
        return new Raster(new(width, height), channels, bytesPerChannel);
    }

    public static Raster FromFile (string filepath) {
        using var f = File.OpenRead(filepath);
        var raster = FromStream(f);
        using var unzip = new DeflateStream(f, CompressionMode.Decompress);
        var read = unzip.Read(raster.Pixels, 0, raster.Pixels.Length);
        return read == raster.Pixels.Length ? raster : throw new ApplicationException($"{filepath}: expected to read {raster.Pixels.Length} bytes, read {read} instead");
    }

    private bool disposed;
    protected virtual void Dispose (bool _) {
        if (!disposed) {
            Pixels = null;
            disposed = true;
        }
    }

    /// <summary><paramref name="y"/> top down</summary>
    public void DrawString (string str, Font font, int x, int y, uint color = ~0u) {
        var textLength = font.WidthOf(str);
        if (x < 0 || Width < x + textLength)
            return;
        if (y < 0 || Height < y + font.Height)
            return;
        var i = (Height - y - 1) * Width + x;
        // bottom row starts at i = 0
        // second row (from bottom) starts at i = width
        // top row starts at Width * (Height - 1)
        // row (from bottom)    | y(from top)   | offset (as above)
        // 0                    | Height - 1    | 0
        // 1                    | Height - 2    | Width
        // Height - 1           | 0             | (Height - 1) * Width
        //                      | y             | (Height - 1) * Width - y * Width = (Height - y - 1) * Width
        foreach (var c in str) {
            var charWidth = font.Width[c];
            x += charWidth;
            if (Width < x)
                return;
            Blit(c, font, i, charWidth, color);
            i += charWidth;
        }
    }

    private unsafe void Blit (char ascii, Font font, int offset, int w, uint color) {
        var i = font.Offset[ascii];
        fixed (byte* b = Pixels) {
            uint* p = (uint*)b;
            for (var row = 0; row < font.Height; ++row, offset -= Width, i += w)
                for (var column = 0; column < w; ++column)
                    p[offset + column] = font.Pixels[i + column] != 0 ? color : 0xff000000u;
        }
    }

    public void Dispose () {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
