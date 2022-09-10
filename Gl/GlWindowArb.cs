namespace Gl;

using Win32;
using static Opengl;

public class GlWindowArb:GlWindow {

    public GlWindowArb (ContextConfigurationARB? configuration = null) :
        base(configuration?.BasicConfiguration ?? ContextConfiguration.Default) {
        RenderingContext = CreateContextARB(Dc, RenderingContext, configuration ?? ContextConfigurationARB.Default);
    }
}
