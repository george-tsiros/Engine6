namespace Gl;

using System;

[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public sealed class GlAttribAttribute:Attribute {
    public string Name { get; }
    public GlAttribAttribute (string name) => Name = name;
}

[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public sealed class GlUniformAttribute:Attribute {
    public string Name { get; }
    public GlUniformAttribute (string name) => Name = name;
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class GlVersionAttribute:Attribute {
    public readonly Version MinimumVersion;
    public GlVersionAttribute (int major, int minor) {
        MinimumVersion = new(major, minor);
    }
}
