namespace Shaders;

using System;
using System.Text;
using System.Reflection;
using Gl;
using static Gl.Utilities;

internal static class ParsedShader {
    
    private static string Unpack (string base64) => 
        Encoding.ASCII.GetString(Convert.FromBase64String(base64));

    internal static void Prepare (Type type) {
        var typeName = type.Name;
        var vs = type.GetField("VertexSource", BindingFlags.Public | BindingFlags.Static).GetValue(null) as string;
        var fs = type.GetField("FragmentSource", BindingFlags.Public | BindingFlags.Static).GetValue(null) as string;

        var p = ProgramFromStrings(Unpack(vs), Unpack(fs));

        var nameProperty = type.GetProperty("Id", BindingFlags.Static | BindingFlags.Public) ?? throw new ApplicationException("no property 'Id'");
        var nameBackingField = GetBackingField(type, nameProperty, BindingFlags.Static) ?? throw new ApplicationException("no backing field for Id");
        nameBackingField.SetValue(null, p);
        
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Static))
            if (prop.GetCustomAttribute<GlAttribAttribute>(false) is GlAttribAttribute attr) {
                var fi = GetBackingField(type, prop, BindingFlags.Static) ?? throw new ApplicationException($"no backing field for {prop.Name} of {typeName}");
                var location = Opengl.GetAttribLocation(p, attr.Name);
                if (location < 0)
                    throw new ApplicationException($"could not find attribute '{attr.Name}' in {type.Name}");
                fi.SetValue(null, location);
            }
    
        foreach (var field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Static)) {
            if (field.GetCustomAttribute<GlUniformAttribute>(false) is GlUniformAttribute attr) {
                var location = Opengl.GetUniformLocation(p, attr.Name);
                if (location < 0)
                    throw new ApplicationException($"could not find uniform '{attr.Name}' in {type.Name}");
                field.SetValue(null, location);
            }
        }
    }
}
