﻿namespace Linear;
using System;

public static class Maths {
#if NET7_0
    public const double dTau = double.Tau;
    public const float fTau = float.Tau;
    public const double dPi = double.Pi;
    public const float fPi = float.Pi;
    public static double DoubleMin (double a, double b) => double.Min(a, b);
    public static double DoubleMax (double a, double b) => double.Max(a, b);
    public static int IntMin (int a, int b) => int.Min(a, b);
    public static int IntMax (int a, int b) => int.Max(a, b);
    public static double DoubleSqrt (double a) => double.Sqrt(a);
    public static double DoubleAbs (double a) => double.Abs(a);
    public static double DoubleFloor (double a) => double.Floor(a);
    public static double DoubleRound (double a, int digits) => double.Round(a, digits);
    public static (double, double) DoubleSinCos (double angle) => double.SinCos(angle);
    public static double DoubleCos (double angle) => double.Cos(angle);
    public static double DoubleSin (double angle) => double.Sin(angle);
    public static double DoubleTan (double angle) => double.Tan(angle);
    public static int IntClamp (int value, int min, int max) => int.Clamp(value, min, max);
    public static float FloatClamp (float value, float min, float max) => float.Clamp(value, min, max);
    public static double DoubleClamp (double value, double min, double max) => double.Clamp(value, min, max);
    public static (long quotient, long remainder) LongDivRem (long a, long b) => long.DivRem(a, b);
#elif NET6_0
    public const double dTau = 2 * Math.PI;
    public const float fTau = (float)(2 * Math.PI);
    public const double dPi = Math.PI;
    public const float fPi = (float)Math.PI;
    public static double DoubleMin (double a, double b) => Math.Min(a, b);
    public static double DoubleMax (double a, double b) => Math.Max(a, b);
    public static int IntMin (int a, int b) => Math.Min(a, b);
    public static int IntMax (int a, int b) => Math.Max(a, b);
    public static float FloatSqrt (float a) => (float)Math.Sqrt(a);
    public static double DoubleSqrt (double a) => Math.Sqrt(a);
    public static int IntAbs (int a) => Math.Abs(a);
    public static double DoubleAbs (double a) => Math.Abs(a);
    public static double DoubleFloor (double a) => Math.Floor(a);
    public static double DoubleRound (double a, int digits) => Math.Round(a, digits);
    public static double DoubleRound (double a) => Math.Round(a);
    public static float FloatRound (float a) => (float)Math.Round(a);
    public static (double, double) DoubleSinCos (double angle) => Math.SinCos(angle);
    public static double DoubleCos (double angle) => Math.Cos(angle);
    public static double DoubleSin (double angle) => Math.Sin(angle);
    public static double DoubleTan (double angle) => Math.Tan(angle);
    public static int IntClamp (int value, int min, int max) => Math.Clamp(value, min, max);
    public static float FloatClamp (float value, float min, float max) => Math.Clamp(value, min, max);
    public static double DoubleClamp (double value, double min, double max) => Math.Clamp(value, min, max);
    public static (int quotient, int remainder) IntDivRem (int a, int b) => Math.DivRem(a, b);
    public static (long quotient, long remainder) LongDivRem (long a, long b) => Math.DivRem(a, b);
#endif
}
