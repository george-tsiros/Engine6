namespace Gl;

using System;
using System.Runtime.InteropServices;
using static GlContext;

public class VertexBuffer<T>:OpenglObject where T : unmanaged {
    public int ElementSize { get; } = Marshal.SizeOf<T>();
    public int Capacity { get; }

    private VertexBuffer () : base() {
        Id = CreateBuffer();
    }

    public VertexBuffer (int capacityInElements) : this() => 
        NamedBufferStorage(Id, ElementSize * (Capacity = capacityInElements), IntPtr.Zero, Const.DYNAMIC_STORAGE_BIT);

    public VertexBuffer (in ReadOnlySpan<T> data) : this(data.Length) => 
        BufferData(data, data.Length, 0, 0);

    private void Check (int sourceOffset, int targetOffset, int count, int dataLength) {
        if (Disposed)
            throw new ObjectDisposedException(GetType().Name);
        if (sourceOffset + count > dataLength)
            throw new ArgumentException("overflow", nameof(sourceOffset));
        if (targetOffset + count > Capacity)
            throw new ArgumentException("overflow", nameof(targetOffset));
    }
 
    unsafe public void BufferData (in ReadOnlySpan<T> data, int count, int sourceOffset, int targetOffset) {
        Check(sourceOffset, targetOffset, count, data.Length);
        fixed (T* ptr = data)
            NamedBufferSubData(Id, ElementSize * targetOffset, ElementSize * count, ptr + sourceOffset);
    }

    unsafe public void BufferData (Span<T> data, int count, int sourceOffset, int targetOffset) {
        Check(sourceOffset, targetOffset, count, data.Length);
        fixed (T* ptr = data)
            NamedBufferSubData(Id, ElementSize * targetOffset, ElementSize * count, ptr + sourceOffset);
    }
    
    protected override Action<int> Delete { get; } = DeleteBuffer;
}
