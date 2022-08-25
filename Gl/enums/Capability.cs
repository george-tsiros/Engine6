namespace Gl;

public enum Capability:uint {
    Blend = Const.BLEND,
    CullFace = Const.CULL_FACE,
    DebugOutput = Const.DEBUG_OUTPUT,
    DebugOutputSynchronous = Const.DEBUG_OUTPUT_SYNCHRONOUS,
    DepthTest = Const.DEPTH_TEST,
    Dither = Const.DITHER,
    LineSmooth = Const.LINE_SMOOTH,
    StencilTest = Const.STENCIL_TEST,
    ScissorTest = Const.SCISSOR_TEST,
    PointSize = Const.POINT_SIZE,
}
