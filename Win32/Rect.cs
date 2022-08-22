using Linear;

namespace Win32;

public readonly struct Rect {
    public readonly int Left;
    public readonly int Top;
    public readonly int Right;
    public readonly int Bottom;

    public Rect (WindowPos w) : this() {
        Left = w.left;
        Top = w.top;
        Right = Left + w.width;
        Bottom = Top + w.height;
    }
    public Rect (Vector2i position, Vector2i size) : this() {
        Left = position.X;
        Right = position.X + size.X;
        Top = position.Y;
        Bottom = position.Y + size.Y;
    }

    public int Width =>
        Right - Left;

    public int Height =>
        Bottom - Top;

    public Vector2i Location => 
        new(Left, Top);

    public Vector2i Size =>
        new(Width, Height);

    public Vector2i Center =>
        new((Left + Right) / 2, (Bottom + Top) / 2);

    public bool IsEmpty =>
        Left == Right || Bottom == Top;

    public override string ToString () =>
        $"({Left},{Top})-({Right},{Bottom})";
}
