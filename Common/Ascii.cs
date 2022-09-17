namespace Common;

using System;
using System.Runtime.InteropServices;
using System.Text;

public readonly struct Ascii:IDisposable {
    public static implicit operator nint (Ascii self) => self.Handle;
    public static implicit operator Ascii (string str) => new(str);

    public Ascii (string str) {
        var byteCount = Encoding.ASCII.GetByteCount(str);
        if (str.Length != byteCount)
            throw new ArgumentException("not an ascii string", nameof(str));
        bytes = new byte[byteCount + 1];
        handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        _ = Encoding.ASCII.GetBytes(str, bytes);
        bytes[str.Length] = 0;
    }

    private readonly byte[] bytes;
    private readonly GCHandle handle;

    public nint Handle => handle.IsAllocated ? handle.AddrOfPinnedObject() : throw new ObjectDisposedException(nameof(Ascii));

    public void Dispose () {
        if (handle.IsAllocated) {
            handle.Free();
            GC.SuppressFinalize(this);
        }
    }
}
