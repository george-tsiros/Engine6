namespace Gl;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Opengl;

public class VertexBuffer<T>:IDisposable where T : unmanaged {
    public static implicit operator int (VertexBuffer<T> b) => b.Id;
    public int Id { get; } = CreateBuffer();
    public int ElementSize { get; } = Marshal.SizeOf<T>();
    public int Capacity { get; }
    public VertexBuffer (int capacityInElements) => NamedBufferStorage(Id, ElementSize * (Capacity = capacityInElements), IntPtr.Zero, Const.DYNAMIC_STORAGE_BIT);
    public VertexBuffer (T[] data) : this(data.Length) => BufferData(data, data.Length, 0, 0);
    private void Check (int sourceOffset, int targetOffset, int count, int dataLength) {
        if (disposed)
            throw new Exception();
        if (sourceOffset + count > dataLength)
            throw new Exception();
        if(targetOffset + count > Capacity)
            throw new Exception();
    }
    unsafe public void BufferData (T[] data, int count, int sourceOffset, int targetOffset) {
        Check(sourceOffset, targetOffset, count, data.Length);
        fixed (T* ptr = data)
            NamedBufferSubData(Id, ElementSize * targetOffset, ElementSize * count, ptr + sourceOffset);
    }
    unsafe public void BufferData (Span<T> data, int count, int sourceOffset, int targetOffset) {
        Check(sourceOffset, targetOffset, count, data.Length);
        fixed (T* ptr = data)
            NamedBufferSubData(Id, ElementSize * targetOffset, ElementSize * count, ptr + sourceOffset);
    }

    private bool disposed;
    private void Dispose (bool _) {
        if (!disposed) {
            DeleteBuffer(Id);
            disposed = true;
        }
    }

    public void Dispose () {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
