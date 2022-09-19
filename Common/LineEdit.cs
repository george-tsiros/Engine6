namespace Common;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

//public enum EditCode {
//    Ok,
//    CaretIndexNegative,
//    CaretIndexOutOfRange,
//    ValueNotAscii,
//}
public sealed unsafe class LineEdit {

    //public readonly struct Op {
    //    public readonly EditOp EditOp;
    //    public readonly ref int At;
    //    public readonly ref int Length;
    //    public readonly ref int Value;
    //}


    //public static EditCode EdOp (ref byte[] array, Op op) {
    //    return op.EditOp switch {
    //        EditOp.Delete => EdDelete(ref array, ref op.At, ref op.Length, ref op.Value),
    //        EditOp.Insert => EdInsert(ref array, ref op.At, ref op.Length, ref op.Value),
    //        EditOp.Overwrite => EdOverwrite(ref array, ref op.At, ref op.Length, ref op.Value),
    //        EditOp.SetCaretIndex => EdSetCaretIndex(ref array, ref op.At, ref op.Length, ref op.Value),
    //        _ => throw new InvalidOperationException(),
    //    };
    //}

    //public static EditCode EdSetCaretIndex (ref byte[] array, ref int at, ref int length, ref int value) {
    //    if (value < 0)
    //        return EditCode.CaretIndexNegative;
    //    if (length < value)
    //        return EditCode.CaretIndexOutOfRange;

    //    at = value;
    //    return EditCode.Ok;
    //}

    //public static EditCode EdOverwrite (ref byte[] array, ref int at, ref int length, ref int value) {
    //    if (at < 0)
    //        return EditCode.CaretIndexNegative;
    //    if (length <= at)
    //        return EditCode.CaretIndexOutOfRange;
    //    if (value < LowerBound || UpperBound < value)
    //        return EditCode.ValueNotAscii;

    //    array[at] = (byte)value;
    //    return EditCode.Ok;
    //}

    //public static EditCode EdDelete (ref byte[] array, ref int at, ref int length, ref int value) {
    //    /*
    //a = { 'a', 'b', 'c', ?, ... }
    //i = 0
    //l = 3

    //a[0] = a[1]
    //a[1] = a[2]
    //--l
    //*/
    //    if (at < 0)
    //        return EditCode.CaretIndexNegative;
    //    if (length <= at)
    //        return EditCode.CaretIndexOutOfRange;

    //    --length;
    //    for (var x = at; x < length; ++x)
    //        array[x] = array[x + 1];
    //    return EditCode.Ok;
    //}

    //public static EditCode EdInsert (ref byte[] array, ref int at, ref int length, ref int value) {
    //    /*
    //a = { 'a', 'b', 'c', ?, ... }
    //i = 0
    //l = 3
    //b = 'p'

    //a[3] = a[2]
    //a[2] = a[1]
    //a[1] = a[0]
    //a[0] = 'p'

    //++l
    //++i
    //*/
    //    if (at < 0)
    //        return EditCode.CaretIndexNegative;
    //    if (length < at)
    //        return EditCode.CaretIndexOutOfRange;
    //    if (value < LowerBound || UpperBound < value)
    //        return EditCode.ValueNotAscii;
    //    array[at] = (byte)value;
    //    ++length;
    //    ++at;
    //    return EditCode.Ok;
    //}


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
        Op undo = new(OpType.Delete, data[At]);

    }

    public void Undo (int count = 1) {
        if (count < 1)
            throw new ArgumentOutOfRangeException(nameof(count));
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

    private void WriteInternal (int value) {
        data[At] = (byte)value;
    }

    private void SetCaretIndexInternal (int value) {
        At = value;
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
