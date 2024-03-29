namespace Win32;

using Common;

public struct Rectangle {

    public int Left;
    public int Top;
    public int Right;
    public int Bottom;

    public Rectangle (int l, int t, int r, int b) =>
            (Left, Top, Right, Bottom) = (l, t, r, b);

    public Rectangle (in Vector2i position, in Vector2i size) : this() {
        Left = position.X;
        Right = position.X + size.X;
        Top = position.Y;
        Bottom = position.Y + size.Y;
    }

    public int Width =>
        Right - Left;

    public int Height =>
        Bottom - Top;

    public Rectangle Clip (in Rectangle r) =>
        new(int.Clamp(Left, r.Left, r.Right), int.Clamp(Top, r.Top, r.Bottom), int.Clamp(Right, r.Left, r.Right), int.Clamp(Bottom, r.Top, r.Bottom));

    public Vector2i Location =>
        new(Left, Top);

    public Vector2i Size =>
        new(Width, Height);

    public Vector2i Center =>
        new((Left + Right) / 2, (Bottom + Top) / 2);

    public bool IsEmpty =>
        Left == Right || Bottom == Top;

    public override string ToString () =>
        $"[{Location}, {Width} x {Height}]";
}
