namespace Win32;

using System;

public class InputEventArgs:EventArgs {
    public readonly int Dx, Dy;
    // could be a vector2i
    public InputEventArgs (int dx, int dy) {
        (Dx, Dy) = (dx, dy);
    }
}