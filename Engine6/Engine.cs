namespace Engine6;

using System;
using System.Windows.Forms;
using Win32;
using Gl;
using System.Diagnostics;
using System.Text;
using System.IO;

unsafe class Engine6 {
    private static int GetPixelFormatCount () {
        using var f = new Form();
        using var dc = new DeviceContext(f.Handle);
        var pixelFormatCount = Gdi32.DescribePixelFormat((IntPtr)dc, 0, 0, null);
        Debug.Assert(0 < pixelFormatCount);
        return pixelFormatCount;
    }

    //private static delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int*, IntPtr> wglCreateContextAttribsARB_Type;
    private static delegate* unmanaged[Stdcall]<IntPtr, int, int, int, int*, int*, int> wglGetPixelFormatAttribivARB;

    static readonly int[] Names = {
        (int)PixelFormatAttributes.Acceleration,
        (int)PixelFormatAttributes.AccumAlphaBits,
        (int)PixelFormatAttributes.AccumBits,
        (int)PixelFormatAttributes.AccumBlueBits,
        (int)PixelFormatAttributes.AccumGreenBits,
        (int)PixelFormatAttributes.AccumRedBits,
        (int)PixelFormatAttributes.AlphaBits,
        (int)PixelFormatAttributes.AlphaShift,
        (int)PixelFormatAttributes.AuxBuffers,
        (int)PixelFormatAttributes.BlueBits,
        (int)PixelFormatAttributes.BlueShift,
        (int)PixelFormatAttributes.ColorBits,
        (int)PixelFormatAttributes.DepthBits,
        (int)PixelFormatAttributes.DoubleBuffer,
        (int)PixelFormatAttributes.DrawToBitmap,
        (int)PixelFormatAttributes.DrawToWindow,
        (int)PixelFormatAttributes.GreenBits,
        (int)PixelFormatAttributes.GreenShift,
        (int)PixelFormatAttributes.NeedPalette,
        (int)PixelFormatAttributes.NeedSystemPalette,
        (int)PixelFormatAttributes.OverlayCount,
        (int)PixelFormatAttributes.PixelType,
        (int)PixelFormatAttributes.RedBits,
        (int)PixelFormatAttributes.RedShift,
        (int)PixelFormatAttributes.StencilBits,
        (int)PixelFormatAttributes.Stereo,
        (int)PixelFormatAttributes.SupportGdi,
        (int)PixelFormatAttributes.SupportOpengl,
        (int)PixelFormatAttributes.SwapLayerBuffers,
        (int)PixelFormatAttributes.SwapMethod,
        (int)PixelFormatAttributes.Transparent,
        (int)PixelFormatAttributes.TransparentAlpha,
        (int)PixelFormatAttributes.TransparentBlue,
        (int)PixelFormatAttributes.TransparentGreen,
        (int)PixelFormatAttributes.TransparentIndex,
        (int)PixelFormatAttributes.TransparentRed,
        (int)PixelFormatAttributes.TransparentValue,
        (int)PixelFormatAttributes.UnderlayCount,
    };

    static readonly Type[] EnumTypes = {
        typeof(Acceleration),
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        typeof(PixelType),
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        typeof(SwapMethod),
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
    };

    static int[] Values = new int[Names.Length];

    static void TestPixelFormat (int pixelFormatIndex, TextWriter w) {
        using var f = new Form();
        using var dc = new DeviceContext(f.Handle);
        var dcPtr = (IntPtr)dc;
        var pfd = new PixelFormatDescriptor { size = PixelFormatDescriptor.Size, version = 1 };
        Gdi32.DescribePixelFormat(dc, pixelFormatIndex, ref pfd);
        Gdi32.SetPixelFormat(dc, pixelFormatIndex, ref pfd);
        var ctx = Opengl.CreateContext(dcPtr);
        if (IntPtr.Zero == ctx)
            throw new WinApiException("CreateContext");
        try {
            if (!Opengl.wglMakeCurrent(dcPtr, ctx))
                throw new WinApiException("wglMakeCurrent {ctx}");
            try {
                var fptr = Opengl.GetProcAddress(nameof(wglGetPixelFormatAttribivARB));
                if (IntPtr.Zero == fptr)
                    throw new WinApiException("GetProcAddress");
                w.Write("{0,-18}", pixelFormatIndex);
                wglGetPixelFormatAttribivARB = (delegate* unmanaged[Stdcall]<IntPtr, int, int, int, int*, int*, int>)fptr;
                int value = 0;
                int* v = &value;
                fixed (int* names = Names)
                    for (var i = 0; i < Names.Length; ++i)
                        if (0 != wglGetPixelFormatAttribivARB(dcPtr, pixelFormatIndex, 0, 1, names + i, v)) {
                            if (EnumTypes[i] is Type t) {
                                w.Write("{0,-18}", Enum.ToObject(t, *v));
                            } else {
                                w.Write("{0,-18}", *v);
                            }
                        } else
                            w.Write("{0,-18}", '-');
                w.WriteLine();
                Console.WriteLine($"{pixelFormatIndex}: {pfd}");
            } finally {
                if (!Opengl.wglMakeCurrent(dcPtr, IntPtr.Zero))
                    Console.WriteLine($"{pixelFormatIndex}: also failed to restore original context?");
            }
        } finally {
            Opengl.DeleteContext(ctx);
        }
    }

    [STAThread]
    static void Main () {
        var pixelFormatCount = GetPixelFormatCount();
        using var w = new StreamWriter("pf.txt", false, Encoding.ASCII) { NewLine = "\n" };
        w.Write("{0,-18}", "Index");
        for (var i = 0; i < Names.Length; ++i)
            w.Write("{0,-18}", (PixelFormatAttributes)Names[i]);
        w.WriteLine();
        for (var i = 1; i <= pixelFormatCount; ++i)
            try {
                TestPixelFormat(i, w);
            } catch (WinApiException e) {
                Console.WriteLine($"{i}: {e}");
            } catch (GlException e) {
                Console.WriteLine($"{i}: {e}");
            }
        Console.WriteLine("done");
        //_ = Console.ReadLine();
        //Debug.WriteLine(f.DialogResult);
        //Debug.WriteLine(f.PixelFormatDescriptor);
        //Debug.WriteLine(f.Index);
        //new BlitTest(new(1280,720)).Run();
        //new HighlightTriangle(new("data/teapot.obj", true), new(800,600)).Run();
    }
}

