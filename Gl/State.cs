namespace Gl;

using System.Diagnostics;
using static Opengl;

public sealed class State {
    private static void MaybeToggle (Capability cap, bool requested) {
        var previous = IsEnabled(cap);
        if (requested != previous) {
            if (requested)
                Opengl.Enable(cap);
            else
                Opengl.Disable(cap);
            Debug.Assert(IsEnabled(cap) == requested);
        }
    }

    public static int ActiveTexture {
        get => GetIntegerv(IntParameter.ActiveTexture) - Const.TEXTURE0;
        set {
            if (ActiveTexture != value)
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
        set => MaybeToggle(Capability.DebugOutput, value);
    }
    public static bool CullFace {
        get => IsEnabled(Capability.CullFace);
        set => MaybeToggle(Capability.CullFace, value);
    }

    public static DepthFunction DepthFunc {
        get => (DepthFunction)GetIntegerv(IntParameter.DepthFunc);
        set {
            if (DepthFunc != value)
                DepthFunc(value);
        }
    }
    public static int Framebuffer {
        get => GetIntegerv(IntParameter.FramebufferBinding);
        set {
            if (Framebuffer != value)
                BindFramebuffer(Const.FRAMEBUFFER, value);
        }
    }
    public static int Program {
        get => GetIntegerv( IntParameter.CurrentProgram);
        set {
            if (value != Program)
                UseProgram(value);
        }
    }
    public static int ArrayBuffer {
        get => GetIntegerv( IntParameter.ArrayBufferBinding);
        set {
            if (value != ArrayBuffer)
                BindBuffer(BufferTarget.Array, value);
        }
    }
    public static int VertexArray {
        get => GetIntegerv(IntParameter.VertexArrayBinding);
        set {
            if (value != VertexArray)
                BindVertexArray(value);
        }
    }
}
