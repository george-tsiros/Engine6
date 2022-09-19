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

}