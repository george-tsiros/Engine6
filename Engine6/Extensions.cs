namespace Engine;

using System;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Numerics;
using static Linear.Maths;
using Linear;

static class Extensions {
    internal static double NextDouble (this Random self, double min, double max) => (max - min) * self.NextDouble() + min;
    internal static float NextFloat (this Random self, double min, double max) => (float)NextDouble(self, min, max);

    internal static string ToEng (this double self) {
        if (self > 1e9)
            return $"{DoubleRound(self / 1e9, 2)} G";
        if (self > 1e6)
            return $"{DoubleRound(self / 1e6, 2)} M";
        if (self > 1e3)
            return $"{DoubleRound(self / 1e3, 2)} K";
        if (self > 1)
            return $"{DoubleRound(self, 2)}";
        if (self > 1e-3)
            return $"{DoubleRound(self * 1e3, 2)} m";
        return $"{DoubleRound(self * 1e6, 2)} u";
    }


    internal static FieldInfo GetEnumFieldInfo (this Assembly self, string s) {
        var parts = s.Split('.');
        if (parts.Length != 2)
            return null;
        const BindingFlags publicStaticIgnoreCase = BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase;
        return self.GetType($"GLFW.{parts[0]}", false, true)?.GetField(parts[1], publicStaticIgnoreCase);
    }


    internal static bool TryMatch (this Regex self, string input, out Match match) {
        var m = self.Match(input);
        match = m.Success ? m : null;
        return m.Success;
    }
}
