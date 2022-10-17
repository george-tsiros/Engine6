namespace Common;

using System;
using System.Runtime.InteropServices;
using System.Text;

public sealed class Ascii:IDisposable {
    private readonly GCHandle handle;
    private readonly byte[] bytes;
    public static implicit operator nint (Ascii self) => self.handle.IsAllocated ? self.handle.AddrOfPinnedObject() : throw new ObjectDisposedException(nameof(Ascii));

    public Ascii (string str) {
        var byteCount = Encoding.ASCII.GetByteCount(str);
        bytes = new byte[byteCount + 1];
        _ = Encoding.ASCII.GetBytes(str, 0, str.Length, bytes, 0);
        bytes[byteCount] = 0;
        handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
    }
    public void Dispose () {
        if (handle.IsAllocated)
            handle.Free();
    }
}
