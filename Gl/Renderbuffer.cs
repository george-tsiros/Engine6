namespace Gl;

using System;
using static Opengl;
using Linear;

public class Renderbuffer:OpenglObject {
    protected override Action<int> Delete { get; } = DeleteRenderbuffer;
    public Renderbuffer (Vector2i size, RenderbufferFormat format) {
        Id = CreateRenderbuffer();
        NamedRenderbufferStorage(Id, format, size.X, size.Y);
    }
}
