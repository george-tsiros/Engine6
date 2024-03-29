namespace Gl;

using System;

public readonly struct ContextConfiguration {
    public int? ColorBits { get; init; }
    public int? DepthBits { get; init; }
    public bool? DoubleBuffer { get; init; }
    public bool? Composited { get; init; }
    public SwapMethod? SwapMethod { get; init; }
    public Version Version { get; init; }
    public ProfileMask? Profile { get; init; }
    public ContextFlag? Flags { get; init; }
    public static readonly ContextConfiguration Default = new() {
        ColorBits = 32,
        DepthBits = 24,
        DoubleBuffer = true,
        SwapMethod= Gl.SwapMethod.Undefined,
        Profile = ProfileMask.Core,
        Flags = ContextFlag.Debug
    };
}
