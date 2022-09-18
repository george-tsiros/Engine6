namespace Gl;

using System;
using static GlContext;

public class Framebuffer:OpenglObject {

    public Framebuffer () =>
        Id = CreateFramebuffer();

    protected override Action<int> Delete =>
        DeleteFramebuffer;

    public FramebufferStatus CheckStatus (FramebufferTarget target = FramebufferTarget.Framebuffer) =>
        CheckNamedFramebufferStatus(this, target);

    public void Attach (Sampler2D texture, FramebufferAttachment attachment) =>
        NamedFramebufferTexture(this, attachment, texture);

    public void Attach (Renderbuffer renderbuffer, FramebufferAttachment attachment) =>
        NamedFramebufferRenderbuffer(this, attachment, renderbuffer.Id);

}
