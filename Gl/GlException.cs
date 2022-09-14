namespace Gl;

using System;
using System.Collections.Generic;
using static RenderingContext;

public class GlException:Exception {

    public GlErrorCode GlError { get; }
    public string GlMessage { get; }

    public static void Assert () {
        var errors = new List<GlErrorCode>();
        for (; ; ) { 
        var e = GetError();
            if (GlErrorCode.NoError == e)
                break;
            errors.Add(e);
        }
        if (0 < errors.Count) {
            var strings = errors.ConvertAll(e => Glu.ErrorString((int)e));
            throw new Exception(string.Join("\n", strings));
        }
    }

    private GlException (GlErrorCode e, string str) : base(str) {
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
