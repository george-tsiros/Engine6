namespace Win32;

using System;

public struct CreateStructW {
    public IntPtr lpCreateParams;
    public IntPtr instance;
    public IntPtr menu;
    public IntPtr parentWindowHandle;
    public int h;
    public int w;
    public int y;
    public int x;
    public WindowStyle style;
    public IntPtr windowName;
    public IntPtr classNameOrAtom;
    public WindowStyleEx exStyle;
}
