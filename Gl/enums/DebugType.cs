namespace Gl;

public enum DebugType {
    Error = Const.DEBUG_TYPE_ERROR,
    DeprecatedBehavior = Const.DEBUG_TYPE_DEPRECATED_BEHAVIOR,
    UndefinedBehavior = Const.DEBUG_TYPE_UNDEFINED_BEHAVIOR,
    Portability = Const.DEBUG_TYPE_PORTABILITY,
    Performance = Const.DEBUG_TYPE_PERFORMANCE,
    Marker = Const.DEBUG_TYPE_MARKER,
    PushGroup = Const.DEBUG_TYPE_PUSH_GROUP,
    PopGroup = Const.DEBUG_TYPE_POP_GROUP,
    Other = Const.DEBUG_TYPE_OTHER
}
