namespace Win32;

public struct BitmapInfoHeader {
    /// <summary>The number of bytes required by the structure</summary>
    public uint size;
    /// <summary>The width of the bitmap, in pixels</summary>
    public int width;
    /// <summary>The height of the bitmap, in pixels. If positive, the bitmap is a bottom-up DIB and its origin is the lower-left corner. If negative, the bitmap is a top-down DIB and its origin is the upper-left corner</summary>
    public int height;
    /// <summary>must be 1</summary>
    public ushort planes;
    /// <summary> </summary>
    public BitCount bitCount;
    /// <summary> </summary>
    public BitmapCompression compression;
    /// <summary>The size, in bytes, of the image. This may be set to zero for BI_RGB bitmaps.</summary>
    public uint sizeImage;
    /// <summary> </summary>
    public int xPelsPerMeter;
    /// <summary> </summary>
    public int yPelsPerMeter;
    /// <summary> </summary>
    public uint colorsUsed;
    /// <summary> </summary>
    public uint colorsImportant;
}
