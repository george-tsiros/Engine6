namespace Gl;

using System;
using System.Numerics;
using System.Collections.Generic;
using static GlContext;
using Common;
using System.Diagnostics;

public class VertexArray:OpenglObject {

    public VertexArray () {
        Id = CreateVertexArray();
    }

    public void Assign<T> (BufferObject<T> buffer, Attrib<T> location, int divisor = 0) where T : unmanaged {
        Debug.Assert(BufferTarget.ARRAY_BUFFER == buffer.Target);
        BindVertexArray(this);
        buffer.Bind();
        Attrib<T>(location, divisor);
    }

    protected override Action<int> Delete { get; } = 
        DeleteVertexArray;

    private void Attrib<T> (int location, int divisor) where T : unmanaged {
        var (size, type) = SizeAndTypeOf(typeof(T));
        if (size > 4)
            for (var i = 0; i < 4; ++i)
                Attrib(location + i, 4, type, 16 * sizeof(float), 4 * i * sizeof(float), divisor);
        else
            Attrib(location, size, type, 0, 0, divisor);
    }

    private void Attrib (int location, int size, VertexAttribPointerType type, int stride, int offset, int divisor) {
        EnableVertexArrayAttrib(this, location);
            VertexAttribPointer(location, size, type, false, stride, offset);
        VertexAttribDivisor(location, divisor);
    }

    private static (int size, VertexAttribPointerType type) SizeAndTypeOf (Type type) =>
        _TYPES.TryGetValue(type, out var i) ? i : throw new ArgumentException($"unsupported type {type.Name}", nameof(type));

    private static readonly Dictionary<Type, (int, VertexAttribPointerType)> _TYPES = new() {
        { typeof(float), (1, VertexAttribPointerType.FLOAT) },
        { typeof(double), (1, VertexAttribPointerType.DOUBLE) },
        { typeof(int), (1, VertexAttribPointerType.INT) },
        { typeof(uint), (1, VertexAttribPointerType.UNSIGNED_INT) },
        { typeof(Vector2), (2, VertexAttribPointerType.FLOAT) },
        { typeof(Vector3), (3, VertexAttribPointerType.FLOAT) },
        { typeof(Vector4), (4, VertexAttribPointerType.FLOAT) },
        { typeof(Vector2i), (2, VertexAttribPointerType.INT) },
        { typeof(Vector3i), (3, VertexAttribPointerType.INT) },
        { typeof(Matrix4x4), (16, VertexAttribPointerType.FLOAT) },
    };

}
