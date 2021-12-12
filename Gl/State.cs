namespace Gl;

using System;
using System.Diagnostics;
using static Opengl;

public sealed class State {
    private static void DebugProc (DebugSource sourceEnum, DebugType typeEnum, int id, DebugSeverity severityEnum, int length, IntPtr message, IntPtr userParam) {
        Debugger.Break();
    }
    private static readonly DebugProc debugProc;
    static State () => debugProc = DebugProc;
    private static void MaybeToggle (Capability cap, bool requested) {
        var previous = IsEnabled(cap);
        if (requested != previous) {
            if (requested)
                Opengl.Enable(cap);
            else
                Opengl.Disable(cap);
            if (IsEnabled(cap) != requested)
                throw new Exception();
        }
    }

    public static int SwapInterval {
        get => GetSwapIntervalEXT();
        set {
            if (value != SwapInterval)
                if (!SwapIntervalEXT(value))
                    throw new Exception();
            if (value != SwapInterval)
                throw new Exception();
        }
    }

    public static int ActiveTexture {
        get => GetIntegerv(IntParameter.ActiveTexture) - Const.TEXTURE0;
        set {
            if (value != ActiveTexture)
                ActiveTexture(Const.TEXTURE0 + value);
        }
    }

    public static bool Blend {
        get => IsEnabled(Capability.Blend);
        set => MaybeToggle(Capability.Blend, value);
    }
    public static bool DepthTest {
        get => IsEnabled(Capability.DepthTest);
        set => MaybeToggle(Capability.DepthTest, value);
    }
    public static bool LineSmooth {
        get => IsEnabled(Capability.LineSmooth);
        set => MaybeToggle(Capability.LineSmooth, value);
    }
    public static bool Dither {
        get => IsEnabled(Capability.Dither);
        set => MaybeToggle(Capability.Dither, value);
    }
    public static bool DebugOutput {
        get => IsEnabled(Capability.DebugOutput);
        set {
            MaybeToggle(Capability.DebugOutput, value);
            Gl.Opengl.DebugMessageCallback(value ? debugProc : null, IntPtr.Zero);
        }
    }
    public static bool CullFace {
        get => IsEnabled(Capability.CullFace);
        set => MaybeToggle(Capability.CullFace, value);
    }

    public static DepthFunction DepthFunc {
        get => (DepthFunction)GetIntegerv(IntParameter.DepthFunc);
        set {
            if (value != DepthFunc)
                DepthFunc(value);
            if (value != DepthFunc)
                throw new Exception();
        }
    }
    public static int Framebuffer {
        get => GetIntegerv(IntParameter.FramebufferBinding);
        set {
            if (value != Framebuffer)
                BindFramebuffer(Const.FRAMEBUFFER, value);
            if (value != Framebuffer)
                throw new Exception();
        }
    }
    public static int Program {
        get => GetIntegerv(IntParameter.CurrentProgram);
        set {
            if (value != Program)
                UseProgram(value);
            if (value != Program)
                throw new Exception();
        }
    }
    public static int ArrayBuffer {
        get => GetIntegerv(IntParameter.ArrayBufferBinding);
        set {
            if (value != ArrayBuffer)
                BindBuffer(BufferTarget.Array, value);
            if (value != ArrayBuffer)
                throw new Exception();
        }
    }
    public static int VertexArray {
        get => GetIntegerv(IntParameter.VertexArrayBinding);
        set {
            if (value != VertexArray)
                BindVertexArray(value);
            if (value != VertexArray)
                throw new Exception();
        }
    }
}
