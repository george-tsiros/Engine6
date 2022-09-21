namespace Common;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

public sealed unsafe class TextEdit { 
    //
}


public sealed unsafe class LineEdit {

    public static LineEdit FromString (string text) {
        const int MaxStackAlloc = 4096;
        var length = Encoding.ASCII.GetByteCount(text);
        if (0 == length)
            return new(Span<byte>.Empty);
        if (length != text.Length)
            throw new ArgumentException("not an ascii string", nameof(text));
        Span<byte> bytes = MaxStackAlloc < length ? new byte[length] : stackalloc byte[length];
        var written = Encoding.ASCII.GetBytes(text, bytes);
        Debug.Assert(written == bytes.Length);
        return new(bytes);
    }

    public LineEdit () : this(Span<byte>.Empty) { }

    public LineEdit (ReadOnlySpan<byte> bytes) {
        Length = bytes.Length;
        data = new byte[Maths.IntMax(128, Length)];
        bytes.CopyTo(data);
        for (var i = 0; i < Length; ++i) {
            var c = data[i];
            if (c < LowerBound || UpperBound < c)
                throw new ArgumentOutOfRangeException(nameof(bytes), $"{nameof(bytes)}[{i}] = 0x{bytes[i]:x2} which is outside the allowed range ['{LowerBound}', '{UpperBound}']");
        }
    }

    /// <summary>DOES NOT INCLUDE TERMINATING NULL CHARACTER/BYTE</summary>
    public void GetCopy (Span<byte> span) =>
        data[0..Length].CopyTo(span);

    /// <summary>RETURNED ARRAY DOES NOT INCLUDE TERMINATING NULL CHARACTER/BYTE</summary>
    public ReadOnlySpan<byte> GetCopy () =>
        new(data, 0, Length);

    public int GetUndoCount () {
        var levels = 0;
        foreach (var eh in undo)
            if (eh.IsMark)
                ++levels;
        return levels;
    }

    public int Length { get; private set; } = 0;

    public int At {
        get => at;
        set {
            if (value < 0 || Length < value)
                throw new ArgumentOutOfRangeException(nameof(value), $"can not move caret to {value}, it is outside the range [0, {Length}]");
            if (at != value) {
                undo.Push(new(OpType.SetCaret, at, true));
                at = value;
            }
        }
    }

    public byte this[int i] =>
        i < Length ? data[i] : throw new ArgumentOutOfRangeException(nameof(i));

    public void Backspace (int count = 1) {
        if (0 == count)
            throw new ArgumentOutOfRangeException(nameof(count), "can not backspace zero positions");
        if (at - count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), $"can not backspace {count} positions starting from {at}");

        for (var i = 0; i < count; ++i) {
            undo.Push(new(OpType.SetCaret, at - i, 0 == i));
            undo.Push(new(OpType.Write, data[at - i - 1], false));
        }

        undo.Push(new(OpType.MoveUp, count, false));

        at -= count;
        MoveDownInternal(count);
    }

    public void Delete (int count = 1) {
        if (0 == count)
            throw new ArgumentOutOfRangeException(nameof(count), "can not backspace zero positions");
        if (Length - count < at)
            throw new ArgumentOutOfRangeException(nameof(count), "can not delete beyond the end of the line (yet)");

        if (1 == count)
            undo.Push(new(OpType.Write, data[at], true));
        else
            for (var i = 0; i < count; ++i) {
                undo.Push(new(OpType.Write, data[at + i], 0 == i));
                undo.Push(new(OpType.SetCaret, at + i, false));
            }

        undo.Push(new(OpType.MoveUp, count, false));

        MoveDownInternal(count);
    }

    public void Insert (ReadOnlySpan<byte> characters) {
        var count = characters.Length;
        if (count == 0)
            throw new ArgumentOutOfRangeException(nameof(characters), "may not be empty");
        for (var i = 0; i < count; ++i)
            if (characters[i] < LowerBound || UpperBound < characters[i])
                throw new ArgumentOutOfRangeException(nameof(characters), $"characters[{i}], is not printable ascii. It is outside the range ['{LowerBound}' .. '{UpperBound}']");

        undo.Push(new(OpType.MoveDown, count, true));
        undo.Push(new(OpType.SetCaret, at, false));

        MoveUpInternal(count);
        characters.CopyTo(data.AsSpan(at, count));
        at += count;
    }

    public void Insert (byte character) {
        if (character < LowerBound || UpperBound < character)
            throw new ArgumentOutOfRangeException(nameof(character), $"that, is not printable ascii. It is outside the range ['{LowerBound}' .. '{UpperBound}']");

        undo.Push(new(OpType.MoveDown, 1, true));
        undo.Push(new(OpType.SetCaret, at, false));

        MoveUpInternal(1);
        data[at] = character;
        ++at;
    }

    public void Overwrite (byte character) {
        if (character < LowerBound || UpperBound < character)
            throw new ArgumentOutOfRangeException(nameof(character), $"that, is not printable ascii. It is outside the range ['{LowerBound}' .. '{UpperBound}']");
        if (at == Length)
            throw new InvalidOperationException("can not overwrite at the end of the line");

        undo.Push(new(OpType.Write, data[at], true));
        undo.Push(new(OpType.SetCaret, at, false));

        data[at] = character;
        ++at;
    }

    public void Undo () {
        if (0 == undo.Count)
            return;
        for (; ; ) {
            var (type, parameter, isMark) = undo.Pop();
            switch (type) {
                case OpType.MoveUp:
                    MoveUpInternal(parameter);
                    break;
                case OpType.MoveDown:
                    MoveDownInternal(parameter);
                    break;
                case OpType.Write:
                    data[at] = (byte)parameter;
                    break;
                case OpType.SetCaret:
                    at = parameter;
                    break;
                default:
                    throw new NotSupportedException();
            }
            if (isMark)
                break;
        }
    }

    enum OpType { MoveUp, MoveDown, Write, SetCaret, }
    record struct Op (OpType Type, int Parameter, bool IsMark);

    private int at = 0;
    private readonly Stack<Op> undo = new();
    private byte[] data;
    private const char LowerBound = ' ', UpperBound = '~';

    private void MoveDownInternal (int value) {
        if (1024 < data.Length && Length < data.Length / 2)
            ShrinkTo(data.Length / 2);

        Array.Copy(data, at + value, data, at, Length - at - value);
        Length -= value;
    }

    private void MoveUpInternal (int value) {
        if (3 * data.Length < 4 * (Length + value))
            ExpandTo(Maths.IntMax(128, 2 * (Length + value)));

        Array.Copy(data, at, data, at + value, Length - at);
        Length += value;
    }

    private void ShrinkTo (int newSize) {
        Debug.Write($"shrinking from {data.Length} to {newSize}\n");
        var a2 = new byte[newSize];
        Array.Copy(data, a2, Length);
        data = a2;
    }

    private void ExpandTo (int newSize) {
        Debug.Write($"expanding from {data.Length} to {newSize}\n");
        var a2 = new byte[newSize];
        data.CopyTo(a2, 0);
        data = a2;
    }
}
