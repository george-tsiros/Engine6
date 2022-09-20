namespace Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using Common;
using System;
using System.Text;
using System.Collections.Generic;

[TestClass]
public class LineEdit_basic {

    [TestMethod]
    [DynamicData(nameof(HundredRandomStrings))]
    public void At_works (string text) {
        var ed = LineEdit.FromString(text);
        var l = ed.Length;
        Random r = new(0);
        for (var i = 0; i < 100; ++i) {
            var at = (ed.At + r.Next(l)) % l;
            ed.At = at;
            AreEqual(at, ed.At);
        }
    }

    [TestMethod]
    [DynamicData(nameof(HundredRandomStrings))]
    public void At_Undo_works (string text) {
        Stack<int> positions = new();
        var ed = LineEdit.FromString(text);
        var l = ed.Length;
        Random r = new(0);
        for (var i = 0; i < 100; ++i) {
            positions.Push(ed.At);
            ed.At = (ed.At + r.Next(l)) % l;
        }

        while (positions.TryPop(out var at)) {
            ed.Undo();
            AreEqual(at, ed.At);
        }
    }

    [TestMethod]
    [DynamicData(nameof(HundredRandomStrings))]
    public void Length_works (string text) {
        var ed = LineEdit.FromString(text);
        AreEqual(text.Length, ed.Length);
    }

    [TestMethod]
    [DynamicData(nameof(HundredRandomStrings))]
    public void Indexing_works (string text) {
        var length = text.Length;
        var ed = LineEdit.FromString(text);
        for (var i = 0; i < length; ++i)
            AreEqual((byte)text[i], ed[i]);
    }

    [TestMethod]
    [DynamicData(nameof(HundredRandomStrings))]
    public void Overwrite_works (string text) {
        const byte value = (byte)' ';
        var length = text.Length;
        for (var i = 0; i < length; ++i) {
            var ed = LineEdit.FromString(text);
            ed.At = i;
            ed.Overwrite(value);
            AreEqual(i + 1, ed.At);
            for (var p = 0; p < length; ++p)
                AreEqual(i == p ? value : (byte)text[p], ed[p]);
        }
    }

    [TestMethod]
    [DynamicData(nameof(HundredRandomStrings))]
    public void Overwrite_Undo_works (string text) {
        const byte value = (byte)' ';
        var length = text.Length;
        for (var i = 0; i < length; ++i) {
            var ed = LineEdit.FromString(text);
            ed.At = i;
            ed.Overwrite(value);
            ed.Undo();
            AreEqual(i, ed.At);
            IsEq(text, ed);
        }
    }

    [TestMethod]
    public void Overwrite_throws_with_invalid_value () {
        var ed = LineEdit.FromString("a");
        _ = ThrowsException<ArgumentOutOfRangeException>(() => ed.Overwrite((byte)'\n'));
    }

    [TestMethod]
    public void Overwrite_throws_at_end () {
        LineEdit ed = new();
        _ = ThrowsException<InvalidOperationException>(() => ed.Overwrite((byte)'a'));
    }

    [TestMethod]
    [DynamicData(nameof(HundredRandomStrings))]
    public void Delete_works (string text) {
        var ed = LineEdit.FromString(text);
        Random r = new(0);
        var count = r.Next(ed.Length);
        var at = r.Next(ed.Length + 1 - count);
        ed.At = at;
        ed.Delete(count);
        IsEq(text.Remove(at, count), ed);
        AreEqual(at, ed.At);
    }

    [TestMethod]
    [DynamicData(nameof(HundredRandomStrings))]
    public void Delete_Undo_works (string text) {
        var ed = LineEdit.FromString(text);
        Random r = new(0);
        var count = r.Next(ed.Length);
        var at = r.Next(ed.Length + 1 - count);
        ed.At = at;
        ed.Delete(count);
        ed.Undo();
        AreEqual(at, ed.At);
        IsEq(text, ed);
    }

    [TestMethod]
    [DynamicData(nameof(HundredRandomStrings))]
    public void Insert_Span_works (string text) {
        var ed = LineEdit.FromString(text);
        Random r = new(0);
        Span<byte> span = stackalloc byte[100];
        for (var i = 0; i < 100; ++i) {
            var at = r.Next(ed.Length + 1);
            var spanLength = 1 + r.Next(span.Length);
            for (var p = 0; p < spanLength; ++p)
                span[p] = (byte)LineEdit_empty.ascii[r.Next(LineEdit_empty.ascii.Length)];

            ed.At = at;
            ed.Insert(span[0..spanLength]);
            text = text.Insert(at, Encoding.ASCII.GetString(span[0..spanLength]));
            IsEq(text, ed);
        }
    }

    [TestMethod]
    [DynamicData(nameof(HundredRandomStrings))]
    public void Insert_Span_Undo_works (string text) {
        var ed = LineEdit.FromString(text);
        Random r = new(0);
        Span<byte> span = stackalloc byte[100];
        for (var i = 0; i < 100; ++i) {
            var at = r.Next(ed.Length + 1);
            var spanLength = 1 + r.Next(span.Length);
            for (var p = 0; p < spanLength; ++p)
                span[p] = (byte)LineEdit_empty.ascii[r.Next(LineEdit_empty.ascii.Length)];
            ed.At = at;
            ed.Insert(span[0..spanLength]);
            ed.Undo();
            AreEqual(at, ed.At);
            IsEq(text, ed);
        }
    }

    [TestMethod]
    [DynamicData(nameof(HundredRandomStrings))]
    public void Insert_works (string text) {
        var ed = LineEdit.FromString(text);
        Random r = new(0);
        for (var i = 0; i < 100; ++i) {
            var at = r.Next(ed.Length + 1);
            var b = (byte)LineEdit_empty.ascii[r.Next(LineEdit_empty.ascii.Length)];
            ed.At = at;
            ed.Insert(b);
            text = text.Insert(at, $"{(char)b}");
            IsEq(text, ed);
        }
    }

    [TestMethod]
    [DynamicData(nameof(HundredRandomStrings))]
    public void Insert_Undo_works (string text) {
        var ed = LineEdit.FromString(text);
        Random r = new(0);
        for (var i = 0; i < 100; ++i) {
            ed.At = r.Next(ed.Length + 1);
            ed.Insert((byte)LineEdit_empty.ascii[r.Next(LineEdit_empty.ascii.Length)]);
            ed.Undo();
            IsEq(text, ed);
        }
    }

    [TestMethod]
    [DynamicData(nameof(HundredRandomStrings))]
    public void Backspace_works (string text) {
        var ed = LineEdit.FromString(text);
        Random r = new(0);
        var count = r.Next(ed.Length);
        var at = r.Next(ed.Length + 1 - count);
        ed.At = at + count;
        ed.Backspace(count);
        IsEq(text.Remove(at, count), ed);
        AreEqual(at, ed.At);
    }

    [TestMethod]
    [DynamicData(nameof(HundredRandomStrings))]
    public void Backspace_Undo_works (string text) {
        var ed = LineEdit.FromString(text);
        Random r = new(0);
        var count = r.Next(ed.Length);
        var at = r.Next(ed.Length + 1 - count);
        ed.At = at + count;
        ed.Backspace(count);
        ed.Undo();
        IsEq(text, ed);
        AreEqual(at + count, ed.At);
    }

    static void IsEq (string expected, LineEdit ed) {
        var actual = ed.GetCopy();
        AreEqual(expected.Length, actual.Length);
        for (var i = 0; i < expected.Length; ++i)
            AreEqual((byte)expected[i], actual[i], "expected ed[{0}] = '{1}', not '{2}'", i, expected[i], (char)actual[i]);
    }

    static string RandomAsciiString (Random random, int length) {
        const int MaxStackAlloc = 4096;
        if (0 == length)
            return string.Empty;
        var bytes = MaxStackAlloc < length ? new byte[length] : stackalloc byte[length];
        // Random.NextBytes is an alternative, but still needs a loop to constrain values to ascii.
        for (var i = 0; i < length; ++i)
            bytes[i] = (byte)random.Next(' ', '~' + 1);
        return Encoding.ASCII.GetString(bytes);
    }

    public static IEnumerable<object[]> HundredRandomStrings {
        get {
            List<object[]> objects = new();
            Random r = new(0);
            for (var i = 0; i < 100; ++i)
                objects.Add(new object[] { RandomAsciiString(r, r.Next(10, 100)) });
            return objects;
        }
    }
}
