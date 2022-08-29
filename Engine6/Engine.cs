namespace Engine6;

using System;
using System.Windows.Forms;
using Win32;
using Gl;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

//unsafe delegate nint wglCreateContextAttribsARB_Type (nint dc, nint sharedContext, int* attributeNameValuePairs);
//unsafe delegate int wglGetPixelFormatAttribivARB_Type (nint dc, int pixelFormatIndex, int layerPlane, int attributeCount, int* attributeNames, int* attributeValues);

unsafe class Engine6 {
    //const PixelFlag Required = PixelFlag.DrawToWindow | PixelFlag.SupportOpengl;
    //const PixelFlag Rejected = PixelFlag.GenericAccelerated | PixelFlag.GenericFormat | PixelFlag.NeedPalette | PixelFlag.NeedSystemPalette;


    //private static int GetPixelFormatCount () {
    //    using var f = new Form();
    //    using var dc = new DeviceContext(f.Handle);
    //    return Gdi32.GetPixelFormatCount(dc);
    //}

    //private static int GetPixelFormatCountARB () {
    //    var pixelFormatCount = GetPixelFormatCount();
    //    using var f = new Form();
    //    using var dc = new DeviceContext(f.Handle);
    //    var dcPtr = (IntPtr)dc;
    //    var pfd = new PixelFormatDescriptor { size = PixelFormatDescriptor.Size, version = 1 };
    //    var pixelFormatIndex = 0;
    //    for (var i = 1; i <= pixelFormatCount && 0 == pixelFormatIndex; ++i) {
    //        Gdi32.DescribePixelFormat(dc, i, ref pfd);
    //        if (0 == (Rejected & pfd.flags) && Required == (Required & pfd.flags))
    //            pixelFormatIndex = i;
    //    }

    //    var pixelFormatCountARB = 0;
    //    if (0 != pixelFormatIndex) {
    //        Gdi32.SetPixelFormat(dc, pixelFormatIndex, ref pfd);
    //        var rc = Opengl.CreateContext(dcPtr);
    //        var pa = Opengl.GetProcAddress("wglGetPixelFormatAttribivARB");
    //        if (0 != pa) {
    //            var wglGetPixelFormatAttribivARB = Marshal.GetDelegateForFunctionPointer<wglGetPixelFormatAttribivARB_Type>(pa);
    //            var name = (int)ContextAttrib.PixelFormatCount;
    //            if (0 == wglGetPixelFormatAttribivARB(dcPtr, pixelFormatIndex, 0, 1, &name, &pixelFormatCountARB)) {
    //                var err = Kernel32.GetLastError();
    //            }
    //        }
    //        Opengl.ReleaseCurrent(dc);
    //        Opengl.DeleteContext(rc);
    //    }
    //    return pixelFormatCountARB;
    //}


    //static readonly int[] Names = {
    //    (int)PixelFormatAttrib.ColorBits,
    //    (int)PixelFormatAttrib.DepthBits,
    //    (int)PixelFormatAttrib.StencilBits,
    //    (int)PixelFormatAttrib.DoubleBuffer,
    //    (int)PixelFormatAttrib.PixelType,
    //    (int)PixelFormatAttrib.SwapMethod,
    //};

    //static readonly Type[] Conversions = {
    //    null,
    //    null,
    //    null,
    //    typeof(bool),
    //    typeof(PixelType),
    //    typeof(SwapMethod),
    //};

    //static int[] Values = new int[Names.Length];
    //static int[] NameValuePairs = {
    //    (int)ContextAttrib.MajorVersion, 0,
    //    (int)ContextAttrib.MinorVersion, 0,
    //    (int)ContextAttrib.ContextFlags, 0,
    //    (int)ContextAttrib.ProfileMask, 0,
    //    0, 0,
    //};
    //static void TestPixelFormat (int requestedPixelFormatIndex, TextWriter w) {
    //    using var f = new Form();
    //    using var dc = new DeviceContext(f.Handle);
    //    var dcPtr = (IntPtr)dc;
    //    var requestedPfd = new PixelFormatDescriptor { size = PixelFormatDescriptor.Size, version = 1 };
    //    Gdi32.DescribePixelFormat(dc, requestedPixelFormatIndex, ref requestedPfd);
    //    Gdi32.SetPixelFormat(dc, requestedPixelFormatIndex, ref requestedPfd);
    //    var actualPixelFormatIndex = Gdi32.GetPixelFormat(dc);
    //    if (actualPixelFormatIndex != requestedPixelFormatIndex)
    //        throw new Exception($"requested pixelformat #{requestedPixelFormatIndex}, got #{actualPixelFormatIndex} instead");
    //    var ctx = Opengl.CreateContext(dcPtr);
    //    if (IntPtr.Zero == ctx)
    //        throw new WinApiException("CreateContext");
    //    try {
    //        Opengl.MakeCurrent(dc, ctx);
    //        try {
    //            var fptr = Opengl.GetProcAddress("wglGetPixelFormatAttribivARB");
    //            if (IntPtr.Zero == fptr)
    //                throw new WinApiException("GetProcAddress wglGetPixelFormatAttribivARB");
    //            w.Write(requestedPixelFormatIndex);
    //            var wglGetPixelFormatAttribivARB = Marshal.GetDelegateForFunctionPointer<wglGetPixelFormatAttribivARB_Type>(fptr);
    //            fptr = Opengl.GetProcAddress("wglCreateContextAttribsARB");
    //            if (IntPtr.Zero == fptr)
    //                throw new WinApiException("GetProcAddress wglCreateContextAttribsARB");
    //            var wglCreateContextAttribsARB = Marshal.GetDelegateForFunctionPointer<wglCreateContextAttribsARB_Type>(fptr);
    //            int value = 0;
    //            int* v = &value;
    //            fixed (int* names = Names)
    //                for (var i = 0; i < Names.Length; ++i)
    //                    if (0 != wglGetPixelFormatAttribivARB(dcPtr, requestedPixelFormatIndex, 0, 1, names + i, v))
    //                        if (Conversions[i] is Type t)
    //                            w.Write(",{0}", t.IsEnum ? Enum.ToObject(t, *v) : Convert.ChangeType(*v, t));
    //                        else
    //                            w.Write(",{0}", *v);
    //                    else
    //                        w.Write(",{0}", '-');

    //            NameValuePairs[1] = 4;
    //            NameValuePairs[3] = 6;
    //            NameValuePairs[5] = (int)ContextFlag.Debug;
    //            NameValuePairs[7] = (int)ProfileMask.Core;
    //            var arbCtx = IntPtr.Zero;
    //            fixed (int* nameValuePairs = NameValuePairs)
    //                arbCtx = wglCreateContextAttribsARB(dcPtr, 0, nameValuePairs);
    //            w.Write(", 0x{0:x}\n", arbCtx);
    //            if (0 != arbCtx)
    //                Opengl.DeleteContext(arbCtx);
    //        } finally {
    //            Opengl.ReleaseCurrent(dc);
    //        }
    //    } finally {
    //        Opengl.DeleteContext(ctx);
    //    }
    //}

    [STAThread]
    static void Main () {
        using var f = new GlForm();
        Application.Run(f);
        //using var f = new NoiseTest();
        //f.Run();
        //using var f = new GdiWindow();
        //f.Run();
        //var pixelFormatCount = GetPixelFormatCountARB();
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

