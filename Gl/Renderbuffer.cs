namespace Gl;

using System;
using static Opengl;

public class Renderbuffer:OpenglObject {
    protected override Action<int> Delete { get; } = DeleteRenderbuffer;
    public override int Id { get; } = CreateRenderbuffer();
    public static implicit operator int (Renderbuffer rb) => rb.Id;
    public Renderbuffer (Vector2i size, RenderbufferFormat format) {
        NamedRenderbufferStorage(Id, format, size.X, size.Y);
    }
}
