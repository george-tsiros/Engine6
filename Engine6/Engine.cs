using Engine6;
using System.Diagnostics;
foreach (var f in new double[] { 1, 1.1, 1.1111111111111, 1e9, 2e9, 3e9, 4e9, 5e9, 6e9, 7e9, 8e9, 9e9 }) {
    Debug.WriteLine(f);
    var g = f;
    for (var i = 0; i < 10; ++i)
        Debug.Write($"    {g = double.BitIncrement(g)}\n");
}

using Experiment window = new();
window.Run();
