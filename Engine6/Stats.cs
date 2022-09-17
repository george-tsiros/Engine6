using System;

namespace Engine6;
class Stats {
    private int index = -1;
    private int count;
    private readonly double[] data;
    private double mean;
    private bool valid;
    public Stats (int count) {
        if (count < 2)
            throw new ArgumentException("must be > 1", nameof(count));
        data = new double[count];
    }
    public void AddDatum (double value) {
        if (++index == data.Length)
            index = 0;
        if (count < data.Length)
            ++count;
        data[index] = value;
        valid = false;
    }
    public double Mean {
        get {
            if (!valid) {
                if (count == 0)
                    return double.NaN;
                var sum = 0.0;
                for (var i = 0; i < count; i++)
                    sum += data[i];
                mean = sum / count;
                valid = true;
            }
            return mean;
        }
    }
}
