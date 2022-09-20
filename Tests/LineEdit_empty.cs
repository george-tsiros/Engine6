namespace Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using Common;
using System;
using System.Linq;
using System.Collections.Generic;

[TestClass]
public class LineEdit_empty {
    [TestMethod]
    public void Length_eq_0 () =>
        AreEqual(0, new LineEdit().Length);

    [TestMethod]
    public void At_eq_0 () =>
        AreEqual(0, new LineEdit().At);

    [TestMethod]
    public void Throws_on_moving_caret () =>
        _ = ThrowsException<ArgumentOutOfRangeException>(() => new LineEdit().At = 1);

    [TestMethod]
    public void Throws_on_access () =>
        _ = ThrowsException<ArgumentOutOfRangeException>(() => _ = new LineEdit()[0]);

    [TestMethod]
    public void Throws_on_delete () =>
        _ = ThrowsException<ArgumentOutOfRangeException>(() => new LineEdit().Delete(1));

    [TestMethod]
    public void Throws_on_backspace () =>
        _ = ThrowsException<ArgumentOutOfRangeException>(() => new LineEdit().Backspace(1));

    [TestMethod]
    public void Throws_on_overwrite () =>
        _ = ThrowsException<InvalidOperationException>(() => new LineEdit().Overwrite(0x21));

    [TestMethod]
    public void Throws_on_insert_empty_sequence () =>
        _ = ThrowsException<ArgumentOutOfRangeException>(() => new LineEdit().Insert(Span<byte>.Empty));

    [TestMethod]
    public void Throws_on_non_printable () {
        for (int i = 0; i <= byte.MaxValue; ++i)
            if (i < ' ' || '~' < i)
                _ = ThrowsException<ArgumentOutOfRangeException>(() => new LineEdit().Insert((byte)i));
    }

    public static readonly char[] nonPrintable = { '\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007', '\u0008', '\u0009', '\u000a', '\u000b', '\u000c', '\u000d', '\u000e', '\u000f', '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d', '\u001e', '\u001f', '\u007f', '\u0080', '\u0081', '\u0082', '\u0083', '\u0084', '\u0085', '\u0086', '\u0087', '\u0088', '\u0089', '\u008a', '\u008b', '\u008c', '\u008d', '\u008e', '\u008f', '\u0090', '\u0091', '\u0092', '\u0093', '\u0094', '\u0095', '\u0096', '\u0097', '\u0098', '\u0099', '\u009a', '\u009b', '\u009c', '\u009d', '\u009e', '\u009f', '\u00a0', '\u00a1', '\u00a2', '\u00a3', '\u00a4', '\u00a5', '\u00a6', '\u00a7', '\u00a8', '\u00a9', '\u00aa', '\u00ab', '\u00ac', '\u00ad', '\u00ae', '\u00af', '\u00b0', '\u00b1', '\u00b2', '\u00b3', '\u00b4', '\u00b5', '\u00b6', '\u00b7', '\u00b8', '\u00b9', '\u00ba', '\u00bb', '\u00bc', '\u00bd', '\u00be', '\u00bf', '\u00c0', '\u00c1', '\u00c2', '\u00c3', '\u00c4', '\u00c5', '\u00c6', '\u00c7', '\u00c8', '\u00c9', '\u00ca', '\u00cb', '\u00cc', '\u00cd', '\u00ce', '\u00cf', '\u00d0', '\u00d1', '\u00d2', '\u00d3', '\u00d4', '\u00d5', '\u00d6', '\u00d7', '\u00d8', '\u00d9', '\u00da', '\u00db', '\u00dc', '\u00dd', '\u00de', '\u00df', '\u00e0', '\u00e1', '\u00e2', '\u00e3', '\u00e4', '\u00e5', '\u00e6', '\u00e7', '\u00e8', '\u00e9', '\u00ea', '\u00eb', '\u00ec', '\u00ed', '\u00ee', '\u00ef', '\u00f0', '\u00f1', '\u00f2', '\u00f3', '\u00f4', '\u00f5', '\u00f6', '\u00f7', '\u00f8', '\u00f9', '\u00fa', '\u00fb', '\u00fc', '\u00fd', '\u00fe', '\u00ff', };
    public static readonly char[] ascii = { ' ', '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ':', ';', '<', '=', '>', '?', '@', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '[', '\\', ']', '^', '_', '`', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '{', '|', '}', '~' };
    public static IEnumerable<object[]> InvalidBytes => nonPrintable.Select(c => new object[] { (byte)c });
    public static IEnumerable<object[]> AsciiBytes => ascii.Select(c => new object[] { (byte)c });

    [TestMethod]
    [DynamicData(nameof(AsciiBytes))]
    public void Can_insert_one_byte (byte value) {
        var ed = new LineEdit();
        ed.Insert(value);
        AreEqual(1, ed.Length);
        AreEqual(1, ed.At);
        AreEqual(value, ed[0]);
    }
}