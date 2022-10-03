namespace Gl;

using System;

[AttributeUsage(AttributeTargets.Field)]
public sealed class GlVersionAttribute:Attribute {
    public readonly Version MinimumVersion;
    public GlVersionAttribute (int major, int minor) {
        MinimumVersion = new(major, minor);
    }
}
