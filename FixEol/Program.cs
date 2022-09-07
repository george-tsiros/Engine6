namespace FixEol;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

class Program {
    static readonly string[] extensions = ".cs,.txt,.cap,.json,.vert,.frag,.csproj".Split(',');

    static bool HasKnownExtension(string f) =>
        Array.Exists(extensions, e => string.Equals(Path.GetExtension(f), e, StringComparison.OrdinalIgnoreCase));

    static bool IsNotInOutputDir (string f) =>
        f.IndexOf(@"\obj\", StringComparison.OrdinalIgnoreCase) < 0 &&
        f.IndexOf(@"\properties\", StringComparison.OrdinalIgnoreCase) < 0 &&
        f.IndexOf(@"\bin\", StringComparison.OrdinalIgnoreCase) < 0;

    static bool HasBadEol (string f) {
        const int BufferLength = 4096;
        var buffer = new byte[BufferLength];
        using var r = File.OpenRead(f);
        while (r.Position< r.Length) {
            var read = r.Read(buffer, 0, BufferLength);
            if (0 == read)
                return false;
            if (0 <= Array.IndexOf(buffer, '\r', 0, read))
                return true;
        }
        return false;
    }

    static bool NeedsFix (string f) => HasKnownExtension(f) && IsNotInOutputDir(f) && HasBadEol(f);

    static void Main () =>
        Parallel.ForEach(Array.FindAll(Directory.GetFiles(".", "*.*", SearchOption.AllDirectories), NeedsFix), Fix);

    static void Fix (string filepath) {
        var lines = File.ReadAllLines(filepath);
        using var f = new StreamWriter(filepath, false, Encoding.ASCII) { NewLine = "\n" };
        foreach (var line in lines)
            f.WriteLine(line);
    }
}
