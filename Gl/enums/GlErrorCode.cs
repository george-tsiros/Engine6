namespace Gl;

public enum GlErrorCode:uint {
    NoError = Const.NO_ERROR,
    InvalidEnum = Const.INVALID_ENUM,
    InvalidValue = Const.INVALID_VALUE,
    InvalidOperation = Const.INVALID_OPERATION,
    StackOverflow = Const.STACK_OVERFLOW,
    StackUnderflow = Const.STACK_UNDERFLOW,
    OutOfMemory = Const.OUT_OF_MEMORY,
}
