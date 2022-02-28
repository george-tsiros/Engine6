namespace Gl;

using System;
using Win32;
enum PixelType { Rgba = 0x202b, RgbaFloat = 0x21a0, Indexed = 0x202c, RgbaUnsignedFloat = 0x20a8 };
enum SwapMethod { Copy = 0x2029, Undefined = 0x202a, };
enum Acceleration { None = 0x2025, Full = 0x2027, };
struct ExtendedPixelFormat {
    public int Index;
    public PixelType PixelType;
    public Acceleration Acceleration;
    public int ColorBits;
    public int DepthBits;
    public bool DoubleBuffer;
    public SwapMethod SwapMethod;
}

enum PixelFormatAttributes {
    PIXEL_TYPE_ARB = 0x2013,
    ACCELERATION_ARB = 0x2003,
    COLOR_BITS_ARB = 0x2014,
    DEPTH_BITS_ARB = 0x2022,
    DOUBLE_BUFFER_ARB = 0x2011,
    SWAP_METHOD_ARB = 0x2007,
    CONTEXT_MAJOR_VERSION_ARB = 0x2091,
    CONTEXT_MINOR_VERSION_ARB = 0x2092,
    DRAW_TO_WINDOW_ARB = 0x2001,
    STEREO_ARB = 0x2012,
    SAMPLES_ARB = 0x2042,
    CONTEXT_FLAGS_ARB = 0x2094,
    CONTEXT_PROFILE_MASK_ARB = 0x9126,
}
abstract public class OffScreenWindowBase:SimpleWindow {
    protected IntPtr DeviceContext { get; private set; }
    protected IntPtr RenderingContext { get; private set; }
    const PixelFlags PfdFlags = PixelFlags.DoubleBuffer | PixelFlags.DrawToWindow | PixelFlags.SupportOpengl | PixelFlags.SwapCopy | PixelFlags.SupportComposition;
    unsafe public OffScreenWindowBase (Vector2i size) : base(size) {
        DeviceContext = User.GetDC(WindowHandle);
        RenderingContext = Opengl.CreateSimpleContext(DeviceContext, x => x.colorBits == 32 && x.depthBits == 24 && x.flags == PfdFlags);
        State.DebugOutput = true;
    }

    protected override void Paint () {
        Render();
        Demand(Gdi.SwapBuffers(DeviceContext));
    }
    abstract protected void Render ();

    public override void Run () {
        Load();
        _ = User.ShowWindow(WindowHandle, 10);
        Demand(User.UpdateWindow(WindowHandle));
        Message m = new();
        var invalidPtr = new IntPtr(-1);
        for (; ; ) {
            if (User.PeekMessageW(ref m, IntPtr.Zero, 0, 0, PeekRemove.NoRemove)) {
                var eh = User.GetMessageW(ref m, IntPtr.Zero, 0, 0);
                if (eh == invalidPtr)
                    Environment.FailFast(null);
                if (eh == IntPtr.Zero)
                    break;
                _ = User.DispatchMessageW(ref m);
            }
            Invalidate();
        }
        Demand(Opengl.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero));
        Demand(Opengl.wglDeleteContext(RenderingContext));
        Demand(User.ReleaseDC(WindowHandle, DeviceContext));
    }
}
