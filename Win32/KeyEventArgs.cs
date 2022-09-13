namespace Win32;

using System;

public class KeyEventArgs:EventArgs {
    public readonly Key Key;
    public KeyEventArgs (Key key) {
        Key = key;
    }
}
