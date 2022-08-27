namespace Gl;

using System;

public readonly struct ContextConfigurationARB {
    public ContextConfiguration BasicConfiguration { get; init; }
    public SwapMethod? SwapMethod { get; init; }
    public Version Version { get; init; }
    public ProfileMask? Profile { get; init; }
    public ContextFlag? Flags { get; init; }
}
