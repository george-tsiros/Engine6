namespace Gl;

using System;
using Common;
using static GlContext;

public sealed class Renderbuffer:OpenglObject {
    protected override Action<int> Delete { get; } = DeleteRenderbuffer;
    public Renderbuffer (Vector2i size, InternalFormat format) {
        Id = CreateRenderbuffer();
        NamedRenderbufferStorage(this, format, size.X, size.Y);
    }
}
