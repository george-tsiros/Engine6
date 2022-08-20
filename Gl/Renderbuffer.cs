namespace Gl;

using System;
using static Opengl;
using Win32;
using Linear;

public class Renderbuffer:OpenglObject {
    protected override Action<int> Delete { get; } = DeleteRenderbuffer;
    public static implicit operator int (Renderbuffer rb) => rb.Id;
    public Renderbuffer (Vector2i size, RenderbufferFormat format) {
        Id = CreateRenderbuffer();
        NamedRenderbufferStorage(Id, format, size.X, size.Y);
    }
}
