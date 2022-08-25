namespace Gl;

using System;

[Flags]
public enum BufferBit {
    Color = Const.COLOR_BUFFER_BIT,
    Depth = Const.DEPTH_BUFFER_BIT,
    Stencil = Const.STENCIL_BUFFER_BIT,
    ColorDepth = Color | Depth,
}
