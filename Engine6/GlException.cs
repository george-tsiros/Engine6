namespace Engine;

using System;
using System.Diagnostics;

class GlException:Exception {
    public GlException () {
        Debug.WriteLine( Gl.Kernel.GetLastError().ToString("x"));
    }
}
