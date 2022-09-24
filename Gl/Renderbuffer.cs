namespace Gl;
using System;
using static GlContext;
using Common;
public sealed class Renderbuffer:OpenglObject {
    protected override Action<int> Delete { get; } = DeleteRenderbuffer;
    public Renderbuffer (Vector2i size, RenderbufferFormat format) {
        Id = CreateRenderbuffer();
        NamedRenderbufferStorage(this, format, size.X, size.Y);
    }
}
