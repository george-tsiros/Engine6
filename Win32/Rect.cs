namespace Win32;

public struct Rect {
    public int left;
    public int top;
    public int right;
    public int bottom;
    public int Width => right - left;
    public int Height => bottom - top;
}
