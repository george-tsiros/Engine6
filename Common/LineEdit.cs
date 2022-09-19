namespace Common;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

public sealed unsafe class LineEdit {

    public LineEdit (string text) {
        Length = Encoding.ASCII.GetByteCount(text);
        data = new byte[Maths.IntMax(128, Length)];
        var actualByteCount = Encoding.ASCII.GetBytes(text, data);
        Debug.Assert(actualByteCount == Length);
        for (var i = 0; i < Length; ++i) {
            var c = data[i];
            if (c < LowerBound || UpperBound < c)
                throw new ArgumentOutOfRangeException(nameof(text), $"text[{i}] = '\\u{(ushort)c:x4}' which is outside the allowed range ['{LowerBound}', '{UpperBound}']");
        }
    }

    /// <summary>DOES NOT INCLUDE TERMINATING NULL CHARACTER/BYTE</summary>
    public void GetCopy (Span<byte> span) =>
        data[0..Length].CopyTo(span);

    /// <summary>RETURNED ARRAY DOES NOT INCLUDE TERMINATING NULL CHARACTER/BYTE</summary>
    public ReadOnlySpan<byte> GetCopy () =>
        new(data, 0, Length);

    public int Length { get; private set; } = 0;
    
    public int At {
        get => at;
        set {
            if (value < 0 || Length < value)
                throw new ArgumentOutOfRangeException(nameof(value), $"can not move caret outside the range [0, {Length}]");
            undo.Push(new(OpType.SetCaret, at, true));
            at = value;
        }
    }

    public byte this[int i] =>
        i < Length ? data[i] : throw new ArgumentOutOfRangeException(nameof(i));

    public void Backspace () {
        if (0 == at)
            throw new InvalidOperationException($"can not backspace at beginning of line (yet)");
        undo.Push(new(OpType.SetCaret, at, true));
        undo.Push(new(OpType.Write, data[at], false));
        undo.Push(new(OpType.MoveUp, 1, false));
        --at;
        MoveDownInternal(1);
    }

    public void Delete () {
        if (Length == at)
            throw new InvalidOperationException($"can not delete at end of line (yet)");
        undo.Push(new(OpType.Write, data[at], true));
        undo.Push(new(OpType.MoveUp, 1, false));
        MoveDownInternal(1);
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
        if (1024 < data.Length && Length < data.Length / 2) {
            var newSize = data.Length / 2;
            Debug.Write($"shrinking from {data.Length} to {newSize}\n");
            var a2 = new byte[newSize];
            Array.Copy(data, a2, Length);
            data = a2;
        }
        Array.Copy(data, at + value, data, at, Length - at);
        Length -= value;
    }

    private void MoveUpInternal (int value) {
        if (3 * data.Length < 4 * (Length + value)) {
            var newSize = Maths.IntMax(128, 2 * (Length + value));
            Debug.Write($"expanding from {data.Length} to {newSize}\n");
            var a2 = new byte[newSize];
            data.CopyTo(a2, 0);
            data = a2;
        }
        Array.Copy(data, at, data, at + value, Length - at);
        Length += value;
    }
}
