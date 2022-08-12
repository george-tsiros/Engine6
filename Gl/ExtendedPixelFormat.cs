namespace Gl;

struct ExtendedPixelFormat {
    public int Index;
    public PixelType PixelType;
    public Acceleration Acceleration;
    public int ColorBits;
    public int DepthBits;
    public bool DoubleBuffer;
    public SwapMethod SwapMethod;
    public override string ToString () => 
        $"{Index}: {PixelType}, {Acceleration}, {ColorBits}/{DepthBits}, {DoubleBuffer}, {SwapMethod}";
}
