namespace Gl;

public enum IntParameter:uint {
    ReadFramebufferBinding = Const.GL_READ_FRAMEBUFFER_BINDING,
    DrawFramebufferBinding = Const.GL_DRAW_FRAMEBUFFER_BINDING,
    DepthFunc = Const.GL_DEPTH_FUNC,
    DepthMask = Const.GL_DEPTH_WRITEMASK,
    CurrentProgram = Const.GL_CURRENT_PROGRAM,
    ArrayBufferBinding = Const.GL_ARRAY_BUFFER_BINDING,
    VertexArrayBinding = Const.GL_VERTEX_ARRAY_BINDING,
    ActiveTexture = Const.GL_ACTIVE_TEXTURE,
    NumExtensions = Const.GL_NUM_EXTENSIONS,
}
