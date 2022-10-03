namespace Gl;

using System;
using System.Text;
using static GlContext;

public abstract class Program:OpenglObject {

    public Program () {
        Id = Utilities.ProgramFromStrings(Unpack(VertexSource), Unpack(FragmentSource));
    }

    protected override Action<int> Delete { get; } = DeleteProgram;
    protected abstract string VertexSource { get; }
    protected abstract string FragmentSource { get; }

    private static string Unpack (string base64) =>
        Encoding.ASCII.GetString(Convert.FromBase64String(base64));
}
