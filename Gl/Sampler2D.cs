namespace Gl;

using System;
using System.Diagnostics;
using static Opengl;

public class Sampler2D:IDisposable {
    public int Id { get; }
    private Vector2i Size { get; }
    public int Width => Size.X;
    public int Height => Size.Y;
    public TextureInternalFormat SizedFormat { get; }
    public static implicit operator int (Sampler2D sampler) => sampler.Id;

    public void BindTo (int t) {
        Debug.Assert(!disposed);
        State.ActiveTexture = t;
        BindTexture(Const.TEXTURE_2D, Id);
    }

    private Wrap wrap;
    public Wrap Wrap {
        get => wrap;
        set {
            Debug.Assert(!disposed);
            wrap = value;
            TextureWrap(Id, WrapCoordinate.WrapS, value);
            TextureWrap(Id, WrapCoordinate.WrapT, value);
        }
    }
    
    private MinFilter min;
    public MinFilter Min {
        get => min;
        set {
            Debug.Assert(!disposed);
            TextureFilter(Id, min = value);
        }
    }

    private MagFilter mag;
    public MagFilter Mag {
        get => mag;
        set {
            Debug.Assert(!disposed);
            TextureFilter(Id, mag = value);
        }
    }

    private Sampler2D () => Id = CreateTexture2D();
    public Sampler2D (Vector2i size, TextureInternalFormat sizedFormat) : this() {
        (Size, SizedFormat) = (size, sizedFormat);
        TextureStorage2D(Id, 1, SizedFormat, Width, Height);
        TextureBaseLevel(Id, 0);
        TextureMaxLevel(Id, 0);
        Wrap = Wrap.ClampToEdge;
    }
    unsafe public static Sampler2D FromFile (string filepath) {
        using var raster = Raster.FromFile(filepath);
        if (raster.BytesPerChannel != 1)
            throw new ApplicationException();

        var texture = new Sampler2D(raster.Size, SizedFormatWith(raster.Channels));
        fixed (byte* ptr = raster.Pixels)
            TextureSubImage2D(texture.Id, 0, 0, 0, raster.Width, raster.Height, FormatWith(raster.Channels), Const.UNSIGNED_BYTE, ptr);
        return texture;
    }
    private static readonly TextureInternalFormat[] sizedFormats = { TextureInternalFormat.R8, TextureInternalFormat.Rg8, TextureInternalFormat.Rgb8, TextureInternalFormat.Rgba8 };
    private static readonly TextureFormat[] formats = { TextureFormat.Red, TextureFormat.Rg, TextureFormat.Bgr, TextureFormat.Bgra };
    private static TextureInternalFormat SizedFormatWith (int channels) => 1 <= channels && channels <= 4 ? sizedFormats[channels - 1] : throw new ApplicationException();
    private static TextureFormat FormatWith (int channels) => 1 <= channels && channels <= 4 ? formats[channels - 1] : throw new ApplicationException();

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
