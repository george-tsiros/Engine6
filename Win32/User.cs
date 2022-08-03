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

    /// <summary>
    /// ?
    /// </summary>
    /// <param name="exStyle">The extended window style of the window being created. For a list of possible values, see Extended Window Styles.</param>
    /// <param name="classNameOrAtom">A null-terminated string or a class atom created by a previous call to the RegisterClass or RegisterClassEx function. The atom must be in the low-order word of lpClassName; the high-order word must be zero. If lpClassName is a string, it specifies the window class name. The class name can be any name registered with RegisterClass or RegisterClassEx, provided that the module that registers the class is also the module that creates the window. The class name can also be any of the predefined system class names.</param>
    /// <param name="title">The window name. If the window style specifies a title bar, the window title pointed to by lpWindowName is displayed in the title bar. When using CreateWindow to create controls, such as buttons, check boxes, and static controls, use lpWindowName to specify the text of the control. When creating a static control with the SS_ICON style, use lpWindowName to specify the icon name or identifier. To specify an identifier, use the syntax "#num".</param>
    /// <param name="style">The style of the window being created. This parameter can be a combination of the window style values, plus the control styles indicated in the Remarks section.</param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="parentHandle">A handle to the parent or owner window of the window being created. To create a child window or an owned window, supply a valid window handle. This parameter is optional for pop-up windows.To create a message-only window, supply HWND_MESSAGE or a handle to an existing message-only window.</param>
    /// <param name="menu">A handle to a menu, or specifies a child-window identifier, depending on the window style. For an overlapped or pop-up window, hMenu identifies the menu to be used with the window; it can be NULL if the class menu is to be used. For a child window, hMenu specifies the child-window identifier, an integer value used by a dialog box control to notify its parent about events. The application determines the child-window identifier; it must be unique for all child windows with the same parent window.</param>
    /// <param name="instance">A handle to the instance of the module to be associated with the window.</param>
    /// <param name="param">Pointer to a value to be passed to the window through the <seealso cref="CreateStruct"/> structure (lpCreateParams member) pointed to by the <paramref name="param"/> of the WM_CREATE message. 
    /// This message is sent to the created window by this function before it returns. 
    /// If an application calls CreateWindow to create a MDI client window, <paramref name="param"/> should point to a CLIENTCREATESTRUCT structure. 
    /// If an MDI client window calls this to create an MDI child window, <paramref name="param"/> should point to an MDICREATESTRUCT structure. 
    /// <paramref name="param"/> may be NULL if no additional data is needed.</param>
    /// <returns></returns>
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

    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    public static extern int ShowCursor ([In, MarshalAs(UnmanagedType.Bool)] bool show);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UpdateWindow (IntPtr handle);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    public static extern nint GetMessageW (ref Message m, nint handle, uint min, uint max);

    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool PeekMessageW (ref Message m, nint handle, uint min, uint max, PeekRemove remove);

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
    public static extern bool InvalidateRect ([In] IntPtr handle, [In] ref Rect rect, nint erase);

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

    public static IntPtr CreateWindow (ushort atom, Vector2i size, IntPtr? moduleHandle = null) => 
        User.CreateWindowExW(WindowStyleEx.Composited, new(atom), IntPtr.Zero, WindowStyle.ClipPopup, 0, 0, size.X, size.Y, IntPtr.Zero, IntPtr.Zero, moduleHandle ?? Kernel.GetModuleHandleW(null), IntPtr.Zero);

}
