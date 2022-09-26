namespace Common;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;


// this looks absolutely insane at first glance
// i intend to pretty much replace all of System.String
// and avoid using heap-allocated, unicode strings
// completely, if possible. 

public sealed unsafe class Ascii:IDisposable {

    public Ascii (nint ptr) {
        Length = 0;
        var p = (byte*)ptr;
        while (0 != p[Length])
            ++Length;
        if (0 < Length) {
            bytes = Marshal.AllocHGlobal(Length + 1);
            for (var i = 0; i <= Length; ++i)
                ((byte*)bytes)[i] = p[i];
            ((byte*)bytes)[Length] = 0;
        } else
            bytes = NullTextPointer;
    }

    public Ascii (Span<byte> span) {
        Length = span.Length;
        if (0 < Length) {
            bytes = Marshal.AllocHGlobal(Length + 1);
            span.CopyTo(new Span<byte>((byte*)bytes, Length));
            ((byte*)bytes)[Length] = 0;
        } else
            bytes = NullTextPointer;
    }

    public Ascii (string text) {
        if (text is null)
            throw new ArgumentNullException(nameof(text));
        Length = 0 != text.Length ? Encoding.ASCII.GetByteCount(text) : 0;
        if (text.Length != Length)
            throw new ArgumentException("not an ascii string", nameof(text));
        if (0 == Length) {
            bytes = NullTextPointer;
        } else {
            bytes = Marshal.AllocHGlobal(Length + 1);
            var bytesWritten = Encoding.ASCII.GetBytes(text, new Span<byte>((byte*)bytes, Length + 1));
            Debug.Assert(bytesWritten == Length);
            ((byte*)bytes)[Length] = 0;
        }
    }

    public byte this[int i] {
        get => Get(i);
        set => Set(i, value);
    }

    public nint Handle => 0 != bytes ? bytes : throw new ObjectDisposedException(nameof(Ascii));
    //public static implicit operator nint (Ascii self) => self.Handle;
    //public static implicit operator Ascii (string str) => new(str);
    public static implicit operator string (Ascii self) =>
        0 != self.bytes ? Encoding.ASCII.GetString((byte*)self.bytes, self.Length) : throw new ObjectDisposedException(nameof(self));

    /// <summary>EXCLUDES TERMINATING NULL BYTE</summary>
    public readonly int Length;

    private nint bytes;

    public void Dispose () {
        if (0 != bytes && NullTextPointer != bytes) {
            Marshal.FreeHGlobal(bytes);
            bytes = 0;
        }
    }

    /// <summary>INCOMING SPAN EXCLUDES TERMINATING NULL BYTE</summary>
    internal Ascii (in ReadOnlySpan<byte> span) {
        Debug.Assert(0 != span.Length);
        Length = span.Length;
        bytes = Marshal.AllocHGlobal(Length + 1);
        ((byte*)bytes)[Length] = 0;
        span.CopyTo(new Span<byte>((byte*)bytes, Length));
    }

    private static nint NullTextPointer => 0 != _null ? _null : _null = CreateNull();
    private static nint _null;
    private static nint CreateNull () {
        // ...
        var p = Marshal.AllocHGlobal(1);
        *(byte*)p = 0;
        return p;
    }

    private byte Get (int i) {
        if (0 == bytes)
            throw new ObjectDisposedException(nameof(Ascii));
        if (i < 0)
            throw new ArgumentOutOfRangeException(nameof(i));
        if (Length <= i)
            throw new ArgumentOutOfRangeException(nameof(i));
        return ((byte*)bytes)[i];
    }

    private byte Set (int i, byte value) {
        if (0 == bytes)
            throw new ObjectDisposedException(nameof(Ascii));
        if (i < 0)
            throw new ArgumentOutOfRangeException(nameof(i));
        if (Length <= i)
            throw new ArgumentOutOfRangeException(nameof(i));
        if (0 == value)
            throw new ArgumentOutOfRangeException("no.", nameof(value));
        return ((byte*)bytes)[i] = value;
    }
}
