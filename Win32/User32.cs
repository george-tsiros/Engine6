namespace Win32;
using System.Runtime.InteropServices;
using System;
using Common;
using System.Diagnostics;

public delegate nint WndProc (nint hWnd, WinMessage msg, nuint wparam, nint lparam);
unsafe public delegate int MonitorEnumProc (nint monitorHandle, nint dc, Rectangle* rect, nint parameter);


public static class User32 {
    private const string dll = nameof(User32) + ".dll";

    [DllImport(dll, EntryPoint = "GetSystemMetrics", ExactSpelling = true, SetLastError = true)]
    internal static extern int GetSystemMetrics_ (int metric);

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

    [DllImport(dll, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnregisterClassW (string className, nint instance);

    [DllImport(dll, EntryPoint = "RegisterClassW", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern ushort RegisterClassW_ (ref WindowClassW windowClass);

    [DllImport(dll, EntryPoint = "DestroyWindow", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow_ (nint windowHandle);

    [DllImport(dll, EntryPoint = "CreateWindowExW", ExactSpelling = true, SetLastError = true)]
    private static extern nint CreateWindowExW_ (WindowStyleEx exStyle, nint classNameOrAtom, nint title, WindowStyle style, int x, int y, int width, int height, nint parentHandle, nint menu, nint instance, nint param);

    [DllImport(dll, EntryPoint = "DefWindowProcW", ExactSpelling = true, CharSet = CharSet.Unicode)]
    internal static extern nint DefWindowProc (nint hWnd, WinMessage msg, nuint wparam, nint lparam);

    [DllImport(dll)]
    public static extern void PostQuitMessage (int code);

    [DllImport(dll)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool ShowWindow (nint handle, CmdShow cmdShow);

    [DllImport(dll)]
    public static extern int ShowCursor ([In, MarshalAs(UnmanagedType.Bool)] bool show);

    [DllImport(dll, EntryPoint = "UpdateWindow", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UpdateWindow_ (nint handle);

    [DllImport(dll, SetLastError = true)]
    private static extern int GetMessageW (ref Message m, nint handle, uint min, uint max);

    [DllImport(dll, EntryPoint = "PeekMessageW", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool PeekMessage (ref Message m, nint handle, uint min, uint max, PeekRemove remove);

    [DllImport(dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool TranslateMessageW (ref Message m);

    [DllImport(dll, EntryPoint = "DispatchMessageW", ExactSpelling = true)]
    internal static extern nint DispatchMessage (ref Message m);

    [DllImport(dll, EntryPoint = "MoveWindow", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool MoveWindow_ (nint windowHandle, int x, int y, int w, int h, bool repaint);

    [DllImport(dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetClientRect (nint handle, ref Rectangle clientRect);

    [DllImport(dll)]
    internal static extern nint BeginPaint (nint hWnd, [In, Out] ref PaintStruct paint);

    [DllImport(dll)]
    internal static extern void EndPaint (nint hWnd, [In] ref PaintStruct paint);

    [DllImport(dll)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private unsafe static extern bool InvalidateRect (nint handle, Rectangle* rect, nint erase);

    [DllImport(dll)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private unsafe static extern bool GetMonitorInfoA (nint monitorHandle, MonitorInfoExA* p);

    [DllImport(dll)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private unsafe static extern bool EnumDisplaySettingsA (byte* deviceName, int modeNumber, DeviceModeA* deviceMode);

    [DllImport(dll, EntryPoint = "EnumDisplayMonitors", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private unsafe static extern bool EnumDisplayMonitors_ (nint dc, Rectangle* rect, MonitorEnumProc callback, nint param);

    [DllImport(dll, SetLastError = true)]
    private static extern nint GetWindowLongPtrA (nint windowHandle, int index);

    [DllImport(dll, SetLastError = true)]
    private static extern nint SetWindowLongPtrA (nint windowHandle, int index, nint value);

    [DllImport(dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos (nint windowHandle, nint insertAfter, int x, int y, int w, int h, uint flags);

    public static void SetWindowPos (Window window, WindowPosFlags flags) =>
        SetWindowPos(window, 0, 0, 0, 0, flags | WindowPosFlags.NoMove | WindowPosFlags.NoSize | WindowPosFlags.NoZorder | WindowPosFlags.DrawFrame);

    public static void SetWindowPos (Window window, int x, int y, int w, int h, WindowPosFlags flags) {
        if (!SetWindowPos(window.Handle, 0, x, y, w, h, (uint)flags))
            throw new WinApiException(nameof(SetWindowPos));
    }

    private static nint SetWindow (nint windowHandle, int index, nint value) {
        var lastError = Kernel32.GetLastError();
        Debug.Assert(0 == lastError);
        var previousValue = SetWindowLongPtrA(windowHandle, index, value);
        if (0 == previousValue) {
            var possibleError = Kernel32.GetLastError();
            if (0 != possibleError)
                throw new WinApiException(nameof(SetWindowLongPtrA), possibleError);
        }
        return previousValue;
    }

    public static WindowStyleEx SetWindowStyleEx (Window window, WindowStyleEx style) =>
        (WindowStyleEx)SetWindow(window.Handle, -20, (nint)style);

    public static WindowStyle SetWindowStyle (Window window, WindowStyle style) =>
        (WindowStyle)SetWindow(window.Handle, -16, (nint)style);

    public static WindowStyleEx GetWindowStyleEx (Window window) =>
        (WindowStyleEx)GetWindowLongPtrA(window.Handle, -20);

    public static WindowStyle GetWindowStyle (Window window) =>
        (WindowStyle)GetWindowLongPtrA(window.Handle, -16);

    public unsafe static bool EnumDisplaySettings (MonitorInfoExA monitor, out DeviceModeA info) {
        info = new();
        fixed (DeviceModeA* p = &info)
            return EnumDisplaySettingsA(monitor.name, -1, p);
    }

    public unsafe static int GetMonitorCount () {
        int monitorCount = 0;

        if (!EnumDisplayMonitors_(0, null, (a, b, c, d) => { ++*(int*)d; return 1; }, (nint)(&monitorCount)))
            throw new WinApiException(nameof(EnumDisplayMonitors));
        return monitorCount;
    }

    public unsafe static MonitorInfoExA[] GetMonitorInfo () {
        var monitorCount = GetMonitorCount();
        if (0 == monitorCount)
            return Array.Empty<MonitorInfoExA>();

        var monitors = new MonitorInfoExA[monitorCount];

        fixed (MonitorInfoExA* p = monitors)
            if (!EnumDisplayMonitors_(0, null, getMonitorInfoEx, (nint)(&p)))
                throw new WinApiException(nameof(EnumDisplayMonitors));

        return monitors;
    }
    //monitorHandle, nint dc, Rectangle* rect, nint parameter
    private unsafe static readonly MonitorEnumProc getMonitorInfoEx = (monitorHandle, dc, rectPtr, parameter) => {

        MonitorInfoExA** pp = (MonitorInfoExA**)parameter;
        // *pp is a MonitorInfoExA*
        // **pp is a MonitorInfoExA
        (*pp)->size = MonitorInfoExA.Size;
        if (GetMonitorInfoA(monitorHandle, *pp)) {
            ++*pp; // who said c# has no ptr arithmetic
            return 1;
        }
        return 0;
    };

    public unsafe static bool EnumDisplayMonitors (DeviceContext dc, MonitorEnumProc callback) {
        return EnumDisplayMonitors_((nint)dc, null, callback, 0);
    }

    public static int GetSystemMetrics (SystemMetric metric) {
        var x = GetSystemMetrics_((int)metric);
        return 0 != x ? x : throw new WinApiException(nameof(GetSystemMetrics));
    }

    public static nint LoadCursor (SystemCursor cursor) {
        var ptr = LoadCursor(0, (nint)cursor);
        return 0 != ptr ? ptr : throw new WinApiException(nameof(LoadCursor));
    }

    public static Rectangle GetWindowRect (Window window) {
        Rectangle r = new();
        return GetWindowRect(window.Handle, ref r) ? r : throw new WinApiException(nameof(GetWindowRect));
    }

    public static void SetCursorPos (in Vector2i p) { 
        if (!SetCursorPos_(p.X, p.Y))
            throw new WinApiException(nameof(SetCursorPos));
    }

    public static void SetCursorPos (int x, int y) {
        if (!SetCursorPos_(x, y))
            throw new WinApiException(nameof(SetCursorPos));
    }

    public static Vector2i GetClientAreaSize (Window window) {
        Rectangle r = new();
        if (GetClientRect(window.Handle, ref r))
            return r.Size;
        throw new WinApiException(nameof(GetClientRect));
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

    public static void MoveWindow (Window window, in Rectangle rect, bool repaint = false) {
        if (!MoveWindow_(window.Handle, rect.Left, rect.Top, rect.Width, rect.Height, repaint))
            throw new WinApiException(nameof(MoveWindow));
    }

    public static void MoveWindow (Window window, int x, int y, int w, int h, bool repaint = false) {
        if (!MoveWindow_(window.Handle, x, y, w, h, repaint))
            throw new WinApiException(nameof(MoveWindow));
    }

    public unsafe static void InvalidateRect (Window window, ref Rectangle rect, bool erase) {
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
        var p = CreateWindowExW_(styleEx, (nint)atom, 0, style, 10, 10, w, h, 0, 0, moduleHandle ?? Kernel32.GetModuleHandle(null), 0);
        return 0 != p ? p : throw new WinApiException(nameof(CreateWindowExW_));
    }

    public static int GetMessage (ref Message m) =>
        GetMessageW(ref m, 0, 0, 0);
}
