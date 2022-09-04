namespace Win32;

using System.Runtime.InteropServices;
using System;
using System.Text;
using Common;

public static class User32 {
    private const string dll = nameof(User32) + ".dll";

    [DllImport(dll, SetLastError = true)]
    internal static extern IntPtr GetDC (IntPtr windowHandle);

    [DllImport(dll, EntryPoint = "LoadCursorW", ExactSpelling = true, SetLastError = true)]
    private static extern IntPtr LoadCursor (IntPtr moduleHandle, nuint cursor);

    public static IntPtr LoadCursor (SystemCursor cursor) {
        var ptr = LoadCursor(IntPtr.Zero, (nuint)cursor);
        return IntPtr.Zero != ptr ? ptr : throw new WinApiException(nameof(LoadCursor));
    }

    [DllImport(dll)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool ReleaseDC (IntPtr hwnd, IntPtr dc);

    [DllImport(dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect (IntPtr windowHandle, ref Rectangle rect);

    public static Rectangle GetWindowRect (IntPtr hwnd) {
        Rectangle r = new();
        return GetWindowRect(hwnd, ref r) ? r : throw new WinApiException(nameof(GetWindowRect));
    }

    [DllImport(dll, SetLastError = true)]
    public static extern int SetCursorPos (int x, int y);

    [DllImport(dll, SetLastError = true)]
    public static extern IntPtr SetCursor (IntPtr cursor);

    //[DllImport(dll, SetLastError = true)]
    //[return: MarshalAs(UnmanagedType.Bool)]
    //public static extern bool GetCursorPos ([Out] out Vector2i position);

    [DllImport(dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ScreenToClient (IntPtr windowHandle, [In, Out] ref Vector2i position);

    public static Vector2i ScreenToClient (IntPtr hwnd, Vector2i point) =>
        ScreenToClient(hwnd, ref point) ? point : throw new WinApiException(nameof(ScreenToClient));

    [DllImport(dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ClientToScreen (IntPtr windowHandle, [In, Out] ref Vector2i position);

    public static Vector2i ClientToScreen (IntPtr hwnd, Vector2i point) =>
        ClientToScreen(hwnd, ref point) ? point : throw new WinApiException(nameof(ClientToScreen));

    [DllImport(dll, EntryPoint = "TrackMouseEvent", ExactSpelling = true, SetLastError = true)]
    private static extern int TrackMouseEvent_ (ref TrackMouseEvent tme);

    public static void TrackMouseEvent (ref TrackMouseEvent tme) {
        if (0 == TrackMouseEvent_(ref tme))
            throw new WinApiException(nameof(TrackMouseEvent));
    }

    //[DllImport(dll, SetLastError = true)]
    //public static extern int RegisterRawInputDevices (ref RawInputDevice raw, uint deviceCount, uint structSize);

    [DllImport(dll, EntryPoint = "RegisterClassExW", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern ushort RegisterClassEx (ref WindowClassExW windowClass);

    public static ushort RegisterClass (ref WindowClassExW windowClass) {
        var atom = RegisterClassEx(ref windowClass);
        return atom != 0 ? atom : throw new WinApiException(nameof(RegisterClassEx));
    }

    [DllImport(dll, EntryPoint = "RegisterClassW", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern ushort RegisterClassW_ (ref WindowClassW windowClass);

    public static ushort RegisterClass (ref WindowClassW windowClass) {
        var atom = RegisterClassW_(ref windowClass);
        return atom != 0 ? atom : throw new WinApiException(nameof(RegisterClassW_));
    }

    //[DllImport(dll, SetLastError = true)]
    //[return: MarshalAs(UnmanagedType.Bool)]
    //public static extern bool UnregisterClassW ([In] IntPtr className, [In, Optional] IntPtr hInstance);

    //[DllImport(dll, SetLastError = true)]
    //[return: MarshalAs(UnmanagedType.Bool)]
    //public static extern bool DestroyCursor ([In] IntPtr cursor);

    [DllImport(dll, EntryPoint = "DestroyWindow", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow_ (IntPtr windowHandle);

    public static void DestroyWindow (IntPtr hwnd) {
        if (!DestroyWindow_(hwnd))
            throw new WinApiException(nameof(DestroyWindow));
    }

    //[DllImport(dll, SetLastError = true)]
    //public static extern IntPtr LoadImageA (IntPtr instance, [MarshalAs(UnmanagedType.LPStr)] string name, uint type, int cx, int cy, uint load);

    //[DllImport(dll, SetLastError = true)]
    //unsafe public static extern IntPtr CreateCursor (IntPtr instance, int xHotSpot, int yHotSpot, int width, int height, byte* andPlane, byte* xorPlane);

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
    /// <param name="param">Pointer to a value to be passed to the window through the <seealso cref="CreateStructW"/> structure (lpCreateParams member) pointed to by the <paramref name="param"/> of the WM_CREATE message. 
    /// This message is sent to the created window by this function before it returns. 
    /// If an application calls CreateWindow to create a MDI client window, <paramref name="param"/> should point to a CLIENTCREATESTRUCT structure. 
    /// If an MDI client window calls this to create an MDI child window, <paramref name="param"/> should point to an MDICREATESTRUCT structure. 
    /// <paramref name="param"/> may be NULL if no additional data is needed.</param>
    /// <returns></returns>
    [DllImport(dll, EntryPoint = "CreateWindowExW", ExactSpelling = true, SetLastError = true)]
    private static extern IntPtr CreateWindowEx (WindowStyleEx exStyle, IntPtr classNameOrAtom, IntPtr title, WindowStyle style, int x, int y, int width, int height, IntPtr parentHandle, IntPtr menu, IntPtr instance, IntPtr param);

    public static IntPtr CreateWindow (ushort atom, int width = 0, int height = 0, IntPtr? moduleHandle = null, WindowStyle style = WindowStyle.ClipPopup, WindowStyleEx styleEx = WindowStyleEx.None) {
        var p = CreateWindowEx(styleEx, new(atom), IntPtr.Zero, style, 100, 100, width, height, IntPtr.Zero, IntPtr.Zero, moduleHandle ?? Kernel32.GetModuleHandle(null), IntPtr.Zero);
        return IntPtr.Zero != p ? p : throw new WinApiException(nameof(CreateWindowEx));
    }

    [DllImport(dll, EntryPoint = "DefWindowProcW", ExactSpelling = true, CharSet = CharSet.Unicode)]
    public static extern nint DefWindowProc (IntPtr hWnd, WinMessage msg, nuint wparam, nint lparam);

    [DllImport(dll)]
    public static extern void PostQuitMessage (int code);

    [DllImport(dll)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ShowWindow (IntPtr handle, CmdShow cmdShow);

    [DllImport(dll)]
    public static extern int ShowCursor ([In, MarshalAs(UnmanagedType.Bool)] bool show);

    [DllImport(dll, EntryPoint = "UpdateWindow", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UpdateWindow_ (IntPtr handle);

    public static void UpdateWindow (IntPtr windowHandle) {
        if (!UpdateWindow_(windowHandle))
            throw new WinApiException(nameof(UpdateWindow));
    }

    [DllImport(dll)]
    private static extern int GetMessageW (ref Message m, IntPtr handle, uint min, uint max);

    public static bool GetMessage (ref Message m) =>
        0 != GetMessageW(ref m, IntPtr.Zero, 0, 0);

    [DllImport(dll)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool PeekMessageW (ref Message m, IntPtr handle, uint min, uint max, PeekRemove remove);

    [DllImport(dll)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool TranslateMessage (ref Message m);

    [DllImport(dll, EntryPoint = "DispatchMessageW", ExactSpelling = true)]
    public static extern IntPtr DispatchMessage (ref Message m);

    [DllImport(dll, SetLastError = true)]
    public static extern IntPtr SetCapture (IntPtr windowHandle);

    [DllImport(dll, SetLastError = true)]
    public static extern void ReleaseCapture ();

    [DllImport(dll, SetLastError = true)]
    public static extern IntPtr GetCapture ();

    [DllImport(dll, SetLastError = true)]
    internal static extern int MessageBox (IntPtr hWnd, string text, string caption, uint type);

    [DllImport(dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool MoveWindow (IntPtr windowHandle, int x, int y, int w, int h, bool repaint);

    //[DllImport(dll, SetLastError = true, CharSet = CharSet.Auto)]
    //[return: MarshalAs(UnmanagedType.Bool)]
    //public static extern bool SetWindowText (IntPtr windowHandle, string text);

    //[DllImport(dll, SetLastError = true, CharSet = CharSet.Ansi)]
    //[return: MarshalAs(UnmanagedType.Bool)]
    //public static extern bool PostMessageA (IntPtr handle, uint msg, IntPtr w, IntPtr l);

    [DllImport(dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetClientRect (IntPtr handle, ref Rectangle clientRect);

    public static Rectangle GetClientRect (IntPtr hwnd) {
        Rectangle r = new();
        return GetClientRect(hwnd, ref r) ? r : throw new WinApiException(nameof(GetClientRect));
    }

    [DllImport(dll)]
    public static extern IntPtr BeginPaint (IntPtr hWnd, [In, Out] ref PaintStruct paint);

    [DllImport(dll)]
    public static extern void EndPaint (IntPtr hWnd, [In] ref PaintStruct paint);

    [DllImport(dll)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private unsafe static extern bool InvalidateRect (IntPtr handle, Rectangle* rect, nint erase);

    public unsafe static void InvalidateRect (IntPtr handle, [In] in Rectangle rect, bool erase) {
        fixed (Rectangle* r = &rect)
            if (!InvalidateRect(handle, r, erase ? 1 : 0))
                throw new Exception(nameof(User32.InvalidateRect));
    }

    public unsafe static void InvalidateWindow (IntPtr windowHandle) {
        if (!InvalidateRect(windowHandle, null, 0))
            throw new Exception(nameof(User32.InvalidateRect));
    }

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

    //[DllImport(dll, SetLastError = true)]
    //unsafe public static extern int GetRawInputDeviceInfoW (IntPtr device, RawInputDeviceCommand command, void* data, uint* size);

    /// <param name="devices">An array of <see cref="RawInputDeviceList"/> structures for the devices attached to the system.If null, the number of devices are returned in <paramref name="count"/>.</param>
    /// <param name="count">If <paramref name="devices"/> is null, the function populates this variable with the number of devices attached to the system; otherwise, this variable specifies the number of <see cref="RawInputDeviceList"/> structures that can be contained in the buffer to which <paramref name="devices"/> points. If this value is less than the number of devices attached to the system, the function returns the actual number of devices in this variable and fails with ERROR_INSUFFICIENT_BUFFER.</param>
    /// <param name="size">The size of a <see cref="RawInputDeviceList"/> structure, in bytes.</param>
    /// <returns>If the function is successful, the return value is the number of devices stored in the buffer pointed to by <paramref name="devices"/>. On any other error, the function returns (UINT) -1 and GetLastError returns the error indication.</returns>
    /// <remarks>The devices returned from this function are the mouse, the keyboard, and other Human Interface Device (HID) devices.To get more detailed information about the attached devices, call GetRawInputDeviceInfo using the <see cref="RawInputDeviceList.device"/> from <see cref="RawInputDeviceList"/>.</remarks>

    //[DllImport(dll, SetLastError = true)]
    //public static extern uint GetRawInputDeviceList (ref RawInputDeviceList devices, ref uint count, uint size);

    //[DllImport(dll, SetLastError = true)]
    //public static extern IntPtr GetWindowLongPtrA (IntPtr hWnd, int nIndex);

    [DllImport(dll, EntryPoint = "SetWindowPos", ExactSpelling = true, SetLastError = true)]
    private static extern bool SetWindowPos_ (IntPtr window, IntPtr after, int x, int y, int w, int h, uint flags);

    public static void SetWindowPos (IntPtr window, IntPtr after, int x, int y, int w, int h, WindowPosFlags flags)
        => SetWindowPos_(window, after, x, y, w, h, (uint)flags);

    [DllImport(dll, SetLastError = true)]
    private static extern IntPtr SetWindowLongPtrW (IntPtr windowHandle, int index, IntPtr value);

    private static void SetWindow (IntPtr windowHandle, SetWindowParameter index, IntPtr value) {
        var before = Kernel32.GetLastError();
        if (0 != before)
            throw new WinApiException("pre-existing error", before);
        var previousValue = SetWindowLongPtrW(windowHandle, (int)index, value);
        if (IntPtr.Zero == previousValue) {
            var after = Kernel32.GetLastError();
            if (0 != after)
                throw new WinApiException($"{nameof(SetWindowLongPtrW)} failed", after);
        }
    }

    public static void SetWindow (IntPtr windowHandle, WndProc proc) =>
        SetWindow(windowHandle, SetWindowParameter.WndProc, Marshal.GetFunctionPointerForDelegate(proc));

    public static void SetWindow (IntPtr windowHandle, WindowStyleEx style) =>
        SetWindow(windowHandle, SetWindowParameter.ExStyle, (IntPtr)style);

    public static void SetWindow (IntPtr windowHandle, WindowStyle style) =>
        SetWindow(windowHandle, SetWindowParameter.Style, (IntPtr)style);
}
