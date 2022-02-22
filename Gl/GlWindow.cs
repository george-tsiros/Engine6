namespace Gl;

using System;
using System.Diagnostics;
using Win32;

public class GlWindow:BaseWindow {
    protected ulong FramesRendered { get; private set; }
    protected IntPtr DeviceContext { get; private set; }
    protected IntPtr RenderingContext { get; private set; }
    private long lastTicks = long.MaxValue;


    public GlWindow (Vector2i size) : base(size) {
        DeviceContext = User.GetDC(WindowHandle);
        RenderingContext = Opengl.CreateSimpleContext(DeviceContext);
        State.DebugOutput = true;
        State.SwapInterval = -1;
    }

    protected override void Paint () {
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

    public override void Run () {
        base.Run();
        Demand(Opengl.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero));
        Demand(Opengl.wglDeleteContext(RenderingContext));
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
