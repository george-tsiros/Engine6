namespace Gl;

public enum BufferTarget {
    Array = Const.ARRAY_BUFFER,
    AtomicCounter = Const.ATOMIC_COUNTER_BUFFER,
    CopyRead = Const.COPY_READ_BUFFER,
    CopyWrite = Const.COPY_WRITE_BUFFER,
    DispatchIndirect = Const.DISPATCH_INDIRECT_BUFFER,
    DrawIndirect = Const.DRAW_INDIRECT_BUFFER,
    ElementArray = Const.ELEMENT_ARRAY_BUFFER,
    PixelPack = Const.PIXEL_PACK_BUFFER,
    PixelUnpack = Const.PIXEL_UNPACK_BUFFER,
    Query = Const.QUERY_BUFFER,
    ShaderStorage = Const.SHADER_STORAGE_BUFFER,
    Texture = Const.TEXTURE_BUFFER,
    TransformFeedback_Buffer = Const.TRANSFORM_FEEDBACK_BUFFER,
    Uniform = Const.UNIFORM_BUFFER,
}
