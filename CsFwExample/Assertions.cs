namespace CsFwExample;

using System.Diagnostics;

static class Assertions {
    private static void AssertStopwatchFrequencyRepresentableAsDouble () {
        Debug.Assert(Stopwatch.Frequency.ToString() == ((double)Stopwatch.Frequency).ToString());
    }
}
