namespace Win32;

using System;
using Common;

public class SizeEventArgs:EventArgs {
    public readonly SizeType SizeType;
    public readonly Vector2i Size;
    public SizeEventArgs (SizeType sizeType, Vector2i size) {
        SizeType = sizeType;
        Size = size;
    }
}

public class ShowWindowEventArgs:EventArgs {
    public readonly bool Shown;
    public readonly ShowWindowReason Reason;
    public ShowWindowEventArgs (bool shown, ShowWindowReason reason) {
        Shown = shown;
        Reason = reason;
    }
}

public class PaintEventArgs:EventArgs {
    public readonly nint Dc;
    public readonly PaintStruct Ps;
    public PaintEventArgs (nint dc, in PaintStruct ps) =>
        (Dc, Ps) = (dc, ps);
}

public class MoveEventArgs:EventArgs {
    public readonly Vector2i ClientRelativePosition;
    public MoveEventArgs (Vector2i clientRelativePosition) {
        ClientRelativePosition = clientRelativePosition;
    }
}

public class KeyEventArgs:EventArgs {
    public readonly Key Key;
    public KeyEventArgs (Key key) {
        Key = key;
    }
}

public class InputEventArgs:EventArgs {
    public readonly int Dx, Dy;
    // could be a vector2i
    public InputEventArgs (int dx, int dy) {
        (Dx, Dy) = (dx, dy);
    }
}

public class FocusChangedEventArgs:EventArgs {
    public readonly bool Focused;
    public FocusChangedEventArgs (bool focused) {
        Focused = focused;
    }
}

public class ButtonEventArgs:EventArgs {
    public readonly MouseButton Button;
    public readonly PointShort Location;
    public ButtonEventArgs (MouseButton button, PointShort location) {
        Button = button;
        Location = location;
    }
}
