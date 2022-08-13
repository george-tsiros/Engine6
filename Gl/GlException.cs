namespace Gl;

using System;
using static Opengl;

public class GlException:Exception {

    public GlErrorCodes GlError { get; }
    public string GlMessage { get; }

    public static void Assert () {
        var e = GetError();
        if (GlErrorCodes.NoError != e)
            throw new GlException(e, Glu.ErrorString((int)e));
    }

    private GlException (GlErrorCodes e, string str) : base(str) {
        GlError = e;
        GlMessage = str;
    }

    public GlException (string message = null) : base(message ?? "") {
        GlError = GetError();
        GlMessage = Glu.ErrorString((int)GlError);
    }

    public override string ToString () =>
        $"opengl says: '{GlMessage}' ({GlError}), '{Message}'";
}
