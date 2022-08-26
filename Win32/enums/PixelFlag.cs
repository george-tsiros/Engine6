namespace Win32;

using System;
[Flags]
public enum PixelFlag:uint {
    None = 0,
    DoubleBuffer = 0x00000001,
    Stereo = 0x00000002,
    DrawToWindow = 0x00000004,
    DrawToBitmap = 0x00000008,
    SupportGdi = 0x00000010,
    SupportOpengl = 0x00000020,
    GenericFormat = 0x00000040,
    NeedPalette = 0x00000080,
    NeedSystemPalette = 0x00000100,
    SwapExchange = 0x00000200,
    SwapCopy = 0x00000400,
    SwapLayerBuffers = 0x00000800,
    GenericAccelerated = 0x00001000,
    SupportComposition = 0x00008000,
    DepthDontCare = 0x20000000,
    DoubleBufferDontCare = 0x40000000,
    StereoDontCare = 0x80000000u,
    //Typical = DoubleBuffer | DrawToWindow | SupportOpengl | SupportComposition,
}
