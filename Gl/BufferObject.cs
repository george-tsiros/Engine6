namespace Gl;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static GlContext;

public class BufferObject<T>:OpenglObject where T : unmanaged {

    public static int ElementSize { get; } = Marshal.SizeOf<T>();
    public int Capacity { get; }
    public BufferTarget Target { get; }

    private BufferObject (BufferTarget target) => 
        (Target, Id) = (target, CreateBuffer());

    public BufferObject (int capacityInElements, BufferTarget target = BufferTarget.Array) : this(target) =>
        NamedBufferStorage(this, ElementSize * (Capacity = capacityInElements), 0, Const.DYNAMIC_STORAGE_BIT);

    public BufferObject (in ReadOnlySpan<T> data, BufferTarget target = BufferTarget.Array) : this(data.Length, target) =>
        BufferData(data, data.Length, 0, 0);

    public unsafe void BufferData (in ReadOnlySpan<T> data, int count, int sourceOffset, int targetOffset) {
        Check(sourceOffset, targetOffset, count, data.Length);
        fixed (T* ptr = data) {
            var start = ptr + sourceOffset;
            Debug.Assert((nint)start == (nint)ptr + (nint)(sourceOffset * ElementSize) );
            NamedBufferSubData(this, ElementSize * targetOffset, ElementSize * count, ptr + sourceOffset);
        }
    }

    public void Bind () => BindBuffer(Target, this);

    private void Check (int sourceOffset, int targetOffset, int count, int dataLength) {
        if (Disposed)
            throw new ObjectDisposedException(GetType().Name);
        if (sourceOffset + count > dataLength)
            throw new ArgumentException("overflow", nameof(sourceOffset));
        if (targetOffset + count > Capacity)
            throw new ArgumentException("overflow", nameof(targetOffset));
    }

    protected override Action<int> Delete { get; } = DeleteBuffer;
}
