namespace Gl;

using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using static Common.Functions;
using static GlContext;
public abstract class Program:OpenglObject {

    public Program () {
        Id = Utilities.ProgramFromStrings(Unpack(VertexSource), Unpack(FragmentSource));
        Debug.Assert(0 < Id);
        var type = GetType();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            if (prop.GetCustomAttribute<GlAttribAttribute>(false) is GlAttribAttribute attr) {
                var fi = Utilities.GetBackingField(type, prop) ?? throw new ApplicationException($"no backing field for {prop.Name} of {type.Name}");
                var location = GetAttribLocation(this, attr.Name ?? LowercaseFirst(prop.Name));
                if (location < 0)
                    throw new ApplicationException($"could not find attribute '{attr.Name}' in {type.Name}");
                fi.SetValue(this, location);
            }

        foreach (var field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)) {
            if (field.GetCustomAttribute<GlUniformAttribute>(false) is GlUniformAttribute attr) {
                var location = GetUniformLocation(this, attr.Name ?? field.Name);
                if (location < 0)
                    throw new ApplicationException($"could not find uniform '{attr.Name ?? field.Name}' in {type.Name}");
                field.SetValue(this, location);
            }
        }
    }

    protected override Action<int> Delete { get; } = DeleteProgram;
    protected abstract string VertexSource { get; }
    protected abstract string FragmentSource { get; }

    private static string Unpack (string base64) =>
        Encoding.ASCII.GetString(Convert.FromBase64String(base64));
}
