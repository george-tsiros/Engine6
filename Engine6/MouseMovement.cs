using Gl;
using System.Diagnostics;
using System.IO;
using Win32;

namespace Engine6;

public class MouseMovement:GlWindow {
    public MouseMovement () : base() {
        writer = new(File.Create("data.bin"));
    }

    protected override void OnClosed () {
        writer.Close();
        writer.Dispose();
    }

    BinaryWriter writer;
    protected override void OnInput (int dx, int dy) {
        if (Fullscreen) {
            writer.Write(Stopwatch.GetTimestamp());
            writer.Write(dx);
            writer.Write(dy);
        }
    }
}
