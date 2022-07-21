namespace Gl;

using System;
using System.Diagnostics;
using Win32;

public class GlWindow:SimpleWindow {
    protected ulong FramesRendered { get; private set; }
    protected IntPtr DeviceContext { get; private init; }
    protected IntPtr RenderingContext { get; private init; }

    private long lastTicks = long.MaxValue;
    const PixelFlags PfdFlags = PixelFlags.DoubleBuffer | PixelFlags.DrawToWindow | PixelFlags.SupportOpengl | PixelFlags.SwapCopy | PixelFlags.SupportComposition;

    unsafe public GlWindow (Vector2i size) : base(size) {
        DeviceContext = User.GetDC(WindowHandle);
        RenderingContext = Opengl.CreateSimpleContext(DeviceContext, x => x.colorBits == 32 && x.depthBits == 24 && x.flags == PfdFlags);
#if true
        var lastDeviceContext = Opengl.GetCurrentDC();
        var lastRenderingContext = Opengl.GetCurrentContext();
        Demand(Opengl.MakeCurrent(DeviceContext, RenderingContext), "failed to make context current");

        var extendedFormatCount = Opengl.GetPixelFormatCount(DeviceContext, 1, 0, 1);
        var attributes = new int[] {
            (int)PixelFormatAttributes.PIXEL_TYPE_ARB,
            (int)PixelFormatAttributes.ACCELERATION_ARB,
            (int)PixelFormatAttributes.COLOR_BITS_ARB,
            (int)PixelFormatAttributes.DEPTH_BITS_ARB,
            (int)PixelFormatAttributes.DOUBLE_BUFFER_ARB,
            (int)PixelFormatAttributes.SWAP_METHOD_ARB,
        };
        var values = new int[attributes.Length];
        var selectedFormat = new ExtendedPixelFormat();

        for (var i = 1; i <= extendedFormatCount && selectedFormat.Index == 0; i++) {
            fixed (int* attr = attributes)
            fixed (int* vals = values) {
                _ = Opengl.GetPixelFormatAttribivARB(DeviceContext, i, 0, 6, attr, vals);
            }
            selectedFormat.PixelType = (PixelType)values[0];
            selectedFormat.Acceleration = (Acceleration)values[1];
            selectedFormat.ColorBits = values[2];
            selectedFormat.DepthBits = values[3];
            selectedFormat.DoubleBuffer = values[4] != 0;
            selectedFormat.SwapMethod = (SwapMethod)values[5];
            if (selectedFormat.DepthBits == 24 && selectedFormat.ColorBits == 32 && selectedFormat.Acceleration == Acceleration.Full && selectedFormat.DoubleBuffer && selectedFormat.PixelType == PixelType.Rgba && selectedFormat.SwapMethod == SwapMethod.Copy)
                selectedFormat.Index = i;
        }
        _ = Opengl.MakeCurrent(lastDeviceContext, lastRenderingContext);
        _ = Opengl.DeleteContext(RenderingContext);
        User.DestroyWindow(WindowHandle);
        Instance = this;
        User.CreateWindow(ClassAtom, size, SelfHandle);
        DeviceContext = User.GetDC(WindowHandle);
        var pfd = new PixelFormatDescriptor { size = PixelFormatDescriptor.Size, version = 1 };
        var described = Gdi.DescribePixelFormat(DeviceContext, selectedFormat.Index, ref pfd);
        if (!described)
            throw new WinApiException("DescribePixelFormat");
        var formatSet = Gdi.SetPixelFormat(DeviceContext, selectedFormat.Index, ref pfd);
        if (formatSet == 0)
            throw new WinApiException("SetPixelFormat");
        var attribs = new int[] {
            (int)PixelFormatAttributes.CONTEXT_MAJOR_VERSION_ARB, Opengl.ShaderVersion.Major,
            (int)PixelFormatAttributes.CONTEXT_MINOR_VERSION_ARB, Opengl.ShaderVersion.Minor,
            (int)PixelFormatAttributes.ACCELERATION_ARB, (int)selectedFormat.Acceleration,
            (int)PixelFormatAttributes.PIXEL_TYPE_ARB, (int)selectedFormat.PixelType,
            (int)PixelFormatAttributes.COLOR_BITS_ARB, selectedFormat.ColorBits,
            (int)PixelFormatAttributes.DEPTH_BITS_ARB, selectedFormat.DepthBits,
            (int)PixelFormatAttributes.SWAP_METHOD_ARB, (int)selectedFormat.SwapMethod,
            (int)PixelFormatAttributes.DOUBLE_BUFFER_ARB, 1,
            (int)PixelFormatAttributes.DRAW_TO_WINDOW_ARB, 1,
            (int)PixelFormatAttributes.STEREO_ARB, 0,
            (int)PixelFormatAttributes.SAMPLES_ARB, 1,
            (int)PixelFormatAttributes.CONTEXT_FLAGS_ARB, 1,
            (int)PixelFormatAttributes.CONTEXT_PROFILE_MASK_ARB, 1,
            0,0
        };
        RenderingContext = Opengl.CreateContextAttribsARB(DeviceContext, IntPtr.Zero, attribs);
        Opengl.MakeCurrent(DeviceContext, RenderingContext);
#endif
        State.DebugOutput = true;
        State.SwapInterval = 0;
    }

    protected override void Paint () {
        long t0 = Stopwatch.GetTimestamp();
        var dticks = t0 - lastTicks;
        //if (dticks < Stopwatch.Frequency / 100)
        //    return;
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

    public override void Run () {
        base.Run();
        Demand(Opengl.MakeCurrent(IntPtr.Zero, IntPtr.Zero));
        Demand(Opengl.DeleteContext(RenderingContext));
        Demand(User.ReleaseDC(WindowHandle, DeviceContext));
    }

    private int virtualCursorX;
    private int virtualCursorY;
    private void MouseMoveInternal (int x, int y) {
        if (x == virtualCursorX && y == virtualCursorY)
            return;
        virtualCursorX = x;
        virtualCursorY = y;
        Debug.WriteLine($"{FramesRendered}: {x}, {y}");
        MouseMove(x, y);
    }
}
