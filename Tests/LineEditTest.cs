namespace Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using Common;
using System;

[TestClass]
public class LineEditTest {
    [TestMethod]
    public void String_has_Length_0 () {
        LineEdit ed = new("a");
        AreEqual(1, ed.Length);
    }

    [TestMethod]
    public void String_has_At_0 () {
        LineEdit ed = new("a");
        AreEqual(0, ed.At);
    }

    [TestMethod]
    public void String_returns_character () {
        LineEdit ed = new("a");
        AreEqual((byte)'a', ed[0]);
    }

    [TestMethod]
    public void Empty_has_Length_0 () {
        LineEdit ed = new(string.Empty);
        AreEqual(0, ed.Length);
    }

    [TestMethod]
    public void Empty_has_At_0 () {
        LineEdit ed = new(string.Empty);
        AreEqual(0, ed.At);
    }

    [TestMethod]
    public void Empty_throws_on_access () {
        LineEdit ed = new(string.Empty);
        ThrowsException<ArgumentOutOfRangeException>(() => _ = ed[0]);
    }

    [TestMethod]
    public void Empty_throws_on_delete () {
        LineEdit ed = new(string.Empty);
        ThrowsException<InvalidOperationException>(ed.Delete);
    }

    [TestMethod]
    public void Undo_throws_on_less_than_1 () {
        LineEdit ed = new(string.Empty);
        ThrowsException<ArgumentOutOfRangeException>(() => ed.Undo(0));
        ThrowsException<ArgumentOutOfRangeException>(() => ed.Undo(-1));
    }

    [TestMethod]
    public void Can_delete_a_letter () {
        LineEdit ed = new("a");
        ed.Delete();
        AreEqual(0, ed.At);
        AreEqual(0, ed.Length);
    }

    [TestMethod]
    public void Can_move_caret () {
        LineEdit ed = new("a");
        ed.SetCaret(1);
        AreEqual(1, ed.At);
        ed.Undo();
        AreEqual(0, ed.At);
    }

    [TestMethod]
    public void Can_overwrite () {
        LineEdit ed = new("a");
        ed.Overwrite((byte)'b');
        AreEqual(1, ed.At);
        AreEqual(1, ed.Length);
        AreEqual((byte)'b', ed[0]);
        ed.Undo();
        AreEqual(0, ed.At);
        AreEqual(1, ed.Length);
        AreEqual((byte)'a', ed[0]);
    }

    [TestMethod]
    public void Can_insert_and_undo () {
        LineEdit ed = new("a");
        ed.Insert((byte)'f');
        AreEqual(1, ed.At);
        AreEqual(2, ed.Length);
        AreEqual((byte)'f', ed[0]);
        AreEqual((byte)'a', ed[1]);
        ed.Undo();
        AreEqual(0, ed.At);
        AreEqual(1, ed.Length);
        AreEqual((byte)'a', ed[0]);
    }

    [TestMethod]
    public void Overwrite_with_non_printable_throws () {
        LineEdit ed = new("a");
        ThrowsException<ArgumentOutOfRangeException>(() => ed.Overwrite((byte)'\n'));
    }

    [TestMethod]
    public void Overwrite_at_end_throws () {
        LineEdit ed = new("a");
        ed.Delete();
        AreEqual(0, ed.At);
        AreEqual(0, ed.Length);
        ThrowsException<InvalidOperationException>(() => ed.Overwrite((byte)'a'));
    }

    [TestMethod]
    public void Can_undo_delete () { 
        LineEdit ed = new("a");
        ed.Delete();
        ed.Undo();
        AreEqual(0, ed.At);
        AreEqual(1, ed.Length);
        AreEqual((byte)'a', ed[0]);
    }

}