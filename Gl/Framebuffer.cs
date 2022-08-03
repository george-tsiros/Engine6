namespace Gl;

using System;
using static Opengl;

public class Framebuffer:OpenglObject {

    public override int Id { get; } = CreateFramebuffer();
    
    protected override Action<int> Delete { get; } = DeleteFramebuffer;
    
    public FramebufferStatus CheckStatus (FramebufferTarget target = FramebufferTarget.Framebuffer) => CheckNamedFramebufferStatus(Id, target);

    public void Attach (Sampler2D texture, FramebufferAttachment attachment) => NamedFramebufferTexture(Id, attachment, texture);

    public void Attach (Renderbuffer renderbuffer, FramebufferAttachment attachment) => NamedFramebufferRenderbuffer(Id, attachment, renderbuffer.Id);

    public static implicit operator int (Framebuffer fb) => fb.Id;
}