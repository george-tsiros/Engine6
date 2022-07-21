namespace Gl;

using System;
using System.Numerics;
using System.Collections.Generic;
using static Opengl;
using Win32;

public class VertexArray:OpenglObject {
    
    public override int Id { get; } = CreateVertexArray();

    protected override Action<int> Delete { get; } = DeleteVertexArray;

    public void Assign<T> (VertexBuffer<T> buffer, int location, int divisor = 0) where T : unmanaged => Assign(this, buffer, location, divisor);

    private static void Assign<T> (int vao, VertexBuffer<T> buffer, int location, int divisor = 0) where T : unmanaged {
        State.VertexArray = vao;
        BindBuffer(BufferTarget.Array, buffer);
        Attrib<T>(vao, location, divisor);
    }

    private static void Attrib<T> (int vao, int location, int divisor) where T : unmanaged {
        var (size, type) = SizeAndTypeOf(typeof(T));
        if (size > 4)
            for (var i = 0; i < 4; ++i)
                Attrib(vao, location + i, 4, type, 16 * sizeof(float), 4 * i * sizeof(float), divisor);
        else
            Attrib(vao, location, size, type, 0, 0, divisor);
    }

    private static void Attrib (int id, int location, int size, AttribType type, int stride, int offset, int divisor) {
        EnableVertexArrayAttrib(id, location);
        VertexAttribPointer(location, size, type, false, stride, offset);
        VertexAttribDivisor(location, divisor);
    }

    private static (int size, AttribType type) SizeAndTypeOf (Type type) => _TYPES.TryGetValue(type, out var i) ? i : throw new ArgumentException($"unsupported type {type.Name}", nameof(type));

    private static readonly Dictionary<Type, (int, AttribType)> _TYPES = new() {
        { typeof(float), (1, AttribType.Float) },
        { typeof(double), (1, AttribType.Double) },
        { typeof(int), (1, AttribType.Int) },
        { typeof(uint), (1, AttribType.UInt) },
        { typeof(Vector2), (2, AttribType.Float) },
        { typeof(Vector3), (3, AttribType.Float) },
        { typeof(Vector4), (4, AttribType.Float) },
        { typeof(Vector2i), (2, AttribType.Int) },
        { typeof(Vector3i), (3, AttribType.Int) },
        { typeof(Matrix4x4), (16, AttribType.Float) },
    };

    public static implicit operator int (VertexArray b) => b.Id;
}
