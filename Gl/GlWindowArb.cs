namespace Gl;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Win32;
using Linear;
using System.Diagnostics;
using System.Drawing;

public class GlWindowArb:GlWindow {

    public GlWindowArb (Vector2i size, Vector2i? position = null) : base(size, position) {

        var extendedFormatCount = Opengl.GetPixelFormatCount(DeviceContext, 1, 0, 1);
        var attributeNames = new int[] {
            (int)PixelFormatAttributes.Acceleration,
            (int)PixelFormatAttributes.ColorBits,
            (int)PixelFormatAttributes.DepthBits,
            (int)PixelFormatAttributes.DoubleBuffer,
            (int)PixelFormatAttributes.DrawToWindow,
            (int)PixelFormatAttributes.PixelType,
        };

        var attributeValues = new int[attributeNames.Length];
        var attributeNameValuePairs = new int[attributeNames.Length * 2 + 2 + 8];
        attributeNameValuePairs[attributeNames.Length * 2] = (int)ContextAttributes.ContextFlags;
        attributeNameValuePairs[attributeNames.Length * 2 + 2] = (int)ContextAttributes.MajorVersion;
        attributeNameValuePairs[attributeNames.Length * 2 + 4] = (int)ContextAttributes.MinorVersion;
        attributeNameValuePairs[attributeNames.Length * 2 + 6] = (int)ContextAttributes.ProfileMask;
        var candidates = new List<int>();
        for (var i = 1; i <= extendedFormatCount; ++i) {
            Opengl.GetPixelFormatAttribivARB(DeviceContext, i, 0, attributeNames.Length, attributeNames, attributeValues);
            if ((int)Acceleration.Full == attributeValues[0] && 32 == attributeValues[1] && 24 == attributeValues[2] && 0 != attributeValues[3] && 0 != attributeValues[4] && (int)PixelType.Rgba == attributeValues[5])
                candidates.Add(i);
        }

        if (0 == candidates.Count)
            throw new Exception("failed to find any pixel formats");

        var pfd = new PixelFormatDescriptor { size = PixelFormatDescriptor.Size, version = 1 };
        bool windowUsed = true;
        const ProfileMask profileMask = ProfileMask.Core;
        foreach (var index in candidates) {
            if (windowUsed) {
                Recreate();
                windowUsed = false;
            }
            try {
                Gdi.DescribePixelFormat(DeviceContext, index, ref pfd);
                if (!Gdi.SetPixelFormat(DeviceContext, index, ref pfd))
                    throw new WinApiException(nameof(Gdi.SetPixelFormat));

                windowUsed = true;

                for (var i = 0; i < attributeNames.Length; ++i)
                    attributeNameValuePairs[2 * i + 1] = attributeValues[i];

                attributeNameValuePairs[attributeNames.Length * 2 + 1] = (int)ContextFlags.Debug ;// (int)(ContextFlags.Debug | ContextFlags.ForwardCompatible);
                attributeNameValuePairs[attributeNames.Length * 2 + 3] = Opengl.ShaderVersion.Major;
                attributeNameValuePairs[attributeNames.Length * 2 + 5] = Opengl.ShaderVersion.Minor;
                attributeNameValuePairs[attributeNames.Length * 2 + 7] = (int)profileMask;

                var candidateContext = Opengl.CreateContextAttribsARB(DeviceContext, IntPtr.Zero, attributeNameValuePairs);
                Opengl.MakeCurrent(DeviceContext, candidateContext);
                if (profileMask != Opengl.Profile)
                    throw new GlException($"requested {profileMask}, got {Opengl.Profile}");
                RenderingContext = candidateContext;
                var initial = Opengl.IsEnabled(Capability.DepthTest);
                if (initial)
                    Opengl.Disable(Capability.DepthTest);
                else
                    Opengl.Enable(Capability.DepthTest);
                var toggled = Opengl.IsEnabled(Capability.DepthTest);
                if (initial == toggled)
                    throw new GlException();
                if (toggled)
                    Opengl.Disable(Capability.DepthTest);
                else
                    Opengl.Enable(Capability.DepthTest);
                var restored = Opengl.IsEnabled(Capability.DepthTest);
                if (initial == restored) {
                    Debug.WriteLine($"{index} works");
                    Debug.WriteLine(System.Runtime.InteropServices.Marshal.PtrToStringAnsi(Opengl.GetString(OpenglString.Renderer)));
                    Debug.WriteLine(System.Runtime.InteropServices.Marshal.PtrToStringAnsi(Opengl.GetString(OpenglString.Version)));
                    break;
                }
            } catch (Exception e) when (e is GlException || e is WinApiException) {
                Debug.WriteLine(e);
            }
        }
    }
    void Recreate () {
        if (IntPtr.Zero != Opengl.GetCurrentContext())
            Opengl.ReleaseCurrent(DeviceContext);
        if (IntPtr.Zero != RenderingContext)
            Opengl.DeleteContext(RenderingContext);
        if (!User.ReleaseDC(WindowHandle, DeviceContext))
            throw new WinApiException(nameof(User.ReleaseDC));
        if (!User.DestroyWindow(WindowHandle))
            throw new WinApiException(nameof(User.DestroyWindow));
        WindowHandle = User.CreateWindow(ClassAtom, new(new(), Size), SelfHandle);
        if (IntPtr.Zero == WindowHandle)
            throw new WinApiException(nameof(User.CreateWindow));
        DeviceContext = User.GetDC(WindowHandle);
        if (IntPtr.Zero == DeviceContext)
            throw new WinApiException(nameof(User.GetDC));
    }
    static string OnOff (bool yes) => yes ? "on" : "off";

}
