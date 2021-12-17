namespace Gl;

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Win32;

public class GlWindow:IDisposable {
    protected static void Demand (bool condition, string message = null) {
        if (!condition) {
            var windowsError = Kernel.GetLastError();
            var stackFrame = new StackFrame(1, true);
            var m = $">{stackFrame.GetFileName()}({stackFrame.GetFileLineNumber()},{stackFrame.GetFileColumnNumber()}): {message ?? "?"}";
            if (Debugger.IsAttached)
                throw new Exception(m);
            else
                Console.WriteLine(m);
        }
    }

    protected static IntPtr Demand (IntPtr p) {
        Demand(IntPtr.Zero != p);
        return p;
    }

    protected static int Demand (int p) {
        Demand(0 != p);
        return p;
    }

    protected const string ClassName = "MYWINDOWCLASS";
    protected ulong FramesRendered { get; private set; }
    public IntPtr DeviceContext { get; private set; }
    public IntPtr RenderingContext { get; private set; }
    public IntPtr WindowHandle { get; private set; }
    protected ushort ClassAtom { get; }
    public int Width { get; }
    public int Height { get; }
    private long lastTicks = long.MaxValue;
    private bool disposed;
    private HashSet<string> extensions = new(StringComparer.OrdinalIgnoreCase);
    const WindowStyle ClipPopup = WindowStyle.ClipChildren | WindowStyle.ClipSiblings | WindowStyle.Popup;
    static void HandleFirstWindowMessages (IntPtr window) {
        var m = new Message();
        while (User.PeekMessageW(ref m, window, 0, 0, 1)) {
            _ = User.TranslateMessage(ref m);
            _ = User.DispatchMessageW(ref m);
        }
    }
    enum PixelType { Rgba = 0x202b, RgbaFloat = 0x21a0, Indexed = 0x202c, RgbaUnsignedFloat = 0x20a8 }
    enum SwapMethod { Exchange = 0x2028, Copy = 0x2029, Undefined = 0x202a, }
    enum Acceleration { None = 0x2025, Full = 0x2027, }
    readonly struct ExtendedPixelFormat {
        public int Index { get; init; }
        public PixelType PixelType { get; init; }
        public Acceleration Acceleration { get; init; }
        public int ColorBits { get; init; }
        public int DepthBits { get; init; }
        public bool DoubleBuffer { get; init; }
        public SwapMethod SwapMethod { get; init; }
        public static ExtendedPixelFormat Create (int index, int[] values) => new() {
            Index = index,
            PixelType = (PixelType)values[0],
            Acceleration = (Acceleration)values[1],
            ColorBits = values[2],
            DepthBits = values[3],
            DoubleBuffer = values[4] != 0,
            SwapMethod = (SwapMethod)values[5],
        };
        public override string ToString () => $"{Index}:{PixelType},{Acceleration},{ColorBits}:{DepthBits}{(DoubleBuffer ? ",DoubleBuffer" : "")},{SwapMethod}";
    }
    private static bool IsGood (ExtendedPixelFormat f) => f.DepthBits == 24 && f.ColorBits == 32 && f.Acceleration == Acceleration.Full && f.DoubleBuffer && f.PixelType == PixelType.Rgba && f.SwapMethod == SwapMethod.Undefined;
    static readonly PixelFormatAttribute[] Attributes = new PixelFormatAttribute[] { PixelFormatAttribute.PIXEL_TYPE_ARB, PixelFormatAttribute.ACCELERATION_ARB, PixelFormatAttribute.COLOR_BITS_ARB, PixelFormatAttribute.DEPTH_BITS_ARB, PixelFormatAttribute.DOUBLE_BUFFER_ARB, PixelFormatAttribute.SWAP_METHOD, };
    const PixelFlags PfdFlags = PixelFlags.DoubleBuffer | PixelFlags.DrawToWindow | PixelFlags.SupportOpengl | PixelFlags.SwapCopy | PixelFlags.SupportComposition;

    static IntPtr CreateWindow (ushort atom, Vector2i size) {
        var w = Demand(User.CreateWindowExW(WindowStyleEx.None, new(atom), IntPtr.Zero, ClipPopup, 0, 0, size.X, size.Y, IntPtr.Zero, IntPtr.Zero, Kernel.GetModuleHandleW(null), IntPtr.Zero));
        HandleFirstWindowMessages(w);
        return w;
    }
    unsafe static int FindPixelFormat (IntPtr dc, PixelFormatDescriptor* pfd, Predicate<PixelFormatDescriptor> condition) {
        var formatCount = Demand(Gdi.DescribePixelFormat(dc, 0, (*pfd).size, null));
        for (var i = 1; i <= formatCount; i++) {
            _ = Demand(Gdi.DescribePixelFormat(dc, i, (*pfd).size, pfd));
            if (condition(*pfd))
                return i;
        }
        return 0;
    }

    unsafe public GlWindow (Vector2i size) {
        var self = Kernel.GetModuleHandleW(null);
        ClassAtom = RegisterWindowClass(self);

        var helperWindow = CreateWindow(ClassAtom, size);

        (Width, Height) = size;
        var temporaryDc = User.GetDC(helperWindow);
        var pfd = PixelFormatDescriptor.Create();
        var temporaryPixelFormatIndex = Demand(FindPixelFormat(temporaryDc, &pfd, x => x.colorBits == 32 && x.depthBits == 24 && x.flags == PfdFlags));
        Demand(Gdi.SetPixelFormat(temporaryDc, temporaryPixelFormatIndex, ref pfd));
        var temporaryRenderingContext = Demand(Opengl.wglCreateContext(temporaryDc));
        Demand(Opengl.wglMakeCurrent(temporaryDc, temporaryRenderingContext));
        Demand(Opengl.ExtensionsSupported);
        var extendedFormatCount = 0;
        int attrib = (int)PixelFormatAttribute.NUMBER_PIXEL_FORMATS_ARB;
        Demand(Opengl.GetPixelFormatAttribivARB(temporaryDc, 1, 0, 1, &attrib, &extendedFormatCount));
        var queriedAttributes = Array.ConvertAll(Attributes, a => (int)a);
        var values = new int[queriedAttributes.Length];
        var selectedFormat = default(ExtendedPixelFormat);
        for (var i = 1; i <= extendedFormatCount && selectedFormat.Index == 0; i++) {
            Demand(GetPixelFormatAttribivARB(temporaryDc, i, 0, queriedAttributes, values));
            var epf = ExtendedPixelFormat.Create(i, values);
            if (IsGood(epf))
                selectedFormat = epf;
        }
        Demand(selectedFormat.Index > 0);
        Demand(Opengl.wglDeleteContext(temporaryRenderingContext));
        Demand(User.DestroyWindow(helperWindow));

        WindowHandle = CreateWindow(ClassAtom, size);// User.CreateWindowExW(WindowStyleEx.None, new(ClassAtom), IntPtr.Zero, ClipPopup, 1280, 0, Width, Height, IntPtr.Zero, IntPtr.Zero, self, IntPtr.Zero);
        DeviceContext = Demand(User.GetDC(WindowHandle));
        Demand(Gdi.DescribePixelFormat(DeviceContext, selectedFormat.Index, pfd.size, &pfd));
        Demand(Gdi.SetPixelFormat(DeviceContext, selectedFormat.Index, ref pfd));
        var attribs = new int[] {
                (int)PixelFormatAttribute.CONTEXT_MAJOR_VERSION_ARB, 4,
                (int)PixelFormatAttribute.CONTEXT_MINOR_VERSION_ARB, 6,
                (int)PixelFormatAttribute.ACCELERATION_ARB, (int)selectedFormat.Acceleration,
                (int)PixelFormatAttribute.PIXEL_TYPE_ARB, (int)selectedFormat.PixelType,
                (int)PixelFormatAttribute.COLOR_BITS_ARB, selectedFormat.ColorBits,
                (int)PixelFormatAttribute.DEPTH_BITS_ARB, selectedFormat.DepthBits,
                (int)PixelFormatAttribute.SWAP_METHOD, (int)selectedFormat.SwapMethod,
                (int)PixelFormatAttribute.DOUBLE_BUFFER_ARB, 1,
                (int)PixelFormatAttribute.DRAW_TO_WINDOW_ARB, 1,
                (int)PixelFormatAttribute.STEREO_ARB, 0,
                (int)PixelFormatAttribute.SAMPLES_ARB, 1,
                (int)ContextAttributes.ContextFlags, (int)ContextFlags.Debug,
                (int)ContextAttributes.ProfileMask, (int)ProfileMask.Core,
                0,0,
            };
        RenderingContext = Opengl.CreateContextAttribsARB(DeviceContext, IntPtr.Zero, attribs);
        Demand(Opengl.wglMakeCurrent(DeviceContext, RenderingContext));
        Demand(RenderingContext);

        State.DebugOutput = true;
        State.SwapInterval = 1;
        //State.DepthWriteMask = true;
    }

    private unsafe ushort RegisterWindowClass (IntPtr self) {
        var windowClass = WindowClassExW.Create();
        windowClass.style = ClassStyle.HRedraw | ClassStyle.VRedraw | ClassStyle.OwnDc;
        windowClass.wndProc = SelectorProc;
        windowClass.hInstance = self;
        windowClass.classname = ClassName;
        var atom = User.RegisterClassExW(ref windowClass);
        return atom != 0 ? atom : throw new Exception("failed to register class");
    }

    unsafe private static bool GetPixelFormatAttribivARB (IntPtr dc, int pixelFormat, int x, int[] attributes, int[] values) {
        Demand(attributes.Length == values.Length);
        fixed (int* a = &attributes[0])
        fixed (int* v = &values[0])
            return Opengl.GetPixelFormatAttribivARB(dc, pixelFormat, 0, (uint)values.Length, a, v);
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
        Opengl.glClearColor(0.5f, 0.5f, 0.5f, 1f);
        Opengl.glClear(BufferBit.Color | BufferBit.Depth);
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
    protected virtual void KeyUp (Keys k) { }// => Debug.WriteLine($"{nameof(KeyUp)}, {k}");

    static void WriteLine (string name, IntPtr w, IntPtr l) => Debug.WriteLine($"{name}: w {Foo(w)}, l {Foo(l)}");
    static void WriteLine (string name, int a, int b) => Debug.WriteLine($"{name}: {a}, {b}");

    protected virtual void KeyDown (Keys k) {
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
    protected void SetText (string text) => Demand(User.SetWindowText(WindowHandle, text));
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
        Demand(Opengl.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero));
        Demand(Opengl.wglDeleteContext(RenderingContext));
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
            Demand(User.UnregisterClassW(new IntPtr(ClassAtom), IntPtr.Zero));
        }
    }

    private IntPtr SelectorProc (IntPtr hWnd, WinMessage msg, IntPtr w, IntPtr l) => IntPtr.Zero != RenderingContext ? WndProcActual(hWnd, msg, w, l) : User.DefWindowProcW(hWnd, msg, w, l);
    private IntPtr WndProcActual (IntPtr hWnd, WinMessage msg, IntPtr w, IntPtr l) {
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