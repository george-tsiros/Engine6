namespace Win32;

using System;

public class ShowWindowEventArgs:EventArgs {
    public readonly bool Shown;
    public readonly ShowWindowReason Reason;
    public ShowWindowEventArgs (bool shown, ShowWindowReason reason) {
        Shown = shown;
        Reason = reason;
    }
}
