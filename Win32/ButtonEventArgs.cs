namespace Win32;

using System;

public class ButtonEventArgs:EventArgs {
    public readonly MouseButton Button;
    public readonly PointShort Location;
    public ButtonEventArgs (MouseButton button, PointShort location) {
        Button = button;
        Location = location;
    }
}
