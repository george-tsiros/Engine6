namespace Win32;

public readonly struct Rectangle {
    public readonly int Left, Right, Bottom, Top;
    public int Width => Right - Left;
    public int Height => Top - Bottom;
    public Rectangle (int l, int r, int b, int t) => (Left, Right, Bottom, Top) = (l, r, b, t);
    public Rectangle (Vector2i bottomLeft, Vector2i size) => 
        (Left, Right, Bottom, Top) = (bottomLeft.X, bottomLeft.X + size.X,bottomLeft.Y,  bottomLeft.Y + size.Y);
    public Rectangle Clip (Rectangle r) =>
        new(int.Clamp(Left, r.Left, r.Right), int.Clamp(Right, r.Left, r.Right), int.Clamp(Bottom, r.Bottom, r.Top), int.Clamp(Top, r.Bottom, r.Top));
    public override string ToString () => $"@({Left}, {Bottom}), {Width}x{Height}";
}
