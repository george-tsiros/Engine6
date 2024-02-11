namespace Gl;

using System;
using Common;
using static GlContext;

public class Sampler2D:OpenglObject {
    protected override Action<int> Delete { get; } = DeleteTexture;
    public Vector2i Size { get; }
    public int Width => Size.X;
    public int Height => Size.Y;
    public SizedInternalFormat SizedFormat { get; }
    public void BindTo (int t) {
        if (Disposed)
            throw new ObjectDisposedException(nameof(Sampler2D));
        ActiveTexture(t);
        BindTexture(TextureTarget.TEXTURE_2D, this);
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

    public Sampler2D (Vector2i size, SizedInternalFormat sizedFormat) {
        Id = CreateTexture2D();
        (Size, SizedFormat) = (size, sizedFormat);
        TextureStorage2D(this, 1, SizedFormat, Width, Height);
        TextureBaseLevel(this, 0);
        TextureMaxLevel(this, 0);
        Wrap = Wrap.ClampToEdge;
    }

    public unsafe static Sampler2D FromFile (string filepath) {
        using var raster = Raster.FromFile(filepath);
        Sampler2D texture = new(raster.Size, SizedInternalFormat.RGBA8);
        texture.Upload(raster);
        return texture;
    }

    public unsafe void Upload (Raster raster) {
        if (Disposed)
            throw new ObjectDisposedException(nameof(Sampler2D));
        if (raster.Size != Size)
            throw new ArgumentException($"expected size {Size}, not {raster.Size}", nameof(raster));
        fixed (uint* ptr = raster.Pixels)
            TextureSubImage2D(this, 0, 0, 0, Width, Height, PixelFormat.RGBA, PixelType.UNSIGNED_BYTE, ptr);
    }

    private static readonly SizedInternalFormat[] textureFormats = { SizedInternalFormat.R8, SizedInternalFormat.RG8, SizedInternalFormat.RGB8, SizedInternalFormat.RGBA8 };
    private static SizedInternalFormat TextureFormatWith (int channels) => 1 <= channels && channels <= 4 ? textureFormats[channels - 1] : throw new ApplicationException();
    private static readonly PixelFormat[] pixelFormats = { PixelFormat.RED, PixelFormat.RG, PixelFormat.RGB, PixelFormat.RGBA };
    private static PixelFormat PixelFormatWith (int channels) => 1 <= channels && channels <= 4 ? pixelFormats[channels - 1] : throw new ApplicationException();
}
