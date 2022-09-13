namespace Win32;

using System;

public class FocusChangedEventArgs:EventArgs {
    public readonly bool Focused;
    public FocusChangedEventArgs (bool focused) {
        Focused = focused;
    }
}
