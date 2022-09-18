namespace Gl;

using System;
using System.IO;
using System.IO.Compression;
using Win32;
using Common;

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

    public void ClearU32 (Color color) {
        NotDisposed();
        if (Channels != 4)
            throw new InvalidOperationException($"{nameof(ClearU32)} only works with 4 channels, not {Channels}");
        ClearU32Internal((color.Argb << 32) | color.Argb);
    }

    private unsafe void ClearU32Internal (ulong ul) {
        fixed (byte* bp = Pixels) {
            var p = (ulong*)bp;
            if ((Pixels.Length & 7) != 0)
                throw new InvalidOperationException("???");
            var count = Pixels.Length >> 3;
            for (var i = 0; i < count; ++i)
                p[i] = ul;
        }
    }

    public void FillRectU32 (Rectangle r, uint color = ~0u) {
        NotDisposed();
        if (Channels != 4)
            throw new InvalidOperationException($"{nameof(FillRectU32)} only works with 4 channels, not {Channels}");
        var clipped = r.Clip(new(Vector2i.Zero, Size));
        if (clipped.Width <= 0 || clipped.Height <= 0)
            return;
        FillRectU32Internal(clipped, color);
    }

    private unsafe void FillRectU32Internal (Rectangle clipped, uint color) {
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

    private static bool IsTopLeft (in Vector2i a, in Vector2i b) =>
        a.Y == b.Y && b.X < a.X || b.Y < a.Y;

    private static int Orient2D (in Vector2i a, in Vector2i b, in Vector2i c) => (b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y);

    public unsafe void TriangleU32 (Vector2i v0, Vector2i v1, Vector2i v2, Color color) {
        NotDisposed();
        if (Channels != 4)
            throw new InvalidOperationException($"{nameof(TriangleU32)} only works with 4 channels, not {Channels}");
        if (BytesPerChannel != 1)
            throw new InvalidOperationException($"{nameof(TriangleU32)} only works with 1 Bpp, not {BytesPerChannel}");
        var c = color.Argb;
        var min = Vector2i.Max(Vector2i.Min(Vector2i.Min(v0, v1), v2), Vector2i.Zero);
        var max = Vector2i.Min(Vector2i.Max(Vector2i.Max(v0, v1), v2), Size - Vector2i.One);

        var bias0 = IsTopLeft(v1, v2) ? 0 : -1;
        var bias1 = IsTopLeft(v2, v0) ? 0 : -1;
        var bias2 = IsTopLeft(v0, v1) ? 0 : -1;

        var (A01, B01) = (v0.Y - v1.Y, v1.X - v0.X);
        var (A12, B12) = (v1.Y - v2.Y, v2.X - v1.X);
        var (A20, B20) = (v2.Y - v0.Y, v0.X - v2.X);

        var w0_row = Orient2D(v1, v2, min) + bias0;
        var w1_row = Orient2D(v2, v0, min) + bias1;
        var w2_row = Orient2D(v0, v1, min) + bias2;

        fixed (byte* bp = Pixels) {
            var p = (uint*)bp;
            var row0 = min.Y * Width + min.X;
            for (var y = min.Y; y <= max.Y; ++y) {
                var w0biased = w0_row;
                var w1biased = w1_row;
                var w2biased = w2_row;
                var i = row0;
                for (var x = min.X; x <= max.X; ++x) {
                    if (0 <= (w0biased | w1biased | w2biased)) {
                        p[i] = c;
                    }
                    ++i;
                    w0biased += A12;
                    w1biased += A20;
                    w2biased += A01;
                }
                w0_row += B12;
                w1_row += B20;
                w2_row += B01;
                row0 += Width;
            }
        }
    }

    public unsafe void TriangleU8 (Vector2i v0, Vector2i v1, Vector2i v2, byte u8) {
        NotDisposed();
        if (Channels != 1)
            throw new InvalidOperationException($"{nameof(TriangleU8)} only works with 1 channels, not {Channels}");
        if (BytesPerChannel != 1)
            throw new InvalidOperationException($"{nameof(TriangleU8)} only works with 1 Bpp, not {BytesPerChannel}");
        var min = Vector2i.Max(Vector2i.Min(Vector2i.Min(v0, v1), v2), Vector2i.Zero);
        var max = Vector2i.Min(Vector2i.Max(Vector2i.Max(v0, v1), v2), Size - Vector2i.One);

        var bias0 = IsTopLeft(v1, v2) ? 0 : -1;
        var bias1 = IsTopLeft(v2, v0) ? 0 : -1;
        var bias2 = IsTopLeft(v0, v1) ? 0 : -1;

        var (A01, B01) = (v0.Y - v1.Y, v1.X - v0.X);
        var (A12, B12) = (v1.Y - v2.Y, v2.X - v1.X);
        var (A20, B20) = (v2.Y - v0.Y, v0.X - v2.X);

        var w0_row = Orient2D(v1, v2, min) + bias0;
        var w1_row = Orient2D(v2, v0, min) + bias1;
        var w2_row = Orient2D(v0, v1, min) + bias2;

        var row0 = min.Y * Width + min.X;
        for (var y = min.Y; y <= max.Y; ++y) {
            var w0biased = w0_row;
            var w1biased = w1_row;
            var w2biased = w2_row;
            var i = row0;
            for (var x = min.X; x <= max.X; ++x) {
                if (0 <= (w0biased | w1biased | w2biased)) {

                    Pixels[i] = u8;
                }
                ++i;
                w0biased += A12;
                w1biased += A20;
                w2biased += A01;
            }
            w0_row += B12;
            w1_row += B20;
            w2_row += B01;
            row0 += Width;
        }
    }

    public void PixelU32 (Vector2i p, Color color) {
        NotDisposed();
        if (Channels != 4)
            throw new InvalidOperationException($"{nameof(PixelU32)} only works with 4 channels, not {Channels}");
        PixelU32Internal(p, color.Argb);
    }

    private void PixelU32Internal (Vector2i p, uint c) {
        if (0 <= p.X && p.X < Width && 0 <= p.Y && p.Y < Height) {
            var i = p.Y * Stride + 4 * p.X;
            Pixels[i] = (byte)(c & 0xff);
            Pixels[i + 1] = (byte)((c >> 8) & 0xff);
            Pixels[i + 2] = (byte)((c >> 16) & 0xff);
            Pixels[i + 3] = (byte)(c >> 24);
        }
    }

    public unsafe void LineU32 (Vector2i a, Vector2i b, Color color) {
        NotDisposed();
        if (Channels != 4)
            throw new InvalidOperationException($"{nameof(LineU32)} only works with 4 channels, not {Channels}");
        if (a == b) {
            PixelU32Internal(a, color.Argb);
        } else if (a.Y == b.Y && 0 <= a.Y && a.Y < Height) {
            var (x0unbounded, x1unbounded) = a.X < b.X ? (a.X, b.X) : (b.X, a.X);
            var (x0, x1) = (Maths.IntClamp(x0unbounded, 0, Width - 1), Maths.IntClamp(x1unbounded, 0, Width - 1));
            fixed (byte* bytes = Pixels) {
                uint* p = (uint*)bytes;
                var line = a.Y * Width; // NOT stride
                var start = line + x0;
                var end = line + x1;
                while (start <= end)
                    p[start++] = color.Argb;
            }
        } else if (a.X == b.X && 0 <= a.X && a.X < Width) {
            var (y0unbound, y1unbound) = a.Y < b.Y ? (a.Y, b.Y) : (b.Y, a.Y);
            var (y0, y1) = (Maths.IntClamp(y0unbound, 0, Height - 1), Maths.IntClamp(y1unbound, 0, Height - 1));
            fixed (byte* bp = Pixels) {
                var p = (uint*)bp;
                for (var i = y1 * Width + a.X; y0 <= y1; i -= Width, --y1)
                    p[i] = color.Argb;
            }
        } else {

            var (p0, p1) = a.Y < b.Y ? (b, a) : (a, b);
            var dp = p1 - p0;

            if (Maths.IntAbs(dp.X) < Maths.IntAbs(dp.Y)) {

            } else {

            }
        }
    }

    private static Raster FromStream (FileStream f) {
        using BinaryReader r = new(f, System.Text.Encoding.UTF8, true);
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
        using DeflateStream unzip = new(f, CompressionMode.Decompress);
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

    private void NotDisposed () {
        if (disposed)
            throw new ObjectDisposedException(nameof(Dib));
    }

    /// <summary><paramref name="y"/> y=0 is top of screen</summary>
    public void DrawString (in ReadOnlySpan<char> str, PixelFont font, int x, int y, uint color = ~0u) {
        NotDisposed();
        var (textWidth, textHeight) = font.SizeOf(str);
        if (textHeight != font.Height)
            throw new ArgumentException();
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
            Blit(c, font, x, y, color);
            x += font.Width;
        }
    }

    private unsafe void Blit (char ascii, PixelFont font, int x, int y, uint color) {
        var charStride = font.Width * font.Height;
        var rowStart = (Height - y - 1) * Width;
        var source = ascii * charStride;
        var offset = rowStart + x;
        fixed (byte* b = Pixels) {
            uint* p = (uint*)b;

            for (var row = 0; row < font.Height; ++row, offset -= Width, source += font.Width) {
                var xpos = x;
                for (var column = 0; xpos < Width && column < font.Width; ++column, ++xpos) {

                    p[offset + column] = font.Pixels[source + column] != 0 ? color : 0xff000000u;

                }
            }
        }
    }

    public void Dispose () {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
