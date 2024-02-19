namespace Gl;

using System;
using System.Numerics;
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
        ObjectDisposedException.ThrowIf(disposed, typeof(Raster));
        ClearInternal(color.Abgr);
    }

    private unsafe void ClearInternal (uint value) {
        for (var i = 0; i < Pixels.Length; ++i)
            Pixels[i] = value;
    }

    public void FillRect (Rectangle r, uint color) {
        ObjectDisposedException.ThrowIf(disposed, typeof(Raster));
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
        ObjectDisposedException.ThrowIf(disposed, typeof(Raster));
        var c = color.Abgr;
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
        ObjectDisposedException.ThrowIf(disposed, typeof(Raster));
        SetPixelInternal(p, color.Abgr);
    }

    private void SetPixelInternal (Vector2i p, uint c) {
        if (0 <= p.X && p.X < Width && 0 <= p.Y && p.Y < Height) {
            Pixels[p.Y * Stride + p.X] = c;
        }
    }

    public unsafe void Line (Vector2i a, Vector2i b, Color color) {
        ObjectDisposedException.ThrowIf(disposed, typeof(Raster));
        if (a == b) {
            SetPixelInternal(a, color.Abgr);
        } else if (a.Y == b.Y && 0 <= a.Y && a.Y < Height) {
            var (x0unbounded, x1unbounded) = a.X < b.X ? (a.X, b.X) : (b.X, a.X);
            var (x0, x1) = (int.Clamp(x0unbounded, 0, Width - 1), int.Clamp(x1unbounded, 0, Width - 1));
            var line = a.Y * Width;
            var start = line + x0;
            var end = line + x1;
            while (start <= end)
                Pixels[start++] = color.Abgr;
        } else if (a.X == b.X && 0 <= a.X && a.X < Width) {
            var (y0unbound, y1unbound) = a.Y < b.Y ? (a.Y, b.Y) : (b.Y, a.Y);
            var (y0, y1) = (int.Clamp(y0unbound, 0, Height - 1), int.Clamp(y1unbound, 0, Height - 1));
            for (var i = y1 * Width + a.X; y0 <= y1; i -= Width, --y1)
                Pixels[i] = color.Abgr;
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

    /// <summary><paramref name="y"/> y=0 is top of screen</summary>
    public void DrawString (in ReadOnlySpan<char> str, RasterFont font, int x, int y, uint fore = ~0u, uint back = 0u) {
        ObjectDisposedException.ThrowIf(disposed, typeof(Raster));
        var (textWidth, textHeight) = (str.Length * font.Width, font.Height);
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
            if ('~' < b || b < ' ')
                throw new ArgumentOutOfRangeException(nameof(str), "not printable");
            if (Width <= x + font.Width)
                return;
            Blit(b, font, x, y, fore, back);
            x += font.Width;
        }
    }
    static readonly int[] maskValues = [0x1, 0x2, 0x4, 0x8, 0x10, 0x20, 0x40, 0x80];
    static readonly Vector<int> Mask = new(maskValues);
    static readonly Vector<int> On = new(unchecked((int)0xffd0d0d0));
    static readonly Vector<int> Off = new(unchecked((int)0xff000000));
    /*
    
    */
    private unsafe void Blit (char ascii, RasterFont font, int x, int y, uint fore, uint back) {
        var charStride = font.Width * font.Height;
        var rowStart = (Height - y - 1) * Width;
        var source = ascii * charStride;
        var offset = rowStart + x;

        var glyph = font[ascii];

        for (var row = 0; row < font.Height; ++row, offset -= Width, source += font.Width) {
            Vector<int> r = new(glyph[row]);
            //vi32 const pixels = vi32_or(vi32_and(vi32_eq(vi32_and(row, Mask), Mask), On), vi32_and(vi32_eq(vi32_andnot(row, Mask), Mask), Off));
            var pp = Vector.BitwiseOr(Vector.BitwiseAnd(Vector.Equals(Vector.BitwiseAnd(r, Mask), Mask), On), Vector.BitwiseAnd(Vector.Equals(Vector.AndNot(r, Mask), Mask), Off));
            pp.As<int, uint>().CopyTo(Pixels, offset);
        }
    }

    public void Dispose () {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
