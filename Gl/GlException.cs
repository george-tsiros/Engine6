namespace Gl;

using System;
using static Opengl;

public class GlException:Exception {
    
    public int GlError { get; }
    public string GlMessage { get; }
    
    public GlException (string message = null) : base(message ?? "") {
        GlError = GetError();
        GlMessage = Glu.ErrorString(GlError);
    }

    public override string ToString () => 
        $"opengl says: '{GlMessage}' ({GlError}), '{Message}'";
}
