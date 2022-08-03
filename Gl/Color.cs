namespace Gl;

public readonly struct Color {
    public readonly uint Argb;
    
    Color (uint argb) =>
        Argb = argb;
    
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