namespace Gl;

using System;
using System.IO;
using System.IO.Compression;
using Win32;
using Common;
using System.Diagnostics;

public class Raster:IDisposable {

    public readonly Vector2i Size;
    public int Width => Size.X;
    public int Height => Size.Y;
    public int Stride => Size.X;

    public uint[] Pixels;

    public Raster (Vector2i size) {
        if (size.X < 1 || size.Y < 1)
            throw new ArgumentException("invalid value", nameof(size));
        Size = size;
        Pixels = new uint[Width * Height];
    }

    public void Clear (Color color) {
        NotDisposed();
        ClearInternal(color.Argb);
    }

    private unsafe void ClearInternal (uint value) {
        for (var i = 0; i < Pixels.Length; ++i)
            Pixels[i] = value;
    }

    public void FillRect (Rectangle r, uint color) {
        NotDisposed();
        var clipped = r.Clip(new(Vector2i.Zero, Size));
        if (clipped.Width <= 0 || clipped.Height <= 0)
            return;
        FillRectInternal(clipped, color);
    }

    private unsafe void FillRectInternal (Rectangle clipped, uint color) {
        Debug.Assert(clipped.Left < clipped.Right);
        Debug.Assert(clipped.Top < clipped.Bottom);
        Debug.Assert((Pixels.Length & 3) == 0);
        for (var y = clipped.Top; y < clipped.Bottom; ++y)
            for (var x = clipped.Left; x < clipped.Right; ++x)
                Pixels[y * Width + x] = color;
    }

    private static bool IsTopLeft (in Vector2i a, in Vector2i b) =>
        a.Y == b.Y && b.X < a.X || b.Y < a.Y;

    private static int Orient2D (in Vector2i a, in Vector2i b, in Vector2i c) => (b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y);

    public unsafe void Triangle (Vector2i v0, Vector2i v1, Vector2i v2, Color color) {
        NotDisposed();
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

        var row0 = min.Y * Width + min.X;
        for (var y = min.Y; y <= max.Y; ++y) {
            var w0biased = w0_row;
            var w1biased = w1_row;
            var w2biased = w2_row;
            var i = row0;
            for (var x = min.X; x <= max.X; ++x) {
                if (0 <= (w0biased | w1biased | w2biased)) {
                    Pixels[i] = c;
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


    public void SetPixel (Vector2i p, Color color) {
        NotDisposed();
        SetPixelInternal(p, color.Argb);
    }

    private void SetPixelInternal (Vector2i p, uint c) {
        if (0 <= p.X && p.X < Width && 0 <= p.Y && p.Y < Height) {
            Pixels[p.Y * Stride + p.X] = c;
        }
    }

    public unsafe void Line (Vector2i a, Vector2i b, Color color) {
        NotDisposed();
        if (a == b) {
            SetPixelInternal(a, color.Argb);
        } else if (a.Y == b.Y && 0 <= a.Y && a.Y < Height) {
            var (x0unbounded, x1unbounded) = a.X < b.X ? (a.X, b.X) : (b.X, a.X);
            var (x0, x1) = (int.Clamp(x0unbounded, 0, Width - 1), int.Clamp(x1unbounded, 0, Width - 1));
            var line = a.Y * Width;
            var start = line + x0;
            var end = line + x1;
            while (start <= end)
                Pixels[start++] = color.Argb;
        } else if (a.X == b.X && 0 <= a.X && a.X < Width) {
            var (y0unbound, y1unbound) = a.Y < b.Y ? (a.Y, b.Y) : (b.Y, a.Y);
            var (y0, y1) = (int.Clamp(y0unbound, 0, Height - 1), int.Clamp(y1unbound, 0, Height - 1));
            for (var i = y1 * Width + a.X; y0 <= y1; i -= Width, --y1)
                Pixels[i] = color.Argb;
        } else {
            throw new NotImplementedException();
            // i am incompetent. 
        }
    }

    private static Raster FromStream (FileStream f) {
        using BinaryReader r = new(f, System.Text.Encoding.UTF8, true);
        var width = r.ReadInt32();
        var height = r.ReadInt32();
        var channels = r.ReadInt32();
        if (4 != channels)
            throw new ArgumentOutOfRangeException(nameof(channels), "only 4 channel bitmaps are currently supported");
        var bytesPerChannel = r.ReadInt32();
        if (1 != bytesPerChannel)
            throw new ArgumentOutOfRangeException(nameof(bytesPerChannel), "only 1Bpp bitmaps are currently supported");
        return new Raster(new(width, height));
    }

    public unsafe static Raster FromFile (string filepath) {
        using var f = File.OpenRead(filepath);
        var raster = FromStream(f);
        using DeflateStream unzip = new(f, CompressionMode.Decompress);
        fixed (uint* uintp = raster.Pixels) {
            var span = new Span<byte>(uintp, raster.Pixels.Length * sizeof(uint));
            var read = unzip.Read(span);
            Debug.Assert(raster.Pixels.Length * sizeof(uint) == read);
        }
        return raster;
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
    public void DrawString (in ReadOnlySpan<char> str, PixelFont font, int x, int y, uint fore = ~0u, uint back = 0u) {
        NotDisposed();
        var (textWidth, textHeight) = font.SizeOf(str);
        if (textHeight != font.Height)
            throw new ArgumentException("does not support multiple lines yet", nameof(str));
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
        foreach (var b in str) {
            if (255 < b)
                throw new ArgumentOutOfRangeException(nameof(str), "not ascii");
            if (Width <= x)
                return;
            Blit(b, font, x, y, fore, back);
            x += font.Width;
        }
    }

    private unsafe void Blit (char ascii, PixelFont font, int x, int y, uint fore, uint back) {
        var charStride = font.Width * font.Height;
        var rowStart = (Height - y - 1) * Width;
        var source = ascii * charStride;
        var offset = rowStart + x;

        for (var row = 0; row < font.Height; ++row, offset -= Width, source += font.Width) {
            var xpos = x;
            for (var column = 0; xpos < Width && column < font.Width; ++column, ++xpos) {

                Pixels[offset + column] = font.Pixels[source + column] != 0 ? fore : back;

            }
        }
    }

    public void Dispose () {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
