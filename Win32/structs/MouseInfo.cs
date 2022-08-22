namespace Win32;

using System;

public struct MouseInfo {
    public uint id, buttonCount, samplerate;
    public IntPtr hasHorWheel;
}
