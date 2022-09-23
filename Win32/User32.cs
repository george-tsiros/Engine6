namespace Win32;

using System.Runtime.InteropServices;
using System;
using Common;

public delegate nint WndProc (nint hWnd, WinMessage msg, nuint wparam, nint lparam);

public static class User32 {

    private const string dll = nameof(User32) + ".dll";

    [DllImport(dll, SetLastError = true)]
    internal static extern nint GetDC (nint windowHandle);

    [DllImport(dll, EntryPoint = "LoadCursorW", ExactSpelling = true, SetLastError = true)]
    private static extern nint LoadCursor (nint moduleHandle, nint cursor);

    [DllImport(dll)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool ReleaseDC (nint windowHandle, nint dc);

    [DllImport(dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect (nint windowHandle, ref Rectangle rect);

    [DllImport(dll, EntryPoint = "SetCursorPos", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetCursorPos_ (int x, int y);

    [DllImport(dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ScreenToClient (nint windowHandle, [In, Out] ref Vector2i position);

    [DllImport(dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ClientToScreen (nint windowHandle, [In, Out] ref Vector2i position);


    [DllImport(dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static unsafe extern bool RegisterRawInputDevices (RawInputDevice* devices, uint count, uint structSize);

    [DllImport(dll)]
    private static unsafe extern int GetRawInputData (nint rawInput, uint command, void* data, uint* size, uint headerSize);

    [DllImport(dll, EntryPoint = "RegisterClassW", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern ushort RegisterClassW_ (ref WindowClassW windowClass);


    [DllImport(dll, EntryPoint = "DestroyWindow", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow_ (nint windowHandle);


    [DllImport(dll, EntryPoint = "CreateWindowExW", ExactSpelling = true, SetLastError = true)]
    private static extern nint CreateWindowEx (WindowStyleEx exStyle, nint classNameOrAtom, nint title, WindowStyle style, int x, int y, int width, int height, nint parentHandle, nint menu, nint instance, nint param);

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

    [DllImport(dll, SetLastError = true)]
    private static extern int GetMessageW (ref Message m, nint handle, uint min, uint max);

    [DllImport(dll, EntryPoint = "PeekMessageW", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool PeekMessage (ref Message m, nint handle, uint min, uint max, PeekRemove remove);

    [DllImport(dll, EntryPoint = "DispatchMessageW", ExactSpelling = true)]
    public static extern nint DispatchMessage (ref Message m);


    [DllImport(dll, EntryPoint = "MoveWindow", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool MoveWindow_ (nint windowHandle, int x, int y, int w, int h, bool repaint);

    [DllImport(dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetClientRect (nint handle, ref Rectangle clientRect);

    public static Vector2i GetClientAreaSize (Window window) {
        Rectangle r = new();
        if (GetClientRect(window.Handle, ref r))
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


    public static nint LoadCursor (SystemCursor cursor) {
        var ptr = LoadCursor(0, (nint)cursor);
        return 0 != ptr ? ptr : throw new WinApiException(nameof(LoadCursor));
    }

    public static Rectangle GetWindowRect (Window window) {
        Rectangle r = new();
        return GetWindowRect(window.Handle, ref r) ? r : throw new WinApiException(nameof(GetWindowRect));
    }

    public static void SetCursorPos (int x, int y) {
        if (!SetCursorPos_(x, y))
            throw new WinApiException(nameof(SetCursorPos));
    }

    public static Vector2i ScreenToClient (nint windowHandle, Vector2i point) =>
        ScreenToClient(windowHandle, ref point) ? point : throw new WinApiException(nameof(ScreenToClient));

    public static unsafe void RegisterMouseRaw (Window window) {
        RawInputDevice device = new() {
            flags = window is null ? RawInputDeviceFlag.Remove : RawInputDeviceFlag.InputSink,
            target = window?.Handle ?? 0,
            usagePage = 1,
            usage = 2,
        };
        if (!RegisterRawInputDevices(&device, 1, (uint)RawInputDevice.Size))
            throw new WinApiException(nameof(RegisterRawInputDevices));
    }

    public static Vector2i ClientToScreen (Window window, Vector2i point) =>
        ClientToScreen(window.Handle, ref point) ? point : throw new WinApiException(nameof(ClientToScreen));

    public static unsafe bool GetRawInputData (nint lParameter, ref RawMouse data) {
        const uint RIM_TYPEMOUSE = 0u;
        RawInput rawData = new();
        var size = (uint)RawInput.Size;
        var x = GetRawInputData(lParameter, 0x10000003, &rawData, &size, (uint)RawInputHeader.Size);
        if (-1 == x)
            throw new WinApiException(nameof(GetRawInputData));
        if (RIM_TYPEMOUSE != rawData.header.type)
            return false;
        data = rawData.mouse;
        return true;
    }

    public static ushort RegisterClass (ref WindowClassW windowClass) {
        var atom = RegisterClassW_(ref windowClass);
        return atom != 0 ? atom : throw new WinApiException(nameof(RegisterClassW_));
    }

    public static void DestroyWindow (Window window) {
        if (!DestroyWindow_(window.Handle))
            throw new WinApiException(nameof(DestroyWindow));
    }

    public static void UpdateWindow (Window window) {
        if (!UpdateWindow_(window.Handle))
            throw new WinApiException(nameof(UpdateWindow));
    }

    public static void MoveWindow (Window window, int x, int y, int w, int h, bool repaint) {
        if (!MoveWindow_(window.Handle, x, y, w, h, repaint))
            throw new WinApiException(nameof(MoveWindow));
    }

    public unsafe static void InvalidateRect (Window window, in Rectangle rect, bool erase) {
        fixed (Rectangle* r = &rect)
            if (!InvalidateRect(window.Handle, r, erase ? 1 : 0))
                throw new Exception(nameof(User32.InvalidateRect));
    }

    public unsafe static void InvalidateWindow (Window window) {
        if (!InvalidateRect(window.Handle, null, 0))
            throw new Exception(nameof(User32.InvalidateRect));
    }

    public static nint CreateWindow (ushort atom, WindowStyle style = WindowStyle.ClipPopup, WindowStyleEx styleEx = WindowStyleEx.None, Vector2i? size = null, nint? moduleHandle = null) {
        var (w, h) = size is Vector2i s ? (s.X, s.Y) : (640, 480);
        var p = CreateWindowEx(styleEx, (nint)atom, 0, style, 10, 10, w, h, 0, 0, moduleHandle ?? Kernel32.GetModuleHandle(null), 0);
        return 0 != p ? p : throw new WinApiException(nameof(CreateWindowEx));
    }

    public static int GetMessage (ref Message m) =>
        GetMessageW(ref m, 0, 0, 0);
}
