namespace Gl;

using System.Runtime.InteropServices;
using System;
using Win32;
public static class User {
    private const string user32 = nameof(user32) + ".dll";
    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    unsafe public static extern ushort RegisterClassExW (ref WindowClassExW windowClass);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    unsafe public static extern bool UnregisterClassW ([MarshalAs(UnmanagedType.LPWStr)] string className, IntPtr hInstance);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    unsafe public static extern bool UnregisterClassW (IntPtr className, IntPtr hInstance);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool DestroyWindow (IntPtr windowHandle);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern IntPtr CreateWindowExW (uint exStyle, [MarshalAs(UnmanagedType.LPWStr)] string className, [MarshalAs(UnmanagedType.LPWStr)] string title, uint style, int x, int y, int width, int height, IntPtr parentHandle, IntPtr menu, IntPtr instance, IntPtr param);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    internal static extern IntPtr GetDC (IntPtr windowHandle);
    [DllImport(user32)]
    public static extern IntPtr DefWindowProcW (IntPtr hWnd, WinMessage msg, IntPtr wparam, IntPtr lparam);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool ReleaseDC (IntPtr hwnd, IntPtr dc);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    internal static extern void PostQuitMessage (int code);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ShowWindow (IntPtr handle, int cmdShow);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool UpdateWindow (IntPtr handle);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    internal static extern IntPtr GetMessageW (ref Message m, IntPtr handle, uint min, uint max);
    [DllImport(user32, CallingConvention = CallingConvention.Winapi)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool PeekMessageW (ref Message m, IntPtr handle, uint min, uint max, uint remove);
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
}
