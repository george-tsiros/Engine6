namespace Gl;

using System;
using static RenderingContext;

public class Framebuffer:OpenglObject {
    
    public Framebuffer () =>
        Id = CreateFramebuffer();

    protected override Action<int> Delete => 
        DeleteFramebuffer;

    public FramebufferStatus CheckStatus (FramebufferTarget target = FramebufferTarget.Framebuffer) => 
        CheckNamedFramebufferStatus(Id, target);

    public void Attach (Sampler2D texture, FramebufferAttachment attachment) => 
        NamedFramebufferTexture(Id, attachment, texture);

    public void Attach (Renderbuffer renderbuffer, FramebufferAttachment attachment) => 
        NamedFramebufferRenderbuffer(Id, attachment, renderbuffer.Id);

}
