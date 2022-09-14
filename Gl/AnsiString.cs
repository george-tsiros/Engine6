namespace Gl;

using System;
using System.Runtime.InteropServices;
using System.Text;

public unsafe sealed class AnsiString:IDisposable {
    public static implicit operator nint (AnsiString self) => self.Handle;
    public static implicit operator AnsiString (string str) => new(str);

    public AnsiString (string str) {
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
    private bool disposed;
    
    public nint Handle => !disposed ? handle.AddrOfPinnedObject() : throw new ObjectDisposedException(nameof(AnsiString));
    
    
    public void Dispose () {
        if (!disposed) {
            disposed = true;
            handle.Free();
        }
    }
}
