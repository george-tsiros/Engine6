namespace Gl;

public enum DebugType {
    Error = Const.GL_DEBUG_TYPE_ERROR,
    DeprecatedBehavior = Const.GL_DEBUG_TYPE_DEPRECATED_BEHAVIOR,
    UndefinedBehavior = Const.GL_DEBUG_TYPE_UNDEFINED_BEHAVIOR,
    Portability = Const.GL_DEBUG_TYPE_PORTABILITY,
    Performance = Const.GL_DEBUG_TYPE_PERFORMANCE,
    Marker = Const.GL_DEBUG_TYPE_MARKER,
    PushGroup = Const.GL_DEBUG_TYPE_PUSH_GROUP,
    PopGroup = Const.GL_DEBUG_TYPE_POP_GROUP,
    Other = Const.GL_DEBUG_TYPE_OTHER
}
