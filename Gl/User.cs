namespace Gl;

using System.Runtime.InteropServices;
using System;

internal static class User {
    private const string user32 = nameof(user32) + ".dll";
    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    unsafe internal static extern ushort RegisterClassExW (ref WindowClassExW windowClass);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    unsafe internal static extern int UnregisterClassW ([MarshalAs(UnmanagedType.LPWStr)] string className, IntPtr hInstance);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    unsafe internal static extern bool UnregisterClassW (IntPtr className, IntPtr hInstance);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    internal static extern bool DestroyWindow (IntPtr windowHandle);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    internal static extern IntPtr CreateWindowExW (int exStyle, [MarshalAs(UnmanagedType.LPWStr)] string className, [MarshalAs(UnmanagedType.LPWStr)] string title, int style, int x, int y, int width, int height, IntPtr parentHandle, IntPtr menu, IntPtr instance, IntPtr param);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    internal static extern IntPtr GetDC (IntPtr windowHandle);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    internal static extern IntPtr DefWindowProcW (IntPtr hWnd, WinMessage msg, IntPtr wparam, IntPtr lparam);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    internal static extern bool ReleaseDC (IntPtr hwnd, IntPtr dc);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    internal static extern void PostQuitMessage (int code);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    internal static extern bool ShowWindow (IntPtr handle, int cmdShow);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    internal static extern bool UpdateWindow (IntPtr handle);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    internal static extern bool GetMessageW (ref Message m, IntPtr handle, uint min, uint max);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    internal static extern bool TranslateMessage (ref Message m);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    internal static extern IntPtr DispatchMessageW (ref Message m);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern IntPtr SetCapture (IntPtr windowHandle);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern void ReleaseCapture ();
    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern IntPtr GetCapture ();
    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    internal static extern int MessageBox (IntPtr hWnd, string text, string caption, uint type);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool SetWindowText (IntPtr windowHandle, string text);
}
