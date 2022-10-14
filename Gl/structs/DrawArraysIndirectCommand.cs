namespace Gl;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct DrawArraysIndirectCommand {
    public int VertexCount; // was uint
    public int InstanceCount; // was uint
    public int First; // was uint
    public int BaseInstance; // was uint
}
