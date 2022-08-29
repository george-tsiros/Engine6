namespace Gl;

using System;

public readonly struct ContextConfigurationARB {
    public ContextConfiguration BasicConfiguration { get; init; }
    public Version Version { get; init; }
    public ProfileMask? Profile { get; init; }
    public ContextFlag? Flags { get; init; }
    public static readonly ContextConfigurationARB Default = new() { 
        BasicConfiguration = ContextConfiguration.Default, 
        Flags = ContextFlag.Debug | ContextFlag.ForwardCompatible, 
        Profile = ProfileMask.Core 
    };
}
