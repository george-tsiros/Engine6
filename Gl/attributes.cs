namespace Gl;

using System;

[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public sealed class GlAttribAttribute:Attribute {
    public string Name { get; } = null;
    public GlAttribAttribute () { }
    public GlAttribAttribute (string name) => Name = name;
}

[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public sealed class GlUniformAttribute:Attribute {
    public string Name { get; } = null;
    public GlUniformAttribute () { }
    public GlUniformAttribute (string name) => Name = name;
}

[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public sealed class GlFragOutAttribute:Attribute {
    public string Name { get; } = null;
    public GlFragOutAttribute () { }
    public GlFragOutAttribute (string name) => Name = name;
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class GlVersionAttribute:Attribute {
    public readonly Version MinimumVersion;
    public GlVersionAttribute (int major, int minor) {
        MinimumVersion = new(major, minor);
    }
}
