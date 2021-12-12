namespace Gl;

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Win32;

public class GlWindow:IDisposable {
    protected static void Demand (bool condition) {
        if (!condition)
            throw new Exception();
    }

    protected static void Demand (IntPtr p) => Demand(IntPtr.Zero != p);
    protected const string ClassName = "MYWINDOWCLASS";
    protected ulong FramesRendered { get; private set; }
    public IntPtr DeviceContext { get; private set; }
    public IntPtr RenderingContext { get; private set; }
    public IntPtr WindowHandle { get; private set; }
    protected ushort ClassAtom { get; }
    public int Width { get; }
    public int Height { get; }
    private long lastTicks;
    private bool disposed;
    private IntPtr helperWindow;
    private HashSet<string> extensions = new(StringComparer.OrdinalIgnoreCase);

    public GlWindow (Vector2i size) {
        var windowClass = WindowClassExW.Create();
        windowClass.style = ClassStyle.HRedraw | ClassStyle.VRedraw | ClassStyle.OwnDc;
        windowClass.wndProc = MyProc;
        var nullModuleHandle = Gl.Kernel.GetModuleHandleW(null);
        windowClass.hInstance = nullModuleHandle;
        windowClass.hCursor = IntPtr.Zero;
        windowClass.classname = ClassName;
        ClassAtom = User.RegisterClassExW(ref windowClass);
        Demand(ClassAtom != 0);
        (Width, Height) = size;
        helperWindow = User.CreateWindowExW(0x300, ClassName, "?", WindowStyle.ClipChildren | WindowStyle.ClipSiblings, 0, 0, 1, 1, IntPtr.Zero, IntPtr.Zero, nullModuleHandle, IntPtr.Zero);
        Demand(helperWindow);
        var m = new Message();
        while (User.PeekMessageW(ref m, helperWindow, 0, 0, 1)) {
            _ = User.TranslateMessage(ref m);
            _ = User.DispatchMessageW(ref m);
        }
        WindowHandle = User.CreateWindowExW(0x300, ClassName, "?", WindowStyle.ClipChildren | WindowStyle.ClipSiblings, 0, 0, Width, Height, IntPtr.Zero, IntPtr.Zero, nullModuleHandle, IntPtr.Zero);
        Demand(WindowHandle);
        InitWgl();
        CreateContextWGL();
    }

    private void InitWgl () {
        var dc = Gl.User.GetDC(helperWindow);
        var pfd = PixelFormatDescriptor.Create();
        pfd.Flags = PixelFlags.DrawToWindow | PixelFlags.SupportOpengl | PixelFlags.DoubleBuffer;
        pfd.ClrBts = 24;
        var formatIndex = Gl.Gdi.ChoosePixelFormat(dc, ref pfd);
        Demand(Gl.Gdi.SetPixelFormat(dc, formatIndex, ref pfd));
        var rc = Gl.Opengl.CreateContext(dc);
        Demand(rc);
        var pdc = Gl.Opengl.GetCurrentDC();
        var prc = Gl.Opengl.GetCurrentContext();
        Demand(Gl.Opengl.MakeCurrent(dc, rc));
        var extensionsString = Marshal.PtrToStringAnsi(Gl.Opengl.GetExtensionsString());
        if (string.IsNullOrEmpty(extensionsString))
            throw new Exception();
        extensions.UnionWith(extensionsString.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        var madePCurrent = Gl.Opengl.MakeCurrent(pdc, prc);
        var deleted = Gl.Opengl.DeleteContext(rc);
        Demand(Gl.User.DestroyWindow(helperWindow));

    }
    unsafe private void CreateContextWGL () {
        var pfd = PixelFormatDescriptor.Create();
        DeviceContext = Gl.User.GetDC(WindowHandle);
        Demand(IntPtr.Zero != DeviceContext);
        int pixelFormatIndex = ChoosePixelFormatWGL();
        Demand(0 != pixelFormatIndex);
        Demand(0 != Gl.Gdi.DescribePixelFormat(DeviceContext, pixelFormatIndex, pfd.Ss, &pfd));
        Demand(Gl.Gdi.SetPixelFormat(DeviceContext, pixelFormatIndex, ref pfd));
        Demand(ExtensionSupportedWGL(ExtensionString.ARB_create_context));
        Demand(ExtensionSupportedWGL(ExtensionString.ARB_create_context_profile));
        if (Gl.Opengl.ExtensionsSupported) {
            var attribs = new int[] {
                (int)PixelFormatAttribute.CONTEXT_MAJOR_VERSION_ARB,
                4,
                (int)PixelFormatAttribute.CONTEXT_MINOR_VERSION_ARB,
                6,
                (int)Gl.ContextAttributes.ContextFlags,
                (int)(Gl.ContextFlags.Debug |  Gl.ContextFlags.ForwardCompatible),
                (int)Gl.ContextAttributes.ProfileMask,
                (int)Gl.ProfileMask.Core,
                0,
            };
            RenderingContext = Opengl.CreateContextAttribsARB(DeviceContext, IntPtr.Zero, attribs);
        } else {
            RenderingContext = Opengl.CreateContext(DeviceContext);
        }
        Demand(RenderingContext);
        Demand(Gl.Opengl.MakeCurrent(DeviceContext, RenderingContext));
        Demand(Gl.Opengl.GetCurrentContext() == RenderingContext);
        Gl.State.DebugOutput = true;
    }
    unsafe private static bool GetPixelFormatAttribivARB (IntPtr dc, int pixelFormat, int x, int[] attributes, int[] values) {
        if (attributes.Length != values.Length)
            throw new ArgumentException();
        fixed (int* a = &attributes[0])
        fixed (int* v = &values[0])
            return Opengl.GetPixelFormatAttribivARB(dc.ToPointer(), pixelFormat, 0, (uint)values.Length, a, v);
    }
    private bool ExtensionSupportedWGL (ExtensionString extension) => extensions.Contains($"WGL_{extension}");
    unsafe private int ChoosePixelFormatWGL () {
        var lastPixelFormatIndex = 0;
        if (Gl.Opengl.ExtensionsSupported && ExtensionSupportedWGL(ExtensionString.ARB_pixel_format)) {
            var formatCount = 0;
            int attrib = (int)PixelFormatAttribute.NUMBER_PIXEL_FORMATS_ARB;
            Demand(Gl.Opengl.GetPixelFormatAttribivARB(DeviceContext.ToPointer(), 1, 0, 1, &attrib, &formatCount));
            var attribs = new int[] {
                (int)PixelFormatAttribute.SUPPORT_OPENGL_ARB,
                (int)PixelFormatAttribute.DRAW_TO_WINDOW_ARB,
                (int)PixelFormatAttribute.PIXEL_TYPE_ARB,
                (int)PixelFormatAttribute.ACCELERATION_ARB,
                (int)PixelFormatAttribute.RED_BITS_ARB,
                (int)PixelFormatAttribute.GREEN_BITS_ARB,
                (int)PixelFormatAttribute.BLUE_BITS_ARB,
                (int)PixelFormatAttribute.ALPHA_BITS_ARB,
                (int)PixelFormatAttribute.DEPTH_BITS_ARB,
                (int)PixelFormatAttribute.STEREO_ARB,
                (int)PixelFormatAttribute.DOUBLE_BUFFER_ARB,
            };
            var values = new int[attribs.Length];
            for (var pixelFormat = 1; pixelFormat < formatCount; pixelFormat++) {
                if (!GetPixelFormatAttribivARB(DeviceContext, pixelFormat, 0, attribs, values))
                    continue;
                if (!TryGetValue(attribs, values, PixelFormatAttribute.SUPPORT_OPENGL_ARB, out var ogl) || ogl == 0)
                    continue;
                if (!TryGetValue(attribs, values, PixelFormatAttribute.DRAW_TO_WINDOW_ARB, out var dtw) || dtw == 0)
                    continue;
                if (!TryGetValue(attribs, values, PixelFormatAttribute.PIXEL_TYPE_ARB, out var pt) || pt != 0x202B)
                    continue;
                if (!TryGetValue(attribs, values, PixelFormatAttribute.ACCELERATION_ARB, out var a) || a != 0x2027)
                    continue;
                if (!TryGetValue(attribs, values, PixelFormatAttribute.RED_BITS_ARB, out var rb) || rb != 8)
                    continue;
                if (!TryGetValue(attribs, values, PixelFormatAttribute.GREEN_BITS_ARB, out var gb) || gb != 8)
                    continue;
                if (!TryGetValue(attribs, values, PixelFormatAttribute.BLUE_BITS_ARB, out var bb) || bb != 8)
                    continue;
                if (!TryGetValue(attribs, values, PixelFormatAttribute.DEPTH_BITS_ARB, out var depth) || depth < 24)
                    continue;
                if (!TryGetValue(attribs, values, PixelFormatAttribute.STEREO_ARB, out var st) || st != 0)
                    continue;
                if (!TryGetValue(attribs, values, PixelFormatAttribute.DOUBLE_BUFFER_ARB, out var bf) || bf == 0)
                    continue;
                Console.WriteLine($"rgb {rb}/{gb}/{bb} depth {depth}");
                return pixelFormat;
            }
        } else {
            var pfd = PixelFormatDescriptor.Create();
            var formatCount = Gl.Gdi.DescribePixelFormat(DeviceContext, 1, pfd.Ss, null);
            for (var pixelFormat = 1; pixelFormat <= formatCount; pixelFormat++)
                if (0 != Gl.Gdi.DescribePixelFormat(DeviceContext, pixelFormat, pfd.Ss, &pfd))
                    if (pfd.Flags.HasFlag(PixelFlags.GenericAccelerated) || pfd.Flags.HasFlag(PixelFlags.GenericFormat) && !pfd.Flags.HasFlag(PixelFlags.Stereo) && PixelFormatDescriptor.Typical(pfd))
                        return pixelFormat;
        }
        return lastPixelFormatIndex;
    }
    static bool TryGetValue (int[] flags, int[] values, PixelFormatAttribute flag, out int value) {
        value = -1;
        var i = Array.IndexOf(flags, (int)flag);
        if (i < 0 || i >= values.Length)
            return false;
        value = values[i];
        return true;
    }

    private void Paint () {
        long t0 = Stopwatch.GetTimestamp();
        var dticks = t0 - lastTicks;
        lastTicks = t0;
        var dt = dticks > 0 ? (float)((double)dticks / Stopwatch.Frequency) : 0f;
        Render(dt);
        Demand(Gdi.SwapBuffers(DeviceContext));
        ++FramesRendered;
    }
    protected virtual void Render (float dt) {
        Opengl.ClearColor(0.5f, 0.5f, 0.5f, 1f);
        Opengl.Clear(BufferBit.Color | BufferBit.Depth);
    }

    static string Foo (IntPtr p) => Foo(p.ToInt64());
    static string Foo (int p) => $"{p:x8}, {(p >> 16) & ushort.MaxValue}, {p & ushort.MaxValue}";
    static string Foo (long p) => $"{p:x16}, {(p >> 48) & ushort.MaxValue}, {(p >> 32) & ushort.MaxValue}, {(p >> 16) & ushort.MaxValue}, {p & ushort.MaxValue}";

    protected virtual void WindowPosChanging (IntPtr w, IntPtr l) => WriteLine(nameof(WindowPosChanging), w, l);
    protected virtual void Moving (Rect r) => Debug.WriteLine($"{nameof(Moving)} {r.left}, {r.top}, {r.right}, {r.bottom}");
    protected virtual void WindowPosChanged (WindowPos p) => Debug.WriteLine($"{nameof(WindowPosChanged)}: {p.x}, {p.y}, {p.cx}, {p.cy}");
    protected virtual void EraseBkgnd () => Debug.WriteLine(nameof(EraseBkgnd));
    protected virtual void Move (short x, short y) => WriteLine(nameof(Move), x, y);
    protected virtual void MouseMove (short x, short y) => WriteLine(nameof(MouseMove), x, y);
    protected virtual void MouseLeave () => Debug.WriteLine(nameof(MouseLeave));
    protected virtual void NCMouseLeave (IntPtr w, IntPtr l) => WriteLine(nameof(NCMouseLeave), w, l);
    protected virtual void CaptureChanged () => Debug.WriteLine(nameof(CaptureChanged));
    protected virtual void ExitSizeMove () => Debug.WriteLine(nameof(ExitSizeMove));
    protected virtual void EnterSizeMove () => Debug.WriteLine(nameof(EnterSizeMove));
    protected virtual void SetFocus () => Debug.WriteLine(nameof(SetFocus));
    protected virtual void KillFocus () => Debug.WriteLine(nameof(KillFocus));
    protected virtual void Size (SizeMessage m, int width, int height) => Debug.WriteLine($"{nameof(Size)}, {m}, {width} x {height}");
    protected virtual void KeyUp (Keys k) => Debug.WriteLine($"{nameof(KeyUp)}, {k}");

    static void WriteLine (string name, IntPtr w, IntPtr l) => Debug.WriteLine($"{name}: w {Foo(w)}, l {Foo(l)}");
    static void WriteLine (string name, int a, int b) => Debug.WriteLine($"{name}: {a}, {b}");

    protected virtual void KeyDown (Keys k) {
        Debug.WriteLine($"{nameof(KeyDown)}, {k}");
        switch (k) {
            case Keys.Escape:
                User.PostQuitMessage(0);
                break;
        }
    }
    private static (short x, short y) Split (IntPtr p) {
        var i = (int)(p.ToInt64() & int.MaxValue);
        return ((short)(i & ushort.MaxValue), (short)((i >> 16) & ushort.MaxValue));
    }
    protected virtual void Load () { }
    protected virtual void Closing () { }

    public void Run () {
        Load();
        _ = User.ShowWindow(WindowHandle, 10);
        Demand(User.UpdateWindow(WindowHandle));
        Message m = new();
        for (; ; ) {
            if (User.PeekMessageW(ref m, IntPtr.Zero, 0, 0, 0)) {
                var eh = User.GetMessageW(ref m, IntPtr.Zero, 0, 0);
                if (eh == new IntPtr(-1))
                    Environment.FailFast(eh.ToString());
                if (eh == IntPtr.Zero)
                    break;
                _ = User.DispatchMessageW(ref m);
            }
            Paint();
        }
        Demand(Opengl.MakeCurrent(IntPtr.Zero, IntPtr.Zero));
        Demand(Opengl.DeleteContext(RenderingContext));
        Demand(User.ReleaseDC(WindowHandle, DeviceContext));
    }

    public void Dispose () {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Dispose (bool dispose) {
        if (dispose && !disposed) {
            disposed = true;
            Closing();
            Demand(User.DestroyWindow(WindowHandle));
            Demand(User.UnregisterClassW((IntPtr)ClassAtom, IntPtr.Zero));
        }
    }

    private IntPtr MyProc (IntPtr hWnd, WinMessage msg, IntPtr w, IntPtr l) {
        if (IntPtr.Zero != RenderingContext)
            switch (msg) {
                case WinMessage.Moving:
                    Moving(Marshal.PtrToStructure<Rect>(l));
                    break;
                case WinMessage.WindowPosChanged:
                    WindowPosChanged(Marshal.PtrToStructure<WindowPos>(l));
                    break;
                case WinMessage.EraseBkgnd:
                    EraseBkgnd();
                    return (IntPtr)1;
                case WinMessage.Move: {
                        var (x, y) = Split(l);
                        Move(x, y);
                    }
                    break;
                case WinMessage.MouseLeave:
                    MouseLeave();
                    break;
                case WinMessage.MouseMove: {
                        var (x, y) = Split(l);
                        MouseMove(x, y);
                    }
                    break;
                case WinMessage.CaptureChanged:
                    CaptureChanged();
                    break;
                case WinMessage.EnterSizeMove:
                    EnterSizeMove();
                    break;
                case WinMessage.ExitSizeMove:
                    ExitSizeMove();
                    break;
                case WinMessage.SetFocus:
                    SetFocus();
                    break;
                case WinMessage.KillFocus:
                    KillFocus();
                    break;
                case WinMessage.Size: {
                        var i = (int)(w.ToInt64() & int.MaxValue);
                        if (Enum.IsDefined(typeof(SizeMessage), i)) {
                            var (width, height) = Split(l);
                            Size((SizeMessage)i, width, height);
                        }
                    }
                    break;
                case WinMessage.KeyDown: {
                        var m = new KeyMessage(w, l);
                        if (m.WasDown)
                            break;
                        KeyDown(m.Key);
                        return IntPtr.Zero;
                    }
                case WinMessage.KeyUp: {
                        var m = new KeyMessage(w, l);
                        KeyUp(m.Key);
                        return IntPtr.Zero;
                    }
                case WinMessage.Close:
                    User.PostQuitMessage(0);
                    break;
                case WinMessage.Paint:
                    Paint();
                    return IntPtr.Zero;
            }
        return User.DefWindowProcW(hWnd, msg, w, l);
    }
}