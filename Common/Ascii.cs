namespace Common;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

public sealed unsafe class Ascii:IDisposable {
    private static nint Null => _null != 0 ? _null : _null = CreateNull();
    private static nint _null;
    private static nint CreateNull () {
        // ...
        var p = Marshal.AllocHGlobal(1);
        *(byte*)p = 0;
        return p;
    }

    public Ascii (string s) {
        if (string.IsNullOrEmpty(s)) {
            Length = 0;
            bytes = (byte*)Null;
        } else {
            Length = Encoding.ASCII.GetByteCount(s);
            if (s.Length != Length)
                throw new ArgumentException("not an ascii string", nameof(s));
            bytes = (byte*)Marshal.AllocHGlobal(Length + 1);
            var bytesWritten = Encoding.ASCII.GetBytes(s, new Span<byte>(bytes, Length + 1));
            Debug.Assert(bytesWritten == Length);
            bytes[Length] = 0;
        }
    }

    //internal Ascii (ReadOnlySpan<byte> span) {

    //}

    public byte this[int i] {
        get => Get(i);
        set => Set(i, value);
    }

    public nint Handle => 0 != (nint)bytes ? (nint)bytes : throw new ObjectDisposedException(nameof(Ascii));
    public static implicit operator nint (Ascii self) => self.Handle;
    public static implicit operator Ascii (string str) => new(str);
    /// <summary>EXCLUDES TERMINATING NULL BYTE</summary>
    public readonly int Length;

    private byte* bytes;

    public void Dispose () {
        if (0 != (nint)bytes && Null != (nint)bytes) {
            Marshal.FreeHGlobal((nint)bytes);
            bytes = (byte*)0;
        }
    }

    private byte Get (int i) {
        if (Length <= i)
            throw new ArgumentOutOfRangeException(nameof(i));
        if (0 == (nint)bytes)
            throw new ObjectDisposedException(nameof(Ascii));
        return bytes[i];
    }

    private byte Set (int i, byte value) {
        if (Length <= i)
            throw new ArgumentOutOfRangeException(nameof(i));
        if (0 == (nint)bytes)
            throw new ObjectDisposedException(nameof(Ascii));
        if (0 == value)
            throw new ArgumentOutOfRangeException("no.", nameof(value));
        return bytes[i] = value;
    }
}
