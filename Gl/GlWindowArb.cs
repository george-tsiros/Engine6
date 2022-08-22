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

    public GlWindowArb (Version shaderVersion = null) {
        shaderVersion ??= Opengl.ShaderVersion;
        var extendedFormatCount = Opengl.GetPixelFormatCount((IntPtr)Dc, 1, 0, 1);
        var attributeNames = new int[] {
            (int)PixelFormatAttributes.Acceleration,
            (int)PixelFormatAttributes.ColorBits,
            (int)PixelFormatAttributes.DepthBits,
            (int)PixelFormatAttributes.DoubleBuffer,
            (int)PixelFormatAttributes.DrawToWindow,
            (int)PixelFormatAttributes.PixelType,
            (int)PixelFormatAttributes.SupportOpengl,
        };

        var attributeValues = new int[attributeNames.Length];
        var attributeNameValuePairs = new int[attributeNames.Length * 2 + 2 + 8];
        attributeNameValuePairs[0] = (int)ContextAttributes.ProfileMask;
        attributeNameValuePairs[2] = (int)ContextAttributes.MajorVersion;
        attributeNameValuePairs[4] = (int)ContextAttributes.MinorVersion;
        attributeNameValuePairs[6] = (int)ContextAttributes.ContextFlags;
        var candidates = new List<int>();
        for (var i = 1; i <= extendedFormatCount; ++i) {
            Opengl.GetPixelFormatAttribivARB((IntPtr)Dc, i, 0, attributeNames.Length, attributeNames, attributeValues);
            if ((int)Acceleration.Full == attributeValues[0] && 24 <= attributeValues[1] && 24 == attributeValues[2] && 0 != attributeValues[3] && 0 != attributeValues[4] && (int)PixelType.Rgba == attributeValues[5] && 0 != attributeValues[6])
                candidates.Add(i);
        }

        if (0 == candidates.Count)
            throw new Exception("failed to find any pixel formats");

        var pfd = new PixelFormatDescriptor { size = PixelFormatDescriptor.Size, version = 1 };
        bool windowUsed = true;
        const ProfileMask profileMask = ProfileMask.Core;
        foreach (var index in candidates) {
            if (windowUsed) {
                Create();
                windowUsed = false;
            }
            try {
                Gdi32.DescribePixelFormat(Dc, index, ref pfd);
                Gdi32.SetPixelFormat(Dc, index, ref pfd);

                windowUsed = true;

                for (var i = 0; i < attributeNames.Length; ++i)
                    attributeNameValuePairs[attributeNames.Length + 2 * i + 1] = attributeValues[i];

                attributeNameValuePairs[1] = (int)profileMask;
                attributeNameValuePairs[3] = shaderVersion?.Major ?? Opengl.ShaderVersion.Major;
                attributeNameValuePairs[5] = shaderVersion?.Minor ?? Opengl.ShaderVersion.Minor;
                attributeNameValuePairs[7] = (int)(ContextFlags.Debug | ContextFlags.ForwardCompatible);

                var candidateContext = Opengl.CreateContextAttribsARB((IntPtr)Dc, IntPtr.Zero, attributeNameValuePairs);
                Opengl.MakeCurrent((IntPtr)Dc, candidateContext);
                RenderingContext = candidateContext;
                if (profileMask != Opengl.Profile) {
                    continue;
                }
                if (shaderVersion is Version v && (v.Major != Opengl.ShaderVersion.Major || v.Minor != Opengl.ShaderVersion.Minor)) {
                    continue;
                }
                var initial = Opengl.IsEnabled(Capability.DepthTest);
                if (initial)
                    Opengl.Disable(Capability.DepthTest);
                else
                    Opengl.Enable(Capability.DepthTest);
                var toggled = Opengl.IsEnabled(Capability.DepthTest);
                if (initial == toggled) {
                    continue;
                }
                if (toggled)
                    Opengl.Disable(Capability.DepthTest);
                else
                    Opengl.Enable(Capability.DepthTest);
                var restored = Opengl.IsEnabled(Capability.DepthTest);
                if (initial != restored) {
                    continue;
                }
                foreach (var e in Opengl.SupportedExtensions)
                    Debug.WriteLine(e);
                break;
            } catch (Exception e) when (e is GlException || e is WinApiException) {
            }
        }
    }
    protected override void Create () {
        if (IntPtr.Zero != Opengl.GetCurrentContext())
            Opengl.ReleaseCurrent((IntPtr)Dc);
        if (IntPtr.Zero != RenderingContext)
            Opengl.DeleteContext(RenderingContext);
        RenderingContext = IntPtr.Zero;
        base.Create();
    }
}
