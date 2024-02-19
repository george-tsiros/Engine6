using System.Runtime.InteropServices;

namespace Win32;

[StructLayout(LayoutKind.Explicit)]
public readonly struct Color {
    [FieldOffset(0)] public readonly uint Abgr;
    [FieldOffset(0)] public readonly byte A;
    [FieldOffset(1)] public readonly byte B;
    [FieldOffset(2)] public readonly byte G;
    [FieldOffset(3)] public readonly byte R;
    public static implicit operator uint (Color c) => c.Abgr;

    private Color (uint abgr) => Abgr = abgr;

    public static Color FromArgb (int a, int r, int g, int b) =>
        new((uint)((a & 0xff) << 24) | ((uint)(r & 0xff)) | ((uint)(g & 0xff) << 8) | ((uint)(b & 0xff) << 16));
    public static Color FromRgb (int r, int g, int b) =>
        new(0xff000000u | ((uint)(r & 0xff)) | ((uint)(g & 0xff) << 8) | ((uint)(b & 0xff) << 16));

    public static readonly Color Transparent = new(0);
    public static readonly Color Black = new(0xff000000);
    public static readonly Color White = new(0xffffffff);
    public static readonly Color Red = new(0xff0000ff);
    public static readonly Color Green = new(0xff00ff00);
    public static readonly Color Blue = new(0xffff0000);
    public static readonly Color QuarterBlue = new(0xff400000);
    public static readonly Color Yellow = new(0xff00ffff);
    public static readonly Color Magenta = new(0xffff00ff);
    public static readonly Color Cyan = new(0xff00ffff);
}
