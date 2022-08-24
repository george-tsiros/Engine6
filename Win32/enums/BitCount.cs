namespace Win32;

internal enum GWLThing {
    ExStyle = -20,
    Style = -16,
    WndProc = -4,
}
public enum BitCount:ushort {
    Unspecified = 0,
    Monochrome = 1,
    Colors16 = 4,
    Colors256 = 8,
    ColorBits16 = 16,
    ColorBits24 = 24,
    ColorBits32 = 32,
}
