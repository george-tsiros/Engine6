namespace Common;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public unsafe class Editor:IDisposable {

    public Editor () {
        ptr = (byte*)Marshal.AllocHGlobal(capacity = StartingSize);
    }

    public Ascii AsAscii () {
        NotDisposed();
        return new(new ReadOnlySpan<byte>(ptr, length));
    }

    public int Position { get; private set; }

    public byte this[int i] {
        get => SafeRead(i);
        set => SafeWrite(i, value);
    }

    public bool Insert { get; set; }

    private byte SafeWrite (int i, byte value) {
        NotDisposed();
        return i < length ? ptr[i] = value : throw new ArgumentOutOfRangeException(nameof(i));
    }

    private byte SafeRead (int i) {
        NotDisposed();
        return i < length ? ptr[i] : throw new ArgumentOutOfRangeException(nameof(i));
    }

    private const int StartingSize = 256;

    private int capacity = 0;
    
    /// <summary>EXCLUDING NULL TERMINATING BYTE</summary>
    private int length = 0;
    
    private byte* ptr;
    
    private bool disposed = false;

    private void NotDisposed () {
        if (disposed)
            throw new ObjectDisposedException(nameof(Editor));
    }

    public void Dispose () {
        if (!disposed) {
            disposed = true;
            Marshal.FreeHGlobal((nint)ptr);
        }
    }
}
