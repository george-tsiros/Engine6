namespace Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using System.Numerics;
using System;

[TestClass]
public class Quaternions {
    const float hPi = (float)(Math.PI / 2);
    const float qPi = (float)(Math.PI / 4);
    const float pi = (float)Math.PI;

    [TestMethod]
    public void Identity_is_0_0_0_1 () {
        var q = Quaternion.Identity;
        AreEqual(0f, q.X);
        AreEqual(0f, q.Y);
        AreEqual(0f, q.Z);
        AreEqual(1f, q.W);
    }

    [TestMethod]
    public void YawPitchRollNone () {
        var q = Quaternion.CreateFromYawPitchRoll(0, 0, 0);
        AreEqual(q, Quaternion.Identity);
    }

    [TestMethod]
    [DataRow(hPi)]
    [DataRow(pi)]
    [DataRow(3 * hPi)]
    public void YawPi (float x) {
        var a = Quaternion.CreateFromYawPitchRoll(x, 0, 0);
        var b = Quaternion.CreateFromAxisAngle(Vector3.UnitY, x);
        AreEqual(a, b);
    }

    [TestMethod]
    public void PitchYaw_order_matters () {
        var yaw = Quaternion.CreateFromYawPitchRoll(hPi, 0, 0);
        var pitch = Quaternion.CreateFromYawPitchRoll(0, hPi, 0);
        var x = Vector3.UnitX;
        var yawThenPitch = Vector3.Transform(Vector3.Transform(x, yaw), pitch);
        Eq(Vector3.UnitY, yawThenPitch);
        var pitchThenYaw = Vector3.Transform(Vector3.Transform(x, pitch), yaw);
        Eq(-Vector3.UnitZ, pitchThenYaw);
    }

    [TestMethod]
    public void Concatenate_order () {
        var yaw = Quaternion.CreateFromYawPitchRoll(hPi, 0, 0);
        var pitch = Quaternion.CreateFromYawPitchRoll(0, hPi, 0);
        var x = Vector3.UnitX;
        var yawThenPitch = Vector3.Transform(Vector3.Transform(x, yaw), pitch);
        var q = Vector3.Transform(x, Quaternion.Concatenate(yaw, pitch));
        Eq(q, yawThenPitch);
        var notQ = Vector3.Transform(x, Quaternion.Concatenate(pitch, yaw));
        NotEq(notQ, yawThenPitch);
    }

    [TestMethod]
    public void Matrix_and_quaternion_yawpitchroll_are_eq () {
        var x = Vector3.UnitX;
        var m = Matrix4x4.CreateFromYawPitchRoll(hPi, 0, 0);
        var q = Quaternion.CreateFromYawPitchRoll(hPi, 0, 0);
        AreEqual(Vector3.Transform(x, m), Vector3.Transform(x, q));
    }

    [TestMethod]
    public void MyTestMethod () {
        //var system = new Vector3[] { Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ, };
        //var y = Quaternion.CreateFromYawPitchRoll(qPi, 0, 0);
        //var p = Quaternion.CreateFromYawPitchRoll(0, qPi, 0);
        //var r = Quaternion.CreateFromYawPitchRoll(0, 0, qPi);
        //var rotated = Array.ConvertAll(system, v => Vector3.Transform(v,
    }

    static void NotEq (Vector3 notExpected, Vector3 actual) {
        var xdiff = 1e-6f < Math.Abs(notExpected.X - actual.X);
        var ydiff = 1e-6f < Math.Abs(notExpected.Y - actual.Y);
        var zdiff = 1e-6f < Math.Abs(notExpected.Z - actual.Z);
        IsTrue(xdiff || ydiff || zdiff);
    }

    static void Eq (Vector3 expected, Vector3 actual, float delta = 1e-6f) {
        AreEqual(expected.X, actual.X, delta);
        AreEqual(expected.Y, actual.Y, delta);
        AreEqual(expected.Z, actual.Z, delta);
    }
}
