namespace Gl;

public enum FramebufferStatus:int {
    Error = 0,
    Undefined = Const.FRAMEBUFFER_UNDEFINED,
    Complete = Const.FRAMEBUFFER_COMPLETE,
    IncompleteAttachment = Const.FRAMEBUFFER_INCOMPLETE_ATTACHMENT,
    IncompleteMissingAttachment = Const.FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT,
    IncompleteDimensionsExt = Const.FRAMEBUFFER_INCOMPLETE_DIMENSIONS_EXT,
    IncompleteFormatsExt = Const.FRAMEBUFFER_INCOMPLETE_FORMATS_EXT,
    IncompleteDrawBuffer = Const.FRAMEBUFFER_INCOMPLETE_DRAW_BUFFER,
    IncompleteReadBuffer = Const.FRAMEBUFFER_INCOMPLETE_READ_BUFFER,
    Unsupported = Const.FRAMEBUFFER_UNSUPPORTED,
    UnknownError = 0x8cde,
    IncompleteMultisample = Const.FRAMEBUFFER_INCOMPLETE_MULTISAMPLE,
    IncompleteLayerTargets = Const.FRAMEBUFFER_INCOMPLETE_LAYER_TARGETS,
    IncompleteLayerCount = Const.FRAMEBUFFER_INCOMPLETE_LAYER_COUNT,
}
