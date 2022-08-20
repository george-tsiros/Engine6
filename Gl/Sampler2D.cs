namespace Gl;

using System;
using static Opengl;
using Win32;
using Linear;

public class Sampler2D:OpenglObject {
    protected override Action<int> Delete { get; } =
        DeleteTexture;

    public Vector2i Size { get; }

    public int Width =>
        Size.X;

    public int Height =>
        Size.Y;

    public TextureFormat SizedFormat { get; }

    public void BindTo (int t) {
        if (Disposed)
            throw new ObjectDisposedException(nameof(Sampler2D));
        State.ActiveTexture = t;
        BindTexture(Const.TEXTURE_2D, Id);
    }

    private Wrap wrap;
    public Wrap Wrap {
        get => Disposed ? throw new ObjectDisposedException(nameof(Sampler2D)) : wrap;
        set {
            if (Disposed)
                throw new ObjectDisposedException(nameof(Sampler2D));
            TextureWrap(this, WrapCoordinate.WrapS, wrap = value);
            TextureWrap(this, WrapCoordinate.WrapT, value);
        }
    }

    private MinFilter min;
    public MinFilter Min {
        get => Disposed ? throw new ObjectDisposedException(nameof(Sampler2D)) : min;
        set {
            if (Disposed)
                throw new ObjectDisposedException(nameof(Sampler2D));
            TextureFilter(this, min = value);
        }
    }

    private MagFilter mag;
    public MagFilter Mag {
        get => Disposed ? throw new ObjectDisposedException(nameof(Sampler2D)) : mag;
        set {
            if (Disposed)
                throw new ObjectDisposedException(nameof(Sampler2D));
            TextureFilter(this, mag = value);
        }
    }

    public Sampler2D (Vector2i size, TextureFormat sizedFormat) {
        Id = CreateTexture2D();
        (Size, SizedFormat) = (size, sizedFormat);
        TextureStorage2D(this, 1, SizedFormat, Width, Height);
        TextureBaseLevel(this, 0);
        TextureMaxLevel(this, 0);
        Wrap = Wrap.ClampToEdge;
    }

    unsafe public static Sampler2D FromFile (string filepath) {
        using var raster = Raster.FromFile(filepath);
        if (raster.BytesPerChannel != 1)
            throw new ArgumentException("only 1 byte per pixel bitmaps are supported");

        var texture = new Sampler2D(raster.Size, TextureFormatWith(raster.Channels));
        texture.Upload(raster);
        return texture;
    }

    unsafe public void Upload (Raster raster) {
        if (Disposed)
            throw new ObjectDisposedException(nameof(Sampler2D));

        if (raster.Size != Size)
            throw new ArgumentException($"expected size {Size}, not {raster.Size}", nameof(raster));

        if (raster.BytesPerChannel != 1)
            throw new ArgumentException($"expected 1 byte per channel, not {raster.BytesPerChannel}", nameof(raster));

        fixed (byte* ptr = raster.Pixels)
            TextureSubImage2D(this, 0, 0, 0, Width, Height, PixelFormatWith(raster.Channels), Const.UNSIGNED_BYTE, ptr);
    }

    private static readonly TextureFormat[] textureFormats =
        { TextureFormat.R8, TextureFormat.Rg8, TextureFormat.Rgb8, TextureFormat.Rgba8 };

    private static TextureFormat TextureFormatWith (int channels) =>
        1 <= channels && channels <= 4 ? textureFormats[channels - 1] : throw new ApplicationException();

    private static readonly PixelFormat[] pixelFormats =
        { PixelFormat.Red, PixelFormat.Rg, PixelFormat.Rgb, PixelFormat.Rgba };

    private static PixelFormat PixelFormatWith (int channels) =>
        1 <= channels && channels <= 4 ? pixelFormats[channels - 1] : throw new ApplicationException();
}
