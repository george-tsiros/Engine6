namespace Gl;

using System;
using System.Reflection;
using System.Text;

public abstract class OpenglObject:IDisposable {

    public int Id { get; protected init; }

    protected abstract Action<int> Delete { get; }

    public static implicit operator int (OpenglObject ob) => ob.Id;

    protected bool Disposed { get; private set; }

    public void Dispose (bool dispose) {
        if (dispose) {
            if (!Disposed) {
                Delete(Id);
                Disposed = true;
            }
        }
    }

    public void Dispose () {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

public abstract class Program:OpenglObject {

    private static string Unpack (string base64) =>
        Encoding.ASCII.GetString(Convert.FromBase64String(base64));

    protected override Action<int> Delete { get; } = Opengl.DeleteProgram;
    protected abstract string VertexSource { get; }
    protected abstract string FragmentSource { get; }
    public Program () {
        if (IntPtr.Zero == Opengl.GetCurrentContext())
            throw new InvalidOperationException("no current context");
        Id = Utilities.ProgramFromStrings(Unpack(VertexSource), Unpack(FragmentSource));
        var type = GetType();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Static))
            if (prop.GetCustomAttribute<GlAttribAttribute>(false) is GlAttribAttribute attr) {
                var fi = Utilities.GetBackingField(type, prop) ?? throw new ApplicationException($"no backing field for {prop.Name} of {type.Name}");
                var location = Opengl.GetAttribLocation(Id, attr.Name);
                if (location < 0)
                    throw new ApplicationException($"could not find attribute '{attr.Name}' in {type.Name}");
                fi.SetValue(this, location);
            }

        foreach (var field in type.GetFields(BindingFlags.NonPublic)) {
            if (field.GetCustomAttribute<GlUniformAttribute>(false) is GlUniformAttribute attr) {
                var location = Opengl.GetUniformLocation(Id, attr.Name);
                if (location < 0)
                    throw new ApplicationException($"could not find uniform '{attr.Name}' in {type.Name}");
                field.SetValue(this, location);
            }
        }
    }
}
