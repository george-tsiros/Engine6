namespace Gl;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Win32;
using Common;
using System.Diagnostics;
using System.Drawing;

public class GlWindowArb:Window {
    protected IntPtr RenderingContext;
    protected long FramesRendered { get; private set; }
    protected long LastSync { get; private set; }
    const PixelFlag Required = PixelFlag.DrawToWindow | PixelFlag.SupportOpengl;
    const PixelFlag Rejected = PixelFlag.GenericFormat | PixelFlag.GenericAccelerated;
    public GlWindowArb (ContextConfigurationARB? configuration = null, Vector2i? size = null) : base(size) {
        var config = configuration ?? ContextConfigurationARB.Default;
        var pfdIndices = new List<int>();
        var pfdCount = Gdi32.GetPixelFormatCount(Dc);
        var pfd = new PixelFormatDescriptor { size = PixelFormatDescriptor.Size, version = 1 };
        var requireDoubleBuffer = config.BasicConfiguration.DoubleBuffer is bool _0 && _0 ? PixelFlag.DoubleBuffer : PixelFlag.None;
        var rejectDoubleBuffer = config.BasicConfiguration.DoubleBuffer is bool _1 && !_1 ? PixelFlag.DoubleBuffer : PixelFlag.None;
        var requireComposited = config.BasicConfiguration.Composited is bool _2 && _2 ? PixelFlag.SupportComposition : PixelFlag.None;
        var rejectComposited = config.BasicConfiguration.Composited is bool _3 && !_3 ? PixelFlag.SupportComposition : PixelFlag.None;
        var required = Required | requireDoubleBuffer | requireComposited;
        var rejected = Rejected | rejectDoubleBuffer | rejectComposited;

        for (var i = 1; i <= pfdCount; ++i) {
            Gdi32.DescribePixelFormat(Dc, i, ref pfd);
            if (0 != (pfd.flags & rejected) || required != (pfd.flags & required))
                continue;
            if (config.BasicConfiguration.ColorBits is int colorBits && pfd.colorBits != colorBits)
                continue;
            if (config.BasicConfiguration.DepthBits is int depthBits && pfd.depthBits != depthBits)
                continue;
            pfdIndices.Add(i);
        }
        var nameValuePairs = new Dictionary<ContextAttrib, int>();
        if (config.Flags is ContextFlag _6) {
            nameValuePairs.Add(ContextAttrib.ContextFlags, (int)_6);
        }
        if (config.Profile is ProfileMask _7) {
            nameValuePairs.Add(ContextAttrib.ProfileMask, (int)_7);
        }
        if (config.SwapMethod is SwapMethod _8) {
            nameValuePairs.Add(ContextAttrib.SwapMethod, (int)_8);
        }
        if (config.Version is Version _9) {
            nameValuePairs.Add(ContextAttrib.MajorVersion, _9.Major);
            nameValuePairs.Add(ContextAttrib.MinorVersion, _9.Minor);
        }
        var keys = new ContextAttrib[nameValuePairs.Count];
        nameValuePairs.Keys.CopyTo(keys, 0);
        var names = Array.ConvertAll(keys, k => (int)k);
        var values = new int[names.Length];

        foreach (var i in pfdIndices) {
            nativeWindow.Dispose();
            nativeWindow = new(WndProc, new(640, 480));
            Gdi32.DescribePixelFormat(Dc, i, ref pfd);
            Gdi32.SetPixelFormat(Dc, i, ref pfd);
            var ctx = Opengl.CreateContext((IntPtr)Dc);
            try {
                Debug.Assert(IntPtr.Zero != ctx);
                Opengl.MakeCurrent(Dc, ctx);
                try {
                    Opengl.GetPixelFormatAttribivARB((IntPtr)Dc, i, names, values);
                } finally {
                    Opengl.ReleaseCurrent(Dc);
                }
            } finally {
                Opengl.DeleteContext(ctx);
            }
        }
        //if (config.BasicConfiguration is ContextConfiguration basicConfig) {
        //    if (basicConfig.ColorBits is int colorBits)
        //        nameValuePairs.Add((PixelFormatAttrib.ColorBits, colorBits));
        //    //if (basicConfig.Composited is bool composited)
        //        //nameValuePairs.Add((PixelFormatAttrib.c, colorBits));
        //}

        //var extendedFormatCount = Opengl.GetPixelFormatCountARB(Dc);
        //var attributeNames = new int[] {
        //    (int)PixelFormatAttrib.Acceleration,
        //    (int)PixelFormatAttrib.ColorBits,
        //    (int)PixelFormatAttrib.DepthBits,
        //    (int)PixelFormatAttrib.DoubleBuffer,
        //    (int)PixelFormatAttrib.DrawToWindow,
        //    (int)PixelFormatAttrib.PixelType,
        //    (int)PixelFormatAttrib.SupportOpengl,
        //};

        //var attributeValues = new int[attributeNames.Length];
        //var attributeNameValuePairs = new int[attributeNames.Length * 2 + 2 + 8];
        //attributeNameValuePairs[0] = (int)ContextAttrib.ProfileMask;
        //attributeNameValuePairs[2] = (int)ContextAttrib.MajorVersion;
        //attributeNameValuePairs[4] = (int)ContextAttrib.MinorVersion;
        //attributeNameValuePairs[6] = (int)ContextAttrib.ContextFlags;
        //var candidates = new List<int>();
        //for (var i = 1; i <= extendedFormatCount; ++i) {
        //    Opengl.GetPixelFormatAttribivARB((IntPtr)Dc, i, attributeNames, attributeValues);
        //    if ((int)Acceleration.Full == attributeValues[0] && 24 <= attributeValues[1] && 24 == attributeValues[2] && 0 != attributeValues[3] && 0 != attributeValues[4] && (int)PixelType.Rgba == attributeValues[5] && 0 != attributeValues[6])
        //        candidates.Add(i);
        //}

        //if (0 == candidates.Count)
        //    throw new Exception("failed to find any pixel formats");

        //var pfd = new PixelFormatDescriptor { size = PixelFormatDescriptor.Size, version = 1 };
        //const ProfileMask profileMask = ProfileMask.Core;
        //foreach (var index in candidates) {
        //    try {
        //        Gdi32.DescribePixelFormat(Dc, index, ref pfd);
        //        Gdi32.SetPixelFormat(Dc, index, ref pfd);

        //        for (var i = 0; i < attributeNames.Length; ++i)
        //            attributeNameValuePairs[attributeNames.Length + 2 * i + 1] = attributeValues[i];

        //        attributeNameValuePairs[1] = (int)profileMask;
        //        //attributeNameValuePairs[3] = shaderVersion?.Major ?? Opengl.ShaderVersion.Major;
        //        //attributeNameValuePairs[5] = shaderVersion?.Minor ?? Opengl.ShaderVersion.Minor;
        //        attributeNameValuePairs[7] = (int)(ContextFlag.Debug | ContextFlag.ForwardCompatible);

        //        var candidateContext = Opengl.CreateContextAttribsARB((IntPtr)Dc, IntPtr.Zero, attributeNameValuePairs);
        //        Opengl.MakeCurrent(Dc, candidateContext);
        //        RenderingContext = candidateContext;
        //        if (profileMask != Opengl.Profile) {
        //            continue;
        //        }
        //        //if (shaderVersion is Version v && (v.Major != Opengl.ShaderVersion.Major || v.Minor != Opengl.ShaderVersion.Minor)) {
        //        //    continue;
        //        //}
        //        var initial = Opengl.IsEnabled(Capability.DepthTest);
        //        if (initial)
        //            Opengl.Disable(Capability.DepthTest);
        //        else
        //            Opengl.Enable(Capability.DepthTest);
        //        var toggled = Opengl.IsEnabled(Capability.DepthTest);
        //        if (initial == toggled) {
        //            continue;
        //        }
        //        if (toggled)
        //            Opengl.Disable(Capability.DepthTest);
        //        else
        //            Opengl.Enable(Capability.DepthTest);
        //        var restored = Opengl.IsEnabled(Capability.DepthTest);
        //        if (initial != restored) {
        //            continue;
        //        }
        //        foreach (var e in Opengl.SupportedExtensions)
        //            Debug.WriteLine(e);
        //        break;
        //    } catch (Exception e) when (e is GlException || e is WinApiException) {
        //    }
        //}
    }

    protected override void OnPaint () {
        Render();
        Gdi32.SwapBuffers(Dc);
        LastSync = Stopwatch.GetTimestamp();
        ++FramesRendered;
        Invalidate();
    }

    protected virtual void Render () {
        Opengl.ClearColor(0.5f, 0.5f, 0.5f, 1f);
        Opengl.Clear(BufferBit.ColorDepth);
    }
}
