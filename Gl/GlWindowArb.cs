namespace Gl;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Win32;

public class GlWindowArb:GlWindow {

    public GlWindowArb (Vector2i size) : base(size) {
        var lastDeviceContext = Opengl.GetCurrentDC();
        var lastRenderingContext = Opengl.GetCurrentContext();

        var extendedFormatCount = Opengl.GetPixelFormatCount(DeviceContext, 1, 0, 1);
        var attributes = new int[] {
            (int)PixelFormatAttributes.PIXEL_TYPE,
            (int)PixelFormatAttributes.ACCELERATION,
            (int)PixelFormatAttributes.COLOR_BITS,
            (int)PixelFormatAttributes.DEPTH_BITS,
            (int)PixelFormatAttributes.DOUBLE_BUFFER,
            (int)PixelFormatAttributes.SWAP_METHOD,
        };
        var values = new int[attributes.Length];
        var format = new ExtendedPixelFormat();
        var candidates = new List<ExtendedPixelFormat>();
        for (var i = 1; i <= extendedFormatCount; ++i) {
            Opengl.GetPixelFormatAttribivARB(DeviceContext, i, 0, 6, attributes, values);
            format.Index = i;
            format.PixelType = (PixelType)values[0];
            format.Acceleration = (Acceleration)values[1];
            format.ColorBits = values[2];
            format.DepthBits = values[3];
            format.DoubleBuffer = values[4] != 0;
            format.SwapMethod = (SwapMethod)values[5];
            candidates.Add(format);
        }

        //using (var f = new StreamWriter("ExtendedPixelFormats.txt", false, Encoding.ASCII) { NewLine = "\n" })
        //    foreach (var eh in candidates)
        //        f.WriteLine(eh);

        var selectedFormat = candidates.Find(IsAppropriate);
        if (selectedFormat.Index == 0)
            throw new Exception();
        _ = Opengl.MakeCurrent(lastDeviceContext, lastRenderingContext);
        _ = Opengl.DeleteContext(RenderingContext);
        _ = User.DestroyWindow(WindowHandle);
        WindowHandle = User.CreateWindow(ClassAtom, size, SelfHandle);
        DeviceContext = User.GetDC(WindowHandle);
        var pfd = new PixelFormatDescriptor { size = PixelFormatDescriptor.Size, version = 1 };
        Gdi.DescribePixelFormat(DeviceContext, selectedFormat.Index, ref pfd);
        var formatSet = Gdi.SetPixelFormat(DeviceContext, selectedFormat.Index, ref pfd);
        if (formatSet == 0)
            throw new WinApiException("SetPixelFormat");
        var attribs = new int[] {
            (int)ContextAttribute.MAJOR_VERSION, Opengl.ShaderVersion.Major,
            (int)ContextAttribute.MINOR_VERSION, Opengl.ShaderVersion.Minor,
            (int)PixelFormatAttributes.ACCELERATION, (int)selectedFormat.Acceleration,
            (int)PixelFormatAttributes.PIXEL_TYPE, (int)selectedFormat.PixelType,
            (int)PixelFormatAttributes.COLOR_BITS, selectedFormat.ColorBits,
            (int)PixelFormatAttributes.DEPTH_BITS, selectedFormat.DepthBits,
            (int)PixelFormatAttributes.SWAP_METHOD, (int)selectedFormat.SwapMethod,
            (int)PixelFormatAttributes.DOUBLE_BUFFER, selectedFormat.DoubleBuffer ? 1 : 0,
            (int)PixelFormatAttributes.DRAW_TO_WINDOW, 1,
            (int)PixelFormatAttributes.STEREO, 0,
            (int)ContextAttribute.FLAGS, 1,
            (int)ContextAttribute.PROFILE_MASK, 1,
            0, 0
        };
        RenderingContext = Opengl.CreateContextAttribsARB(DeviceContext, IntPtr.Zero, attribs);
        _ = Opengl.MakeCurrent(DeviceContext, RenderingContext);
        var extensionsPtr = Opengl.GetExtensionsString();
        var extensionsString = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(extensionsPtr);
        //State.DebugOutput = true;
    }
    static bool IsAppropriate (ExtendedPixelFormat f) => true
        && f.ColorBits == 32
        && f.DepthBits >= 24
        && f.Acceleration == Acceleration.Full
        && f.DoubleBuffer
        //&& f.SwapMethod == SwapMethod.Copy
        && f.PixelType == PixelType.Rgba
        ;
}
