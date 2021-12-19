#define __BINARY__

namespace Engine;
using System;
using System.Diagnostics;
using System.IO;
#if !DEBUG
using System.Runtime.CompilerServices;
#endif
using System.Text;

sealed class Perf<T>:IDisposable where T : struct, Enum {

    public Perf (string filepath) {
        if (typeof(T).GetEnumUnderlyingType() != typeof(int))
            throw new ApplicationException($"enum {typeof(T).Name} has underlying type {typeof(T).GetEnumUnderlyingType().Name}, expected {typeof(int).Name} ");
        writer = new(File.Create(filepath));
        var names = Enum.GetNames<T>();
        writer.Write(names.Length);
        foreach (var name in names) {
            writer.Write((byte)(int)Enum.Parse(typeof(T), name));
            writer.Write(name.Length);
            writer.Write(Encoding.ASCII.GetBytes(name));
        }
    }

#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#endif
    unsafe public void Leave () {
        Span<byte> bytes = stackalloc byte[sizeof(long) + sizeof(byte)];
        fixed (byte* p = bytes) {
            *(long*)p = Stopwatch.GetTimestamp();
            p[sizeof(long)] = 0;
        }
        writer.Write(bytes);
    }
#if !DEBUG
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
#endif
    unsafe public void Enter (int id) {
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
