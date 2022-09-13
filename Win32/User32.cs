namespace Win32;

using System.Runtime.InteropServices;
using System;
using System.Text;
using Common;

public static class User32 {
    private const string dll = nameof(User32) + ".dll";

    [DllImport(dll, SetLastError = true)]
    internal static extern nint GetDC (nint windowHandle);

    [DllImport(dll, EntryPoint = "LoadCursorW", ExactSpelling = true, SetLastError = true)]
    private static extern nint LoadCursor (nint moduleHandle, nint cursor);

    public static nint LoadCursor (SystemCursor cursor) {
        var ptr = LoadCursor(0, (nint)cursor);
        return 0 != ptr ? ptr : throw new WinApiException(nameof(LoadCursor));
    }

    [DllImport(dll)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool ReleaseDC (nint hwnd, nint dc);

    [DllImport(dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect (nint windowHandle, ref Rectangle rect);

    public static Rectangle GetWindowRect (nint hwnd) {
        Rectangle r = new();
        return GetWindowRect(hwnd, ref r) ? r : throw new WinApiException(nameof(GetWindowRect));
    }

    [DllImport(dll, EntryPoint = "SetCursorPos", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetCursorPos_ (int x, int y);

    public static void SetCursorPos (int x, int y) {
        if (!SetCursorPos_(x, y))
            throw new WinApiException(nameof(SetCursorPos));
    }

    [DllImport(dll, SetLastError = true)]
    public static extern nint SetCursor (nint cursor);

    //[DllImport(dll, SetLastError = true)]
    //[return: MarshalAs(UnmanagedType.Bool)]
    //public static extern bool GetCursorPos ([Out] out Vector2i position);

    [DllImport(dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ScreenToClient (nint windowHandle, [In, Out] ref Vector2i position);

    public static Vector2i ScreenToClient (nint hwnd, Vector2i point) =>
        ScreenToClient(hwnd, ref point) ? point : throw new WinApiException(nameof(ScreenToClient));

    [DllImport(dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ClientToScreen (nint windowHandle, [In, Out] ref Vector2i position);

    public static Vector2i ClientToScreen (nint hwnd, Vector2i point) =>
        ClientToScreen(hwnd, ref point) ? point : throw new WinApiException(nameof(ClientToScreen));

    [DllImport(dll, EntryPoint = "TrackMouseEvent", ExactSpelling = true, SetLastError = true)]
    private static extern int TrackMouseEvent_ (ref TrackMouseEvent tme);

    public static void TrackMouseEvent (ref TrackMouseEvent tme) {
        if (0 == TrackMouseEvent_(ref tme))
            throw new WinApiException(nameof(TrackMouseEvent));
    }

    [DllImport(dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static unsafe extern bool RegisterRawInputDevices (RawInputDevice* devices, uint count, uint structSize);

    public static unsafe void UnregisterMouseRaw () {
        var device = new RawInputDevice {
            flags = RawInputDeviceFlag.Remove,
            target = 0,
            usagePage = 1,
            usage = 2,
        };
        if (!RegisterRawInputDevices(&device, 1, (uint)RawInputDevice.Size))
            throw new WinApiException(nameof(RegisterRawInputDevices));
    }

    public static unsafe void RegisterMouseRaw (nint windowHandle) {
        var device = new RawInputDevice {
            flags = RawInputDeviceFlag.InputSink,
            target = windowHandle,
            usagePage = 1,
            usage = 2,
        };
        if (!RegisterRawInputDevices(&device, 1, (uint)RawInputDevice.Size))
            throw new WinApiException(nameof(RegisterRawInputDevices));
    }

    [DllImport(dll)]
    private static unsafe extern int GetRawInputData (nint rawInput, uint command, void* data, uint* size, uint headerSize);

    public static unsafe bool GetRawInputData (nint lParameter, ref RawMouse data) {
        const uint RIM_TYPEMOUSE = 0u;
        var rawData = new RawInput();
        var size = (uint)RawInput.Size;
        var x = GetRawInputData(lParameter, 0x10000003, &rawData, &size, (uint)RawInputHeader.Size);
        if (-1 == x)
            throw new WinApiException(nameof(GetRawInputData));
        if (RIM_TYPEMOUSE != rawData.header.type)
            return false;
        data = rawData.mouse;
        return true;
    }
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
    //public static extern bool UnregisterClassW ([In] nint className, [In, Optional] nint hInstance);

    //[DllImport(dll, SetLastError = true)]
    //[return: MarshalAs(UnmanagedType.Bool)]
    //public static extern bool DestroyCursor ([In] nint cursor);

    [DllImport(dll, EntryPoint = "DestroyWindow", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow_ (nint windowHandle);

    public static void DestroyWindow (nint hwnd) {
        if (!DestroyWindow_(hwnd))
            throw new WinApiException(nameof(DestroyWindow));
    }

    //[DllImport(dll, SetLastError = true)]
    //public static extern nint LoadImageA (nint instance, [MarshalAs(UnmanagedType.LPStr)] string name, uint type, int cx, int cy, uint load);

    //[DllImport(dll, SetLastError = true)]
    //unsafe public static extern nint CreateCursor (nint instance, int xHotSpot, int yHotSpot, int width, int height, byte* andPlane, byte* xorPlane);

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
    private static extern nint CreateWindowEx (WindowStyleEx exStyle, nint classNameOrAtom, nint title, WindowStyle style, int x, int y, int width, int height, nint parentHandle, nint menu, nint instance, nint param);
    private const int CwDefault = unchecked((int)0x80000000);
    public static nint CreateWindow (ushort atom, WindowStyle style = WindowStyle.ClipPopup, WindowStyleEx styleEx = WindowStyleEx.None, Vector2i? size = null, nint? moduleHandle = null) {
        var (w, h) = size is Vector2i s ? (s.X, s.Y) : (640, 480);
        var p = CreateWindowEx(styleEx, new(atom), 0, style, 10, 10, w, h, 0, 0, moduleHandle ?? Kernel32.GetModuleHandle(null), 0);
        return 0 != p ? p : throw new WinApiException(nameof(CreateWindowEx));
    }

    [DllImport(dll, EntryPoint = "DefWindowProcW", ExactSpelling = true, CharSet = CharSet.Unicode)]
    public static extern nint DefWindowProc (nint hWnd, WinMessage msg, nuint wparam, nint lparam);

    [DllImport(dll)]
    public static extern void PostQuitMessage (int code);

    [DllImport(dll)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ShowWindow (nint handle, CmdShow cmdShow);

    [DllImport(dll)]
    public static extern int ShowCursor ([In, MarshalAs(UnmanagedType.Bool)] bool show);

    [DllImport(dll, EntryPoint = "UpdateWindow", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UpdateWindow_ (nint handle);

    public static void UpdateWindow (nint windowHandle) {
        if (!UpdateWindow_(windowHandle))
            throw new WinApiException(nameof(UpdateWindow));
    }

    [DllImport(dll)]
    private static extern int GetMessageW (ref Message m, nint handle, uint min, uint max);

    public static bool GetMessage (ref Message m) =>
        0 != GetMessageW(ref m, 0, 0, 0);

    [DllImport(dll, EntryPoint = "PeekMessageW", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool PeekMessage (ref Message m, nint handle, uint min, uint max, PeekRemove remove);

    [DllImport(dll)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool TranslateMessage (ref Message m);

    [DllImport(dll, EntryPoint = "DispatchMessageW", ExactSpelling = true)]
    public static extern nint DispatchMessage (ref Message m);

    [DllImport(dll, SetLastError = true)]
    public static extern nint SetCapture (nint windowHandle);

    [DllImport(dll, SetLastError = true)]
    public static extern void ReleaseCapture ();

    [DllImport(dll, SetLastError = true)]
    public static extern nint GetCapture ();

    [DllImport(dll, SetLastError = true)]
    internal static extern int MessageBox (nint hWnd, string text, string caption, uint type);

    [DllImport(dll, EntryPoint = "MoveWindow", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool MoveWindow_ (nint windowHandle, int x, int y, int w, int h, bool repaint);

    public static void MoveWindow (nint windowHandle, int x, int y, int w, int h, bool repaint) {
        if (!MoveWindow_(windowHandle, x, y, w, h, repaint))
            throw new WinApiException(nameof(MoveWindow));
    }

    [DllImport(dll, SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowTextW (nint windowHandle, string text);

    //[DllImport(dll, SetLastError = true, CharSet = CharSet.Ansi)]
    //[return: MarshalAs(UnmanagedType.Bool)]
    //public static extern bool PostMessageA (nint handle, uint msg, nint w, nint l);

    [DllImport(dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetClientRect (nint handle, ref Rectangle clientRect);

    public static Vector2i GetClientAreaSize (nint hwnd) {
        Rectangle r = new();
        if (GetClientRect(hwnd, ref r))
            return r.Size;
        throw new WinApiException(nameof(GetClientRect));
    }

    [DllImport(dll)]
    public static extern nint BeginPaint (nint hWnd, [In, Out] ref PaintStruct paint);

    [DllImport(dll)]
    public static extern void EndPaint (nint hWnd, [In] ref PaintStruct paint);

    [DllImport(dll)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private unsafe static extern bool InvalidateRect (nint handle, Rectangle* rect, nint erase);

    public unsafe static void InvalidateRect (nint handle, in Rectangle rect, bool erase) {
        fixed (Rectangle* r = &rect)
            if (!InvalidateRect(handle, r, erase ? 1 : 0))
                throw new Exception(nameof(User32.InvalidateRect));
    }

    public unsafe static void InvalidateWindow (nint windowHandle) {
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
    //unsafe public static extern int GetRawInputDeviceInfoW (nint device, RawInputDeviceCommand command, void* data, uint* size);

    /// <param name="devices">An array of <see cref="RawInputDeviceList"/> structures for the devices attached to the system.If null, the number of devices are returned in <paramref name="count"/>.</param>
    /// <param name="count">If <paramref name="devices"/> is null, the function populates this variable with the number of devices attached to the system; otherwise, this variable specifies the number of <see cref="RawInputDeviceList"/> structures that can be contained in the buffer to which <paramref name="devices"/> points. If this value is less than the number of devices attached to the system, the function returns the actual number of devices in this variable and fails with ERROR_INSUFFICIENT_BUFFER.</param>
    /// <param name="size">The size of a <see cref="RawInputDeviceList"/> structure, in bytes.</param>
    /// <returns>If the function is successful, the return value is the number of devices stored in the buffer pointed to by <paramref name="devices"/>. On any other error, the function returns (UINT) -1 and GetLastError returns the error indication.</returns>
    /// <remarks>The devices returned from this function are the mouse, the keyboard, and other Human Interface Device (HID) devices.To get more detailed information about the attached devices, call GetRawInputDeviceInfo using the <see cref="RawInputDeviceList.device"/> from <see cref="RawInputDeviceList"/>.</remarks>

    //[DllImport(dll, SetLastError = true)]
    //public static extern uint GetRawInputDeviceList (ref RawInputDeviceList devices, ref uint count, uint size);

    //[DllImport(dll, SetLastError = true)]
    //public static extern nint GetWindowLongPtrA (nint hWnd, int nIndex);

    [DllImport(dll, SetLastError = true)]
    private static extern nint SetWindowLongPtrW (nint windowHandle, int index, nint value);

    private static void SetWindow (nint windowHandle, SetWindowParameter index, nint value) {
        var before = Kernel32.GetLastError();
        if (0 != before)
            throw new WinApiException("pre-existing error", before);
        var previousValue = SetWindowLongPtrW(windowHandle, (int)index, value);
        if (0 == previousValue) {
            var after = Kernel32.GetLastError();
            if (0 != after)
                throw new WinApiException($"{nameof(SetWindowLongPtrW)} failed", after);
        }
    }

    //public static void SetWindow (nint windowHandle, WndProc proc) =>
    //    SetWindow(windowHandle, SetWindowParameter.WndProc, Marshal.GetFunctionPointerForDelegate(proc));

    public static void SetWindow (nint windowHandle, WindowStyleEx style) =>
        SetWindow(windowHandle, SetWindowParameter.ExStyle, (nint)style);

    public static void SetWindow (nint windowHandle, WindowStyle style) =>
        SetWindow(windowHandle, SetWindowParameter.Style, (nint)style);
}
