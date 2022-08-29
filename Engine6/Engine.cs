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
        return Gdi32.GetPixelFormatCount(dc);
    }


    //private static delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int*, IntPtr> wglCreateContextAttribsARB_Type;
    private static delegate* unmanaged[Stdcall]<IntPtr, int, int, int, int*, int*, int> wglGetPixelFormatAttribivARB;

    static readonly int[] Names = {
        (int)PixelFormatAttrib.ColorBits,
        (int)PixelFormatAttrib.DepthBits,
        (int)PixelFormatAttrib.StencilBits,
        (int)PixelFormatAttrib.DoubleBuffer,
        (int)PixelFormatAttrib.PixelType,
        (int)PixelFormatAttrib.SwapMethod,
    };

    static readonly Type[] Conversions = {
        null,
        null,
        null,
        typeof(bool),
        typeof(PixelType),
        typeof(SwapMethod),
    };

    static int[] Values = new int[Names.Length];

    static void TestPixelFormat (int requestedPixelFormatIndex, TextWriter w) {
        using var f = new Form();
        using var dc = new DeviceContext(f.Handle);
        var dcPtr = (IntPtr)dc;
        var requestedPfd = new PixelFormatDescriptor { size = PixelFormatDescriptor.Size, version = 1 };
        Gdi32.DescribePixelFormat(dc, requestedPixelFormatIndex, ref requestedPfd);
        Gdi32.SetPixelFormat(dc, requestedPixelFormatIndex, ref requestedPfd);
        var actualPixelFormatIndex = Gdi32.GetPixelFormat(dc);
        if (actualPixelFormatIndex != requestedPixelFormatIndex)
            throw new Exception($"requested pixelformat #{requestedPixelFormatIndex}, got #{actualPixelFormatIndex} instead");
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
                w.Write(requestedPixelFormatIndex);
                wglGetPixelFormatAttribivARB = (delegate* unmanaged[Stdcall]<IntPtr, int, int, int, int*, int*, int>)fptr;
                int value = 0;
                int* v = &value;
                fixed (int* names = Names)
                    for (var i = 0; i < Names.Length; ++i)
                        if (0 != wglGetPixelFormatAttribivARB(dcPtr, requestedPixelFormatIndex, 0, 1, names + i, v))
                            if (Conversions[i] is Type t)
                                w.Write(",{0}", t.IsEnum ? Enum.ToObject(t, *v) : Convert.ChangeType(*v, t));
                            else
                                w.Write(",{0}", *v);
                        else
                            w.Write(",{0}", '-');
                w.WriteLine();
                Console.WriteLine($"{requestedPixelFormatIndex}: {requestedPfd}");
            } finally {
                if (!Opengl.wglMakeCurrent(dcPtr, IntPtr.Zero))
                    Console.WriteLine($"{requestedPixelFormatIndex}: also failed to restore original context?");
            }
        } finally {
            Opengl.DeleteContext(ctx);
        }
    }

    [STAThread]
    static void Main () {
        using var f = new NoiseTest();
        f.Run();
        //using var f = new GdiWindow();
        //f.Run();
        //var pixelFormatCount = GetPixelFormatCount();
        //using var w = new StreamWriter("pf.txt", false, Encoding.ASCII) { NewLine = "\n" };
        //w.Write("Index");
        //for (var i = 0; i < Names.Length; ++i)
        //    w.Write(",{0}", (PixelFormatAttrib)Names[i]);
        //w.WriteLine();
        //for (var i = 1; i <= pixelFormatCount; ++i)
        //    try {
        //        TestPixelFormat(i, w);
        //    } catch (WinApiException e) {
        //        Console.WriteLine($"{i}: {e}");
        //    } catch (GlException e) {
        //        Console.WriteLine($"{i}: {e}");
        //    } catch (Exception e) {
        //        Console.WriteLine($"{i}: {e}");
        //    }
        //Console.WriteLine(f.DialogResult);
        //Console.WriteLine(f.PixelFormatDescriptor);
        //Console.WriteLine(f.Index);
        //new BlitTest(new(1280,720)).Run();
        //new HighlightTriangle(new("data/teapot.obj", true), new(800,600)).Run();
    }
}

