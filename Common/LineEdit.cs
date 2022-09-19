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
    public int At { get; private set; } = 0;
    public byte this[int i] =>
        i < Length ? data[i] : throw new ArgumentOutOfRangeException(nameof(i));

    public void Delete () {
        if (Length == At)
            throw new InvalidOperationException($"can not delete at end of line (yet)");
        recent.Push(new(OpType.Delete, data[At]));
        MoveDownInternal(1);
    }

    public void Insert (byte character) {
        if (character < LowerBound || UpperBound < character)
            throw new ArgumentOutOfRangeException(nameof(character), $"that, is not printable ascii. It is outside the range ['{LowerBound}' .. '{UpperBound}']");
        recent.Push(new(OpType.Insert, 0));
        MoveUpInternal(1);
        data[At++] = character;
    }

    public void SetCaret (int position) {
        if (position < 0 || Length < position)
            throw new ArgumentOutOfRangeException(nameof(position), $"can not move caret outside the range [0, {Length}]");
        recent.Push(new(OpType.SetCaretIndex, At));
        At = position;
    }

    public void Overwrite (byte character) {
        if (character < LowerBound || UpperBound < character)
            throw new ArgumentOutOfRangeException(nameof(character), $"that, is not printable ascii. It is outside the range ['{LowerBound}' .. '{UpperBound}']");
        if (At == Length)
            throw new InvalidOperationException("can not overwrite at the end of the line");
        recent.Push(new(OpType.Overwrite, data[At]));
        data[At] = character;
        ++At;
    }

    public void Undo (int count = 1) {
        if (count < 1)
            throw new ArgumentOutOfRangeException(nameof(count));
        var (type, parameter) = recent.Pop();
        switch (type) {
            case OpType.Insert:
                --At;
                MoveDownInternal(1);
                break;
            case OpType.Delete:
                MoveUpInternal(1);
                data[At] = (byte)parameter;
                break;
            case OpType.Overwrite:
                data[--At] = (byte)parameter;
                break;
            case OpType.SetCaretIndex:
                At = parameter;
                break;
            default:
                throw new NotSupportedException();
        }
    }

    enum OpType { Insert, Delete, Overwrite, SetCaretIndex, }
    record struct Op (OpType Type, int Parameter);

    private readonly Stack<Op> recent = new();
    private byte[] data;
    private const char LowerBound = ' ', UpperBound = '~';

    private void MoveDownInternal (int value) {
        if (1024 < data.Length && Length < data.Length / 2) {
            var a2 = new byte[data.Length / 2];
            Array.Copy(data, a2, Length);
            data = a2;
        }
        Array.Copy(data, At + value, data, At, Length - At);
        Length -= value;
    }

    private void MoveUpInternal (int value) {
        if (3 * data.Length < 4 * (Length + value)) {
            var a2 = new byte[Maths.IntMax(128, 2 * (Length + value))];
            data.CopyTo(a2, 0);
            data = a2;
        }
        Array.Copy(data, At, data, At + value, Length - At);
        Length += value;
    }
}
