namespace Win32;

using Common;
using System;
using System.Runtime.InteropServices;

public struct Rectangle {

    public int Left;
    public int Top;
    public int Right;
    public int Bottom;

    public Rectangle (int l, int t, int r, int b) =>
            (Left, Top, Right, Bottom) = (l, t, r, b);

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
        new(Maths.IntClamp(Left, r.Left, r.Right), Maths.IntClamp(Top, r.Bottom, r.Top), Maths.IntClamp(Right, r.Left, r.Right), Maths.IntClamp(Bottom, r.Bottom, r.Top));

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
