namespace Win32;

using Linear;
using System;

public readonly struct Rectangle {

    public readonly int Left;
    public readonly int Top;
    public readonly int Right;
    public readonly int Bottom;

    public Rectangle (int l, int t, int r, int b) =>
            (Left, Top, Right, Bottom) = (l, t, r, b);

    public Rectangle (WindowPos w) : this() {
        Left = w.left;
        Top = w.top;
        Right = Left + w.width;
        Bottom = Top + w.height;
    }

    public Rectangle (Vector2i position, Vector2i size) : this() {
        Left = position.X;
        Right = position.X + size.X;
        Top = position.Y;
        Bottom = position.Y + size.Y;
    }

    public int Width =>
        Right - Left;

    public int Height =>
        Bottom - Top;

    public Rectangle Clip (Rectangle r) =>
        new(Math.Clamp(Left, r.Left, r.Right), Math.Clamp(Top, r.Bottom, r.Top), Math.Clamp(Right, r.Left, r.Right), Math.Clamp(Bottom, r.Bottom, r.Top));

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
