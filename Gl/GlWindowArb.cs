namespace Gl;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Win32;
using Linear;
using System.Diagnostics;

public class GlWindowArb:GlWindow {

    public GlWindowArb (Vector2i size, Vector2i? position = null) : base(size, position) {

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
            //if (IsAppropriate(format))
            candidates.Add(format);
        }

        if (0 == candidates.Count)
            throw new Exception();

        //Opengl.MakeCurrent(DeviceContext, IntPtr.Zero);
        //Opengl.DeleteContext(RenderingContext);
        //RenderingContext = IntPtr.Zero;
        //if (!User.ReleaseDC(WindowHandle, DeviceContext))
        //    throw new WinApiException(nameof(User.ReleaseDC));
        //DeviceContext = IntPtr.Zero;
        //_ = User.DestroyWindow(WindowHandle);
        //WindowHandle = IntPtr.Zero;

        var pfd = new PixelFormatDescriptor { size = PixelFormatDescriptor.Size, version = 1 };
        var workingPfdIndices = new List<int>();

        foreach (var candidate in candidates) {
            try {
                if (IntPtr.Zero != RenderingContext) {
                    Opengl.MakeCurrent(DeviceContext, RenderingContext);
                    Opengl.DeleteContext(RenderingContext);
                    RenderingContext = IntPtr.Zero;
                }
                if (IntPtr.Zero != DeviceContext) {
                    if (!User.ReleaseDC(WindowHandle, DeviceContext))
                        throw new WinApiException(nameof(User.ReleaseDC));
                    DeviceContext = IntPtr.Zero;
                }
                if (IntPtr.Zero != WindowHandle) {
                    _ = User.DestroyWindow(WindowHandle);
                    WindowHandle = IntPtr.Zero;
                }
                WindowHandle = User.CreateWindow(ClassAtom, new(position ?? new(), size), SelfHandle);
                DeviceContext = User.GetDC(WindowHandle);
                if (IntPtr.Zero == DeviceContext)
                    throw new WinApiException(nameof(User.GetDC));

                Debug.WriteLine($"trying #{candidate.Index}");
                Gdi.DescribePixelFormat(DeviceContext, candidate.Index, ref pfd);
                if (!Gdi.SetPixelFormat(DeviceContext, candidate.Index, ref pfd))
                    throw new WinApiException(nameof(Gdi.SetPixelFormat));
                var attribs = new int[] { (int)ContextAttribute.MAJOR_VERSION, Opengl.ShaderVersion.Major, (int)ContextAttribute.MINOR_VERSION, Opengl.ShaderVersion.Minor, (int)PixelFormatAttributes.ACCELERATION, (int)candidate.Acceleration, (int)PixelFormatAttributes.PIXEL_TYPE, (int)candidate.PixelType, (int)PixelFormatAttributes.COLOR_BITS, candidate.ColorBits, (int)PixelFormatAttributes.DEPTH_BITS, candidate.DepthBits, (int)PixelFormatAttributes.SWAP_METHOD, (int)candidate.SwapMethod, (int)PixelFormatAttributes.DOUBLE_BUFFER, candidate.DoubleBuffer ? 1 : 0, (int)PixelFormatAttributes.DRAW_TO_WINDOW, 1, (int)PixelFormatAttributes.STEREO, 0, (int)ContextAttribute.FLAGS, 1, (int)ContextAttribute.PROFILE_MASK, 1, 0, 0 };
                var candidateContext = Opengl.CreateContextAttribsARB(DeviceContext, IntPtr.Zero, attribs);
                Opengl.MakeCurrent(DeviceContext, candidateContext);
                RenderingContext = candidateContext;
                var depthTestState = Opengl.IsEnabled(Capability.DepthTest);
                Debug.Write($"{nameof(Capability.DepthTest)} is {OnOff(depthTestState)}");
                if (depthTestState)
                    Opengl.Disable(Capability.DepthTest);
                else
                    Opengl.Enable(Capability.DepthTest);
                if (depthTestState != Opengl.IsEnabled(Capability.DepthTest)) {
                    Debug.Write(", toggled ok");
                    if (!depthTestState)
                        Opengl.Enable(Capability.DepthTest);
                    else
                        Opengl.Disable(Capability.DepthTest);
                    if (depthTestState == Opengl.IsEnabled(Capability.DepthTest)) {
                        Debug.WriteLine(", restored ok");
                        workingPfdIndices.Add(candidate.Index);
                    } else
                        Debug.WriteLine(", failed to restore");
                } else {
                    Debug.WriteLine($", failed to toggle");
                }
            } catch (Exception e) when (e is GlException || e is WinApiException) {
                Debug.WriteLine(e);
                continue;
            }
        }


        var extensionsPtr = Opengl.GetExtensionsString();
        var extensionsString = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(extensionsPtr);
        // optional
        //State.DebugOutput = true;
    }
    static string OnOff (bool yes) => yes ? "on" : "off";
    static bool IsAppropriate (ExtendedPixelFormat f) => true
        && f.PixelType == PixelType.Rgba
        && f.Acceleration == Acceleration.Full
        && f.ColorBits == 32
        && f.DepthBits >= 24
        //&& f.DoubleBuffer
        //&& SwapMethod.Undefined != f.SwapMethod
        ;
}
