namespace Gl;

using System;
using System.Diagnostics;
using static Opengl;
public class Sampler2D:IDisposable {
    public int Id { get; }
    public Vector2i Size { get; }
    public int Width => Size.X;
    public int Height => Size.Y;
    public TextureFormat SizedFormat { get; }
    public static implicit operator int (Sampler2D sampler) => sampler.Id;
    public void BindTo (int t) {
        if (disposed)
            throw new Exception();
        State.ActiveTexture = t;
        BindTexture(Const.TEXTURE_2D, Id);
    }

    private Wrap wrap;
    public Wrap Wrap {
        get => wrap;
        set {
            if (disposed)
                throw new Exception();
            wrap = value;
            TextureWrap(this, WrapCoordinate.WrapS, value);
            TextureWrap(this, WrapCoordinate.WrapT, value);
        }
    }

    private MinFilter min;
    public MinFilter Min {
        get => min;
        set {
            if (disposed)
                throw new Exception();
            TextureFilter(this, min = value);
        }
    }

    private MagFilter mag;
    public MagFilter Mag {
        get => mag;
        set {
            if (disposed)
                throw new Exception();
            TextureFilter(this, mag = value);
        }
    }

    private Sampler2D () => Id = CreateTexture2D();
    public Sampler2D (Vector2i size, TextureFormat sizedFormat) : this() {
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
        if (raster.Size != Size || raster.BytesPerChannel != 1)
            throw new Exception();

        //Demand(FormatWith
        fixed (byte* ptr = raster.Pixels)
            TextureSubImage2D(this, 0, 0, 0, Width, Height, PixelFormatWith(raster.Channels), Const.UNSIGNED_BYTE, ptr);
    }

    private static readonly TextureFormat[] textureFormats = { TextureFormat.R8, TextureFormat.Rg8, TextureFormat.Rgb8, TextureFormat.Rgba8 };
    private static TextureFormat TextureFormatWith (int channels) => 1 <= channels && channels <= 4 ? textureFormats[channels - 1] : throw new ApplicationException();

    private static readonly PixelFormat[] pixelFormats = { PixelFormat.Red, PixelFormat.Rg, PixelFormat.Bgr, PixelFormat.Bgra };
    private static PixelFormat PixelFormatWith (int channels) => 1 <= channels && channels <= 4 ? pixelFormats[channels - 1] : throw new ApplicationException();

    private bool disposed;
    private void Dispose (bool disposing) {
        if (!disposed) {
            if (disposing)
                DeleteTexture(this);
            disposed = true;
        }
    }
    public void Dispose () {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
