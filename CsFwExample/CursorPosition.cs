namespace CsFwExample;

using System.Runtime.InteropServices;
using System.Drawing;

static class CursorPosition {

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private unsafe static extern bool GetCursorPos (Point* lpPoint);

    unsafe public static Point Get () {
        Point p;
        _ = GetCursorPos(&p);
        return p;
    }
}
