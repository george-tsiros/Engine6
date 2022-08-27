namespace Gl;

using System;
using System.Diagnostics;
using static Opengl;

public sealed class State {

    private static string SetBoolFailed (string name, bool value) => $"failed to turn {name} {(value ? "on" : "off")}";
    private static string SetInt32Failed (string name, int value) => $"failed to set {name} to {value}";
    private static string SetEnumFailed<T> (T value) where T : Enum => $"failed to set {typeof(T)} to {value}";
    private static void DebugProc (DebugSource sourceEnum, DebugType typeEnum, int id, DebugSeverity severityEnum, int length, IntPtr message, IntPtr userParam) {
        Debug.WriteLine($"{nameof(DebugSource)}: {sourceEnum}");
        Debug.WriteLine($"{nameof(DebugType)}: {typeEnum}");
        Debug.WriteLine($"Id: {id}");
        Debug.WriteLine($"{nameof(DebugSeverity)}: {severityEnum}");
        Debug.WriteLine(System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message));
    }

    private static readonly DebugProc debugProc;

    static State () {
        debugProc = DebugProc;
    }

    private static void MaybeToggle (Capability cap, bool requested) {
        if (requested != IsEnabled(cap)) {
            if (requested)
                Enable(cap);
            else
                Disable(cap);
        }
        if (requested != IsEnabled(cap)) {
            throw new GlException(SetBoolFailed(cap.ToString(), requested));
        }
    }

    public static int SwapInterval {
        get => GetSwapIntervalEXT();
        set {
            if (value != SwapInterval) {
                if (SwapIntervalEXT(value)) {
                    if (value != SwapInterval)
                        throw new GlException(SetInt32Failed(nameof(SwapInterval), value));
                } else
                    throw new GlException(SetInt32Failed(nameof(SwapInterval), value));
            }
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
            DebugMessageCallback(value ? debugProc : null, IntPtr.Zero);
        }
    }
    public static bool CullFace {
        get => IsEnabled(Capability.CullFace);
        set => MaybeToggle(Capability.CullFace, value);
    }

    public static bool DepthMask {
        get => 0 != GetIntegerv(IntParameter.DepthMask);
        set {
            if (value != DepthMask) {
                DepthMask(value);
                if (value != DepthMask)
                    throw new GlException(SetBoolFailed(nameof(IntParameter.DepthMask), value));
            }
        }
    }

    public static DepthFunction DepthFunc {
        get => (DepthFunction)GetIntegerv(IntParameter.DepthFunc);
        set {
            if (value != DepthFunc) {
                DepthFunc(value);
                if (value != DepthFunc)
                    throw new GlException(SetEnumFailed(value));
            }
        }
    }
    public static int FramebufferBinding {
        get => GetIntegerv(IntParameter.FramebufferBinding);
        set {
            if (value != FramebufferBinding) {
                BindFramebuffer(FramebufferTarget.Framebuffer, value);
                if (value != FramebufferBinding)
                    throw new GlException(SetInt32Failed(nameof(FramebufferTarget.Framebuffer), value));
            }
        }
    }
    //public static int CurrentProgram {
    //    get => GetIntegerv(IntParameter.CurrentProgram);
    //    set {
    //        if (value != CurrentProgram) {
    //            UseProgram(value);
    //            if (value != CurrentProgram)
    //                throw new GlException(SetInt32Failed(nameof(IntParameter.CurrentProgram), value));
    //        }
    //    }
    //}
    public static int ArrayBufferBinding {
        get => GetIntegerv(IntParameter.ArrayBufferBinding);
        set {
            if (value != ArrayBufferBinding) {
                BindBuffer(BufferTarget.Array, value);
                if (value != ArrayBufferBinding)
                    throw new GlException(SetInt32Failed(nameof(IntParameter.ArrayBufferBinding), value));
            }
        }
    }
    public static int VertexArrayBinding {
        get => GetIntegerv(IntParameter.VertexArrayBinding);
        set {
            if (value != VertexArrayBinding) {
                BindVertexArray(value);
                if (value != VertexArrayBinding)
                    throw new GlException(SetInt32Failed(nameof(IntParameter.VertexArrayBinding), value));
            }
        }
    }
}
