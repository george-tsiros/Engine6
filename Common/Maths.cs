namespace Common;

using System;

public static class Maths {
#if NET7_0
    public const double dPi = double.Pi;
    public const double dTau = double.Tau;
    public const float fPi = float.Pi;
    public const float fTau = float.Tau;
    public static (double, double) DoubleSinCos (double angle) => double.SinCos(angle);
    public static (float, float) SingleSinCos (float angle) => float.SinCos(angle);
    public static (long quotient, long remainder) Int64DivRem (long a, long b) => long.DivRem(a, b);
    public static (ulong quotient, ulong remainder) UInt64DivRem (ulong a, ulong b) => ulong.DivRem(a, b);
    public static double DoubleAbs (double a) => double.Abs(a);
    public static double DoubleClamp (double value, double min, double max) => double.Clamp(value, min, max);
    public static double DoubleCos (double angle) => double.Cos(angle);
    public static double DoubleFloor (double a) => double.Floor(a);
    public static double DoubleMax (double a, double b) => double.Max(a, b);
    public static double DoubleMin (double a, double b) => double.Min(a, b);
    public static double DoubleRound (double a) => double.Round(a);
    public static double DoubleRound (double a, int digits) => double.Round(a, digits);
    public static double DoubleSin (double angle) => double.Sin(angle);
    public static double DoubleSqrt (double a) => double.Sqrt(a);
    public static double DoubleTan (double angle) => double.Tan(angle);
    public static float SingleClamp (float value, float min, float max) => float.Clamp(value, min, max);
    public static float SingleRound (float a) => float.Round(a);
    public static float SingleCos (float angle) => float.Cos(angle);
    public static float SingleSin (float angle) => float.Sin(angle);
    public static float SingleSqrt (float a) => float.Sqrt(a);
    public static float SingleTan (float angle) => float.Tan(angle);
    public static int Int32Abs (int a) => int.Abs(a);
    public static int Int32Clamp (int value, int min, int max) => int.Clamp(value, min, max);
    public static int Int32Max (int a, int b) => int.Max(a, b);
    public static int Int32Min (int a, int b) => int.Min(a, b);
    public static long Int64Max (long a, long b) => long.Max(a, b);
#else
    public const double dPi = Math.PI;
    public const double dTau = Math.Tau;
    public const float fPi = (float)Math.PI;
    public const float fTau = (float)Math.Tau;
    public static (double, double) DoubleSinCos (double angle) => Math.SinCos(angle);
    public static (float, float) SingleSinCos (float angle) => ((float)Math.Sin(angle), (float)Math.Cos(angle)); // :(
    public static (int quotient, int remainder) Int32DivRem (int a, int b) => Math.DivRem(a, b);
    public static (uint quotient, uint remainder) UInt32DivRem (uint a, uint b) => Math.DivRem(a, b);
    public static (long quotient, long remainder) Int64DivRem (long a, long b) => Math.DivRem(a, b);
    public static (ulong quotient, ulong remainder) UInt64DivRem (ulong a, ulong b) => Math.DivRem(a, b);
    public static double DoubleAbs (double a) => Math.Abs(a);
    public static double DoubleClamp (double value, double min, double max) => Math.Clamp(value, min, max);
    public static double DoubleCos (double angle) => Math.Cos(angle);
    public static double DoubleFloor (double a) => Math.Floor(a);
    public static double DoubleMax (double a, double b) => Math.Max(a, b);
    public static double DoubleMin (double a, double b) => Math.Min(a, b);
    public static double DoubleRound (double a) => Math.Round(a);
    public static double DoubleRound (double a, int digits) => Math.Round(a, digits);
    public static double DoubleSin (double angle) => Math.Sin(angle);
    public static double DoubleSqrt (double a) => Math.Sqrt(a);
    public static double DoubleTan (double angle) => Math.Tan(angle);
    public static float SingleAbs (float a) => Math.Abs(a);
    public static float SingleClamp (float value, float min, float max) => Math.Clamp(value, min, max);
    public static float SingleRound (float a, int digits) => (float)Math.Round(a, digits);
    public static float SingleRound (float a) => (float)Math.Round(a);
    public static float SingleCos (float angle) => (float)Math.Cos(angle);
    public static float SingleSin (float angle) => (float)Math.Sin(angle);
    public static float SingleSqrt (float a) => (float)Math.Sqrt(a);
    public static int Int32Abs (int a) => Math.Abs(a);
    public static int Int32Clamp (int value, int min, int max) => Math.Clamp(value, min, max);
    public static int Int32Max (int a, int b) => Math.Max(a, b);
    public static int Int32Min (int a, int b) => Math.Min(a, b);
#endif
}
