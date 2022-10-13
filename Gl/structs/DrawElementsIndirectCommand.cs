namespace Gl;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct DrawElementsIndirectCommand {
    public int Count; // was uint
    public int InstanceCount; // was uint
    public int FirstIndex; // was uint
    public int BaseVertex;
    public int BaseInstance; // was uint
}