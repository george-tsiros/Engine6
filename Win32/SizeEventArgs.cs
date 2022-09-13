namespace Win32;

using Common;
using System;

public class SizeEventArgs:EventArgs {
    public readonly SizeType SizeType;
    public readonly Vector2i Size;
    public SizeEventArgs (SizeType sizeType, Vector2i size) {
        SizeType = sizeType;
        Size = size;
    }
}
