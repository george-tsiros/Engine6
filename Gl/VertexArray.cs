namespace Gl;

using System;
using System.Numerics;
using System.Collections.Generic;
using static Opengl;
using Win32;
using Linear;

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
        var (size, type, isInteger) = SizeAndTypeOf(typeof(T));
        if (size > 4)
            for (var i = 0; i < 4; ++i)
                Attrib(vao, location + i, 4, type, 16 * sizeof(float), 4 * i * sizeof(float), divisor, isInteger);
        else
            Attrib(vao, location, size, type, 0, 0, divisor, isInteger);
    }

    private static void Attrib (int id, int location, int size, AttribType type, int stride, int offset, int divisor, bool isInteger) {
        EnableVertexArrayAttrib(id, location);
        if (isInteger)
            VertexAttribIPointer(location, size, type, stride, offset);
        else
            VertexAttribPointer(location, size, type, false, stride, offset);
        VertexAttribDivisor(location, divisor);
    }

    private static (int size, AttribType type, bool isInteger) SizeAndTypeOf (Type type) => _TYPES.TryGetValue(type, out var i) ? i : throw new ArgumentException($"unsupported type {type.Name}", nameof(type));

    private static readonly Dictionary<Type, (int, AttribType, bool)> _TYPES = new() {
        { typeof(float), (1, AttribType.Float, false) },
        { typeof(double), (1, AttribType.Double, false) },
        { typeof(int), (1, AttribType.Int, true) },
        { typeof(uint), (1, AttribType.UInt, true) },
        { typeof(Vector2), (2, AttribType.Float, false) },
        { typeof(Vector3), (3, AttribType.Float, false) },
        { typeof(Vector4), (4, AttribType.Float, false) },
        { typeof(Vector2i), (2, AttribType.Int, true) },
        { typeof(Vector3i), (3, AttribType.Int, true) },
        { typeof(Matrix4x4), (16, AttribType.Float, false) },
    };

    public static implicit operator int (VertexArray b) => b.Id;
}
