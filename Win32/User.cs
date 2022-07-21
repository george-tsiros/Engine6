namespace Win32;

using System.Runtime.InteropServices;
using System;

[Flags]
public enum PeekRemove:uint {
    NoRemove,
    Remove,
    NoYield,
}
public static partial class User {
    private const string user32 = nameof(user32) + ".dll";

    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern int GetWindowRect (IntPtr windowHandle, ref Rect rect);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern int ClipCursor (ref Rect rect);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern int SetCursorPos (int x, int y);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern int TrackMouseEvent (ref TrackMouseEvent tme);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern int RegisterRawInputDevices (ref RawInputDevice raw, uint deviceCount, uint structSize);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    public static extern ushort RegisterClassExW ([In] ref WindowClassExW windowClass);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnregisterClassW ([In] IntPtr className, [In, Optional] IntPtr hInstance);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyWindow (IntPtr windowHandle);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern IntPtr CreateWindowExW (WindowStyleEx exStyle, IntPtr classNameOrAtom, IntPtr title, WindowStyle style, int x, int y, int width, int height, IntPtr parentHandle, IntPtr menu, IntPtr instance, IntPtr param);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern IntPtr GetDC (IntPtr windowHandle);

    [DllImport(user32)]
    public static extern IntPtr DefWindowProcW (IntPtr hWnd, WinMessage msg, IntPtr wparam, IntPtr lparam);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ReleaseDC (IntPtr hwnd, IntPtr dc);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    public static extern void PostQuitMessage (int code);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ShowWindow (IntPtr handle, int cmdShow);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UpdateWindow (IntPtr handle);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    public static extern IntPtr GetMessageW (ref Message m, IntPtr handle, uint min, uint max);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool PeekMessageW (ref Message m, IntPtr handle, uint min, uint max, PeekRemove remove);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool TranslateMessage (ref Message m);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    public static extern IntPtr DispatchMessageW (ref Message m);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern IntPtr SetCapture (IntPtr windowHandle);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern void ReleaseCapture ();

    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern IntPtr GetCapture ();

    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    internal static extern int MessageBox (IntPtr hWnd, string text, string caption, uint type);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowText (IntPtr windowHandle, string text);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool PostMessageW (IntPtr handle, uint msg, IntPtr w, IntPtr l);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetClientRect (IntPtr handle, ref Rect clientRect);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    public static extern IntPtr BeginPaint ([In] IntPtr hWnd, [In, Out] ref PaintStruct paint);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EndPaint ([In] IntPtr hWnd, [In] ref PaintStruct paint);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool InvalidateRect ([In] IntPtr handle, [In] ref Rect rect, [In] IntPtr erase);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="device">A handle to the raw input device. This comes from the hDevice member of RAWINPUTHEADER or from GetRawInputDeviceList.</param>
    /// <param name="command">Specifies what data will be returned in <paramref name="data"/>.</param>
    /// <param name="data">A pointer to a buffer that contains the information specified by <paramref name="command"/>.
    /// If <paramref name="command"/> is <see cref="RawInputDeviceCommand.DeviceInfo"/>, set <see cref="RawInputDeviceInfo.size"/> to sizeof(<see cref="RawInputDeviceInfo"/>) before calling <see cref="GetRawInputDeviceInfoW"/>.</param>
    /// <param name="size">The size, in bytes, of the data in <paramref name="data"/>.</param>
    /// <returns>If successful, this function returns a non-negative number indicating the number of bytes copied to <paramref name="data"/>.
    /// If <paramref name="data"/> is not large enough for the data, the function returns -1. 
    /// If <paramref name="data"/> is null, the function returns a value of zero. 
    /// In both of these cases, <paramref name="size"/> is set to the minimum size required for the <paramref name="data"/> buffer.</returns>

    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    unsafe public static extern int GetRawInputDeviceInfoW (IntPtr device, RawInputDeviceCommand command, void* data, uint* size);

    /// <param name="devices">An array of <see cref="RawInputDeviceList"/> structures for the devices attached to the system.If null, the number of devices are returned in <paramref name="count"/>.</param>
    /// <param name="count">If <paramref name="devices"/> is null, the function populates this variable with the number of devices attached to the system; otherwise, this variable specifies the number of <see cref="RawInputDeviceList"/> structures that can be contained in the buffer to which <paramref name="devices"/> points. If this value is less than the number of devices attached to the system, the function returns the actual number of devices in this variable and fails with ERROR_INSUFFICIENT_BUFFER.</param>
    /// <param name="size">The size of a <see cref="RawInputDeviceList"/> structure, in bytes.</param>
    /// <returns>If the function is successful, the return value is the number of devices stored in the buffer pointed to by <paramref name="devices"/>. On any other error, the function returns (UINT) -1 and GetLastError returns the error indication.</returns>
    /// <remarks>The devices returned from this function are the mouse, the keyboard, and other Human Interface Device (HID) devices.To get more detailed information about the attached devices, call GetRawInputDeviceInfo using the <see cref="RawInputDeviceList.device"/> from <see cref="RawInputDeviceList"/>.</remarks>

    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern uint GetRawInputDeviceList (ref RawInputDeviceList devices, ref uint count, uint size);

    public static ushort RegisterWindowClass (WndProc wndProc, string className) {
        var windowClass = WindowClassExW.Create();
        windowClass.style = ClassStyle.HRedraw | ClassStyle.VRedraw | ClassStyle.OwnDc;
        windowClass.wndProc = wndProc;
        windowClass.classname = className;
        var atom = User.RegisterClassExW(ref windowClass);
        return atom != 0 ? atom : throw new Exception("failed to register class");
    }

    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern IntPtr GetWindowLongPtrA (IntPtr hWnd, int nIndex);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern IntPtr SetWindowLongPtrA (IntPtr hWnd, int nIndex, IntPtr newLong);

    public static void CreateWindow (ushort atom, Vector2i size, IntPtr? moduleHandle = null) {
        _ = User.CreateWindowExW(WindowStyleEx.None, new(atom), IntPtr.Zero, WindowStyle.ClipPopup, 0, 0, size.X, size.Y, IntPtr.Zero, IntPtr.Zero, moduleHandle ?? Kernel.GetModuleHandleW(null), IntPtr.Zero);
    }

}
public enum RawInputDeviceCommand:uint {
    PreparsedData = 0x20000005,
    DeviceName = 0x20000007,
    DeviceInfo = 0x2000000b,
}

[Flags]
public enum WindowStyleEx:uint {
    None = /*               */0x0,
    DlgModalFrame = /*      */0x00000001,
    NoParentNotify = /*     */0x00000004,
    TopMost = /*            */0x00000008,
    AcceptFiles = /*        */0x00000010,
    Transparent = /*        */0x00000020,
    MdiChild = /*           */0x00000040,
    ToolWindow = /*         */0x00000080,
    WindowEdge = /*         */0x00000100,
    ClientEdge = /*         */0x00000200,
    ContextHelp = /*        */0x00000400,
    Right = /*              */0x00001000,
    RtlReading = /*         */0x00002000,
    LeftScrollBar = /*      */0x00004000,
    ControlParent = /*      */0x00010000,
    StaticEdge = /*         */0x00020000,
    AppWindow = /*          */0x00040000,
    Layered = /*            */0x00080000,
    NoInheritLayout = /*    */0x00100000,
    NoRedirectionBitmap = /**/0x00200000,
    LayoutRtl = /*          */0x00400000,
    Composited = /*         */0x02000000,
    NoActivate = /*         */0x08000000,
    OverlappedWindow = /*   */WindowEdge | ClientEdge,
    PaletteWindow = /*      */WindowEdge | ToolWindow | TopMost,
}

[Flags]
public enum WindowStyle:uint {
    Overlapped = /*     */ 0x00000000,
    Tabstop = /*        */ 0x00010000,
    MaximizeBox = /*    */ 0x00010000,
    MinimizeBox = /*    */ 0x00020000,
    Group = /*          */ 0x00020000,
    Thickframe = /*     */ 0x00040000,
    Sysmenu = /*        */ 0x00080000,
    Hscroll = /*        */ 0x00100000,
    Vscroll = /*        */ 0x00200000,
    Dlgframe = /*       */ 0x00400000,
    Border = /*         */ 0x00800000,
    Maximize = /*       */ 0x01000000,
    ClipChildren = /*   */ 0x02000000,
    ClipSiblings = /*   */ 0x04000000,
    Disabled = /*       */ 0x08000000,
    Visible = /*        */ 0x10000000,
    Minimize = /*       */ 0x20000000,
    Child = /*          */ 0x40000000,
    Popup = unchecked(0x80000000),
    Tiled = Overlapped,
    ChildWindow = Child,
    Iconic = Minimize,
    Sizebox = Thickframe,
    Caption = Border | Dlgframe,
    OverlappedWindow = Caption | Sysmenu | Thickframe | MinimizeBox | MaximizeBox,
    TiledWindow = OverlappedWindow,
    PopupWindow = Popup | Border | Sysmenu,
    ClipPopup = ClipChildren | ClipSiblings | Popup,
}
