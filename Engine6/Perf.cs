#define __BINARY__

namespace Engine;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

sealed class Perf<T>:IDisposable where T : struct, Enum {

    public Perf (string filepath) {
        if (typeof(T).GetEnumUnderlyingType() != typeof(int))
            throw new ApplicationException($"enum {typeof(T).Name} has underlying type {typeof(T).GetEnumUnderlyingType().Name}, expected {typeof(int).Name} ");
        writer = new(File.Create(filepath));
        var names = Enum.GetNames<T>();
        writer.Write(names.Length);
        foreach (var name in names) {
            var nameLength = name.Length < 256 ? name.Length : throw new Exception("name must have length < 256 characters");
            var value = (int)Enum.Parse(typeof(T), name);
            writer.Write((byte)value);
            writer.Write(name.Length);
            var bytes = Encoding.ASCII.GetBytes(name);
            if (!Array.TrueForAll(bytes, b => 'a' <= b && b <= 'z' || 'A' <= b && b <= 'Z' || '0' <= b && b <= '9' || b == '_'))
                throw new Exception("name may only contain a..z, A..Z, underscores and digits");
            writer.Write(bytes);
        }
    }

    unsafe public void Leave () {
        if (disposed)
            throw new ObjectDisposedException(nameof(Perf<T>));
        Span<byte> bytes = stackalloc byte[sizeof(long) + sizeof(byte)];
        fixed (byte* p = bytes) {
            *(long*)p = Stopwatch.GetTimestamp();
            p[sizeof(long)] = 0;
        }
        writer.Write(bytes);
    }

    unsafe public void Enter (int id) {
        if (disposed)
            throw new ObjectDisposedException(nameof(Perf<T>));
        Span<byte> bytes = stackalloc byte[sizeof(long) + sizeof(byte)];
        fixed (byte* p = bytes) {
            *(long*)p = Stopwatch.GetTimestamp();
            p[sizeof(long)] = (byte)id;
        }
        writer.Write(bytes);
    }

    private bool disposed;
    private readonly BinaryWriter writer;

    private void Dispose (bool disposing) {
        if (disposed)
            return;
        if (disposing)
            writer.Dispose();
        disposed = true;
    }

    public void Dispose () {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
