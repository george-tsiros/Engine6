namespace Win32;

public struct CreateStructW {
    public nint lpCreateParams;
    public nint instance;
    public nint menu;
    public nint parentWindowHandle;
    public int h;
    public int w;
    public int y;
    public int x;
    public WindowStyle style;
    public nint windowName;
    public nint classNameOrAtom;
    public WindowStyleEx exStyle;
}
