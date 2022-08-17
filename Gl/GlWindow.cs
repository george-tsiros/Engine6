namespace Gl;

using System;
using System.Diagnostics;
using Win32;
using Linear;

public class GlWindow:Window {
    const PixelFlags RequiredFlags = PixelFlags.None
        | PixelFlags.DrawToWindow
        | PixelFlags.DoubleBuffer
        | PixelFlags.SupportOpengl
        //| PixelFlags.SwapExchange
        //| PixelFlags.SupportComposition
        ;

    const PixelFlags RejectedFlags = PixelFlags.None
        | PixelFlags.GenericAccelerated
        | PixelFlags.GenericFormat
        ;

    protected IntPtr RenderingContext;
    protected long FramesRendered { get; private set; }
    protected long LastSync { get; private set; }
    public GlWindow () {
        RenderingContext = Opengl.CreateSimpleContext(DeviceContext, x => x.colorBits == 32 && x.depthBits >= 24 && (x.flags & RequiredFlags) == RequiredFlags && (x.flags & RejectedFlags) == 0);
        Opengl.MakeCurrent(DeviceContext, RenderingContext);
        LastSync = Stopwatch.GetTimestamp();
    }

    protected override void OnKeyUp (Keys k) {
        switch (k) {
            case Keys.Escape:
                User32.PostQuitMessage(0);
                return;
        }
        base.OnKeyUp(k);
    }

    protected override void OnPaint () {
        Render();
        Gdi32.SwapBuffers(DeviceContext);
        LastSync = Stopwatch.GetTimestamp();
        ++FramesRendered;
    }

    protected virtual void Render () {
        Opengl.ClearColor(0.5f, 0.5f, 0.5f, 1f);
        Opengl.Clear(BufferBit.ColorDepth);
    }

    bool disposed = false;
    public override void Dispose (bool dispose) {
        if (dispose && !disposed) {
            disposed = true;
            Opengl.ReleaseCurrent(DeviceContext);
            Opengl.DeleteContext(RenderingContext);
            if (!User32.ReleaseDC(WindowHandle, DeviceContext))
                throw new Exception(nameof(User32.ReleaseDC));
        }
    }
}
