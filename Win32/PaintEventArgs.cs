namespace Win32;

using System;

public class PaintEventArgs:EventArgs {
    public readonly nint Dc;
    public readonly PaintStruct Ps;
    public PaintEventArgs (nint dc,  in PaintStruct ps) =>
        (Dc, Ps) = (dc, ps);
}
