namespace CsFwExample;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

struct TimePoint {
    public double millis;
    public Point p;
    unsafe public static TimePoint FromFileStream (FileStream f) {
        const int structSize = sizeof(long) + 2 * sizeof(int);
        var bytes = new byte[structSize];
        var read = f.Read(bytes, 0, structSize);
        if (read != structSize)
            throw new ApplicationException($"read {read} instead of {structSize} bytes");
        var millis = BitConverter.ToDouble(bytes, 0);
        var x = BitConverter.ToInt32(bytes, sizeof(long));
        var y = BitConverter.ToInt32(bytes, sizeof(long) + sizeof(int));
        return new TimePoint { millis = millis, p = new Point(x, y) };
    }
    public static IEnumerable<TimePoint> FromFile (string filename) {
        using (var f = File.OpenRead(filename))
            while (f.Position != f.Length)
                yield return TimePoint.FromFileStream(f);
    }
}
