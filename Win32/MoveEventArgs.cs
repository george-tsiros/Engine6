namespace Win32;

using Common;
using System;

public class MoveEventArgs:EventArgs {
    public readonly Vector2i ClientRelativePosition;
    public MoveEventArgs (Vector2i clientRelativePosition) {
        ClientRelativePosition = clientRelativePosition;
    }
}
