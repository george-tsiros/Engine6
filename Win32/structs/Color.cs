using System.Runtime.InteropServices;

namespace Win32;

[StructLayout(LayoutKind.Explicit)]
public readonly struct Color {
    [FieldOffset(0)] public readonly uint Argb;

    public static implicit operator uint (Color c) => 
        c.Argb;
    
    private Color (uint argb) =>
        Argb = argb;

    public static Color FromRgb (int red, int green, int blue) => new(0xff000000u | ((uint)red << 16) | ((uint)green << 8) | ((uint)blue));
    public static readonly Color Black = new(0xff000000);
    public static readonly Color White = new(0xffffffff);
    public static readonly Color Red = new(0xffff0000);
    public static readonly Color Green = new(0xff00ff00);
    public static readonly Color Blue = new(0xff0000ff);
    public static readonly Color QuarterBlue = new(0xff000040);
    public static readonly Color Yellow = new(0xffffff00);
    public static readonly Color Magenta = new(0xffff00ff);
    public static readonly Color Cyan = new(0xffffff00);
}
