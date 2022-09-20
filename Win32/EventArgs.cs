namespace Win32;

using System;
using Common;

public readonly struct SizeArgs {
    public readonly SizeType SizeType;
    public readonly Vector2i Size;
    public SizeArgs (SizeType sizeType, Vector2i size) =>
        (SizeType, Size) = (sizeType, size);
}

public readonly struct ShowWindowArgs {
    public readonly bool Shown;
    public readonly ShowWindowReason Reason;
    public ShowWindowArgs (bool shown, ShowWindowReason reason) =>
        (Shown, Reason) = (shown, reason);
}

public readonly struct PaintArgs {
    public readonly nint Dc;
    public readonly PaintStruct Ps;
    public PaintArgs (nint dc, in PaintStruct ps) =>
        (Dc, Ps) = (dc, ps);
}

public readonly struct MoveArgs {
    public readonly Vector2i ClientRelativePosition;
    public MoveArgs (Vector2i clientRelativePosition) =>
        ClientRelativePosition = clientRelativePosition;
}

public readonly struct KeyArgs {
    public readonly Key Key;
    public readonly bool Repeat;
    public KeyArgs (Key key, bool repeat) =>
        (Key, Repeat) = (key, repeat);
}

public readonly struct InputArgs {
    public readonly int Dx, Dy;
    // could be a vector2i
    public InputArgs (int dx, int dy) =>
        (Dx, Dy) = (dx, dy);
}

public readonly struct FocusChangedArgs {
    public readonly bool Focused;
    public FocusChangedArgs (bool focused) =>
        Focused = focused;
}

public readonly struct ButtonArgs {
    public readonly MouseButton Button;
    public readonly PointShort Location;
    public ButtonArgs (MouseButton button, PointShort location) =>
        (Button, Location) = (button, location);
}
