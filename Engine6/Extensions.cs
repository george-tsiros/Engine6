namespace Engine;

using System;
using System.Text.RegularExpressions;
using System.Reflection;

static class Extensions {
    internal static double NextDouble (this Random self, double min, double max) => (max - min) * self.NextDouble() + min;
    internal static float NextFloat (this Random self, double min, double max) => (float)NextDouble(self, min, max);

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
